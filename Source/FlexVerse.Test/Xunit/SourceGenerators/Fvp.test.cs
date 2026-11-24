using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using FlexVerse.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test.SourceGenerators
{
    public class FvpEmitterTests
    {
        private const string FixtureViewModel = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class HealthBarViewModel
                {
                    [FVP] public Behavior<float> Health { get; set; }
                }
            }
            """;

        private const string FixtureNonPartial = """
            using FlexVerse;

            namespace FVGen
            {
                public class NonPartialViewModel
                {
                    [FVP] public Behavior<float> Health { get; set; }
                }
            }
            """;

        private const string FixtureWidgetWithFvp = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class FvpWidget : Widget
                {
                    [FVP] public Behavior<float> Health { get; set; }
                }
            }
            """;

        [Fact]
        public void FvpViewModel_CompilesWithoutErrors_AfterGeneration()
        {
            var (diagnostics, _) = GeneratorRunner.Run(FixtureViewModel);

            var errors = diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();
            Assert.True(
                errors.Count == 0,
                "Expected zero compilation errors, got:\n"
                    + string.Join("\n", errors.Select(d => $"{d.Id}: {d.GetMessage()}"))
            );
        }

        [Fact]
        public void EmittedSource_ContainsFvpRegionWithPropShapedMembers_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureViewModel);

            Assert.Contains("#region FVP", generated);
            Assert.Contains("protected Behavior<float> _Health", generated);
            Assert.Contains("protected Action _healthChanging;", generated);
            Assert.Contains("protected Action _healthChanged;", generated);
            Assert.Contains("protected Behavior<float> _health", generated);
        }

        [Fact]
        public void EmittedSetter_MirrorsPropStateSemantics_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureViewModel);

            Assert.Contains("@old.SemEquals(@new) == true) return;", generated);
            Assert.Contains("\"M-FLEXVERSE-PROPERTY\"", generated);
            Assert.Contains("_healthChanging?.Invoke();", generated);
            Assert.Contains("_healthChanged?.Invoke();", generated);
            Assert.Contains("if (@old is not null) @new.UnionWith(@old);", generated);
            Assert.Contains("this.ActivateBehavior(@new);", generated);
            Assert.Contains(
                "FlexVerseRoot.Instance.BehaviorRegistry.Increment(behavior);",
                generated
            );
            Assert.Contains(
                "behavior.OnStart += () => FlexVerseRoot.Instance.BehaviorUpdater.Subscribe(behavior);",
                generated
            );
            Assert.Contains(
                "behavior.OnComplete += () => FlexVerseRoot.Instance.BehaviorUpdater.Unsubscribe(behavior);",
                generated
            );
            Assert.Contains("behavior.Activate();", generated);
            Assert.Contains(
                "FlexVerseRoot.Instance.BehaviorRegistry.Decrement(behavior)) behavior.Reset();",
                generated
            );
        }

        [Fact]
        public void EmittedSetter_OmitsWidgetLifecycleMachinery_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureViewModel);

            Assert.DoesNotContain("switch (this.LifecycleState)", generated);
            Assert.DoesNotContain("Engine.Post", generated);
            Assert.DoesNotContain("_isInitializing", generated);
        }

        [Fact]
        public void FvpReleaseNotifiesSlotRemovalBeforeReset()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureViewModel);

            Assert.Contains("var oldHealth = _health;", generated);
            Assert.Contains("if (oldHealth is not null)", generated);
            Assert.Contains("_healthChanging?.Invoke();", generated);
            Assert.Contains("_health = null;", generated);
            Assert.Contains("_healthChanged?.Invoke();", generated);
            Assert.Contains("this.ReleaseBehavior(oldHealth);", generated);
        }

        [Fact]
        public void WidgetReleaseNotifiesSlotRemovalBeforeReset()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureWidgetWithFvp);

            Assert.Contains("var oldHealth = this._health;", generated);
            Assert.Contains("if (oldHealth is not null)", generated);
            Assert.Contains("this._healthChanging?.Invoke();", generated);
            Assert.Contains("this._health = null;", generated);
            Assert.Contains("this._healthChanged?.Invoke();", generated);
            Assert.Contains("this.ReleaseBehavior(oldHealth);", generated);
        }

        [Fact]
        public void FvpGeneratesRefsBindWatchAndExplicitRelease()
        {
            var (diagnostics, generated) = GeneratorRunner.Run(FixtureViewModel);

            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("public HealthBarViewModelRefs Refs", generated);
            Assert.Contains("public readonly struct HealthRef(", generated);
            Assert.Contains("IPropertyRef<float>", generated);
            Assert.Contains("public BindRefs Bind", generated);
            Assert.Contains("public WatchRefs Watch", generated);
            Assert.Contains("public virtual void ReleaseFvp()", generated);
            Assert.Contains("EnsureFvpInitialized();", generated);
            Assert.Contains(
                "FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscriptions(this);",
                generated
            );
            Assert.Contains("public IPropertyRef this[string key]", generated);
        }

        [Fact]
        public void GenericFvpOwnerGeneratesTypedRefsWithoutErrors()
        {
            const string fixture = """
                using FlexVerse;
                namespace FVGen
                {
                    public partial class GenericModel<T>
                    {
                        [FVP] public Behavior<T> Current { get; set; }
                    }
                }
                """;

            var (diagnostics, generated) = GeneratorRunner.Run(fixture);

            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("GenericModel<T> o", generated);
            Assert.Contains("IPropertyRef<T>", generated);
        }

        [Fact]
        public void NonPartialType_ReportsFv0002_AndEmitsNothing()
        {
            var (diagnostics, generated) = GeneratorRunner.Run(FixtureNonPartial);

            Assert.Contains(
                diagnostics,
                d => d.Id == "FV0002" && d.GetMessage().Contains("NonPartialViewModel")
            );
            Assert.DoesNotContain("#region FVP", generated);
        }

        [Fact]
        public void WidgetWithFvpUsesWidgetLifecycle_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(FixtureWidgetWithFvp);

            Assert.Contains("#region FVP", generated);
            Assert.Contains("public override void ReleaseBehaviors()", generated);
            Assert.DoesNotContain("public virtual void ReleaseFvp()", generated);
        }

        [Fact]
        public void SealedFvpOwnerDoesNotEmitVirtualRelease()
        {
            const string fixture = """
                using FlexVerse;
                public sealed partial class SealedModel
                {
                    [FVP] public Behavior<int> Count { get; set; }
                }
                """;
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("public void ReleaseFvp()", generated);
        }

        [Fact]
        public void DerivedFvpOwnerChainsLifecycleAndReferencesBaseProperties()
        {
            const string fixture = """
                using FlexVerse;
                public partial class BaseModel
                {
                    [FVP] public Behavior<int> ParentCount { get; set; }
                }
                public partial class DerivedModel : BaseModel
                {
                    [FVP] public Behavior<int> ChildCount { get; set; }
                }
                """;
            var run = GeneratorRunner.Run(fixture, "FvpInheritance");
            Assert.DoesNotContain(run.Diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        }

        [Fact]
        public void InheritedOnlyOwnerGeneratesItsOwnTypedSurface()
        {
            const string fixture = """
                using FlexVerse;
                public partial class ParentModel
                {
                    [FVP] public Behavior<int> Count { get; set; }
                }
                public sealed partial class ChildModel : ParentModel
                {
                    public IPropertyRef Lookup() => Refs["Count"];
                }
                """;
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("ChildModelRefs", generated);
            Assert.Contains("protected override void ReleaseBehaviors()", generated);
        }

        [Fact]
        public void HiddenBaseSlotStillHasBaseLifecycle()
        {
            const string fixture = """
                using FlexVerse;
                public partial class ParentModel
                {
                    [FVP] public Behavior<int> Count { get; set; }
                }
                public partial class ChildModel : ParentModel
                {
                    [FVP] public new Behavior<int> Count { get; set; }
                }
                """;
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("base.ReleaseBehaviors();", generated);
        }

        [Theory]
        [InlineData("[FVP]")]
        [InlineData("")]
        public void OverrideReusesBaseSlotAndExposesReferences(string annotation)
        {
            var fixture = """
                using FlexVerse;
                public partial class ParentModel
                {
                    [FVP] public virtual Behavior<int> Count { get; set; }
                }
                public partial class ChildModel : ParentModel
                {
                    ANNOTATION public override Behavior<int> Count
                    {
                        get => base.Count;
                        set => base.Count = value;
                    }
                }
                """.Replace("ANNOTATION", annotation);
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("public new readonly struct CountRef(ChildModel o)", generated);
        }

        [Fact]
        public void BehaviorValueTypeComesFromBaseChainRatherThanConcreteGenericArguments()
        {
            const string fixture = """
                using FlexVerse;
                public class PairBehavior<TLeft, TRight> : StaticBehavior<int> { }
                public class CountBehavior : StaticBehavior<int> { }
                public sealed partial class Model<T> where T : class, new()
                {
                    [FVP] public PairBehavior<string, T> Pair { get; set; }
                    [FVP] public CountBehavior Count { get; set; }
                }
                """;
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("PairRef(Model<T> o) : IPropertyRef<int>", generated);
            Assert.Contains("CountRef(Model<T> o) : IPropertyRef<int>", generated);
        }

        [Fact]
        public void WidgetIndexerIncludesInheritedSlotsWithoutLocalProperties()
        {
            const string fixture = """
                using FlexVerse;
                public partial class ParentWidget : Widget
                {
                    [FVP] public Behavior<int> Count { get; set; }
                }
                public partial class ChildWidget : ParentWidget
                {
                    public IPropertyRef Lookup() => Refs["Count"];
                }
                """;
            var (diagnostics, _) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
        }

        [Theory]
        [InlineData("FVP")]
        [InlineData("Prop")]
        [InlineData("State")]
        public void WidgetOverrideInheritsItsBaseReactiveCategory(string category)
        {
            var fixture = """
                using FlexVerse;
                public partial class ParentWidget : Widget
                {
                    [CATEGORY] public virtual Behavior<int> Count { get; set; }
                }
                public partial class ChildWidget : ParentWidget
                {
                    public override Behavior<int> Count
                    {
                        get => base.Count;
                        set => base.Count = value;
                    }
                }
                """.Replace("CATEGORY", category);
            var (diagnostics, generated) = GeneratorRunner.Run(fixture);
            Assert.DoesNotContain(diagnostics, d => d.Severity == DiagnosticSeverity.Error);
            Assert.Contains("public new readonly struct CountRef(ChildWidget o)", generated);
        }

        [Fact]
        public void GeneratedLifecyclePreservesSharedOwnershipAndClosesInheritedSlots()
        {
            const string fixture = """
                using System;
                using FlexVerse;
                public partial class ParentModel
                {
                    [FVP] public Behavior<int> Count { get; set; }
                    public Behavior<int> ParentRead() => _Count;
                    public void ParentWrite(Behavior<int> value) => _Count = value;
                }
                public sealed partial class ChildModel : ParentModel
                {
                    [FVP] public new Behavior<int> Count { get; set; }
                    public Behavior<int> ChildRead() => _Count;
                    public void ChildWrite(Behavior<int> value) => _Count = value;
                }
                public static class Scenario
                {
                    public static string Run()
                    {
                        var shared = new StaticBehavior<int>();
                        var first = new ChildModel();
                        var second = new ChildModel();
                        first.ParentWrite(shared);
                        first.ChildWrite(shared);
                        second.ChildWrite(shared);
                        int releases = 0;
                        FlexVerseRoot.Instance.BehaviorRegistry.RegisterSubscription(
                            first, "probe", (Action)(() => { }), () => releases++);
                        first.ReleaseFvp();
                        first.ReleaseFvp();
                        if (releases != 1)
                            return $"releases after double ReleaseFvp: got {releases}, want 1";
                        if (shared.State != Behavior.Lifecycle.Active)
                            return $"shared.State after first release: got {shared.State}, want Active";
                        second.ReleaseFvp();
                        if (shared.State != Behavior.Lifecycle.Inactive)
                            return $"shared.State after last release: got {shared.State}, want Inactive";
                        try { first.ParentRead(); return "ParentRead after release: no ObjectDisposedException"; }
                        catch (ObjectDisposedException) { }
                        try { first.ChildRead(); return "ChildRead after release: no ObjectDisposedException"; }
                        catch (ObjectDisposedException) { }
                        return "";
                    }
                }
                """;
            var run = GeneratorRunner.Run(fixture, "FvpLifecycle_" + Guid.NewGuid().ToString("N"));
            using var stream = new MemoryStream();
            var emitted = run.UpdatedCompilation.Emit(stream);
            Assert.True(emitted.Success, string.Join("\n", emitted.Diagnostics));
            var assembly = System.Reflection.Assembly.Load(stream.ToArray());
            var verdict = (string)
                assembly.GetType("Scenario")!.GetMethod("Run")!.Invoke(null, null)!;
            Assert.True(verdict.Length == 0, "lifecycle scenario diverged at: " + verdict);
        }
    }
}
