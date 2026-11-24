// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Immutable;
using System.Linq;
using FlexVerse.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test.SourceGenerators
{
    /// <summary>
    /// xunit coverage for the source-generator semantics documented by the
    /// legacy screen fixtures Source/FlexVerse.Test/Explorer/System/
    /// SgLifecycleMethodsTest.cs, SgPropertyGenericReactivityFlagTest.cs and
    /// SgPropertyNonGenericReactivityFlagTest.cs (namespace
    /// FlexVerse.Test.Explorer (legacy fixtures)). Those classes stay as compile-time
    /// documentation fixtures; these tests run FlexVerseGenerator in-process
    /// over the same property shapes:
    /// [Prop]/[State]/[InheritedContext] on Behavior/int,
    /// TweenBehavior/int (generic concrete), Behavior (non-generic
    /// abstract) and EffectBehavior (non-generic concrete), plus the
    /// ReconcileOnSet/ReconcileOnUpdate/MarkDirtyOnSet/MarkDirtyOnUpdate
    /// reactivity flags.
    ///
    /// Oracle substrates follow GeneratorsFlexVerseGeneratorTests: WidgetEmitter
    /// writes every member with a deterministic CodeBuilder layout, so the
    /// substrings below are the stable output contract.
    /// </summary>
    public class WidgetEmitterSemanticsTests
    {
        private const string LifecycleFixture = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class SampleWidget : Widget
                {
                    [Prop] private Behavior<int> GenericAbstractProp { get; set; }
                    [Prop] private TweenBehavior<int> GenericConcreteProp { get; set; }
                    [Prop] private Behavior NonGenericAbstractProp { get; set; }
                    [Prop] private EffectBehavior NonGenericConcreteProp { get; set; }

                    [State] private Behavior<int> GenericAbstractState { get; set; }
                    [State] private TweenBehavior<int> GenericConcreteState { get; set; }
                    [State] private Behavior NonGenericAbstractState { get; set; }
                    [State] private EffectBehavior NonGenericConcreteState { get; set; }

                    [InheritedContext] private Behavior<int> GenericAbstractInheritedContextProvider { get; set; } = 1;
                    [InheritedContext] private TweenBehavior<int> GenericConcreteInheritedContextProvider { get; set; } =
                        Animate.State(1);
                    [InheritedContext] private Behavior NonGenericAbstractInheritedContextProvider { get; set; } =
                        Effect.Create();
                    [InheritedContext] private EffectBehavior NonGenericConcreteInheritedContextProvider { get; set; } =
                        Effect.Create();

                    [InheritedContext] private Behavior<int> GenericAbstractInheritedContextConsumer { get; set; }
                    [InheritedContext] private TweenBehavior<int> GenericConcreteInheritedContextConsumer { get; set; }
                    [InheritedContext] private Behavior NonGenericAbstractInheritedContextConsumer { get; set; }
                    [InheritedContext] private EffectBehavior NonGenericConcreteInheritedContextConsumer { get; set; }
                }
            }
            """;

        private const string GenericReactivityFixture = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class SampleWidget : Widget
                {
                    [State(ReconcileOnSet = true)] private Behavior<int> AbstractReconcileOnSetState { get; set; }
                    [State(ReconcileOnUpdate = true)] private Behavior<int> AbstractReconcileOnUpdateState { get; set; }
                    [State(MarkDirtyOnSet = true)] private Behavior<int> AbstractMarkDirtyOnSetState { get; set; }
                    [State(MarkDirtyOnUpdate = true)] private Behavior<int> AbstractMarkDirtyOnUpdateState { get; set; }

                    [Prop(ReconcileOnSet = true)] private Behavior<int> AbstractReconcileOnSetProp { get; set; }
                    [Prop(ReconcileOnUpdate = true)] private Behavior<int> AbstractReconcileOnUpdateProp { get; set; }
                    [Prop(MarkDirtyOnSet = true)] private Behavior<int> AbstractMarkDirtyOnSetProp { get; set; }
                    [Prop(MarkDirtyOnUpdate = true)] private Behavior<int> AbstractMarkDirtyOnUpdateProp { get; set; }

                    [InheritedContext(ReconcileOnSet = true)] private Behavior<int> AbstractReconcileOnSetInheritedContext { get; set; }
                    [InheritedContext(ReconcileOnUpdate = true)] private Behavior<int> AbstractReconcileOnUpdateInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnSet = true)] private Behavior<int> AbstractMarkDirtyOnSetInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnUpdate = true)] private Behavior<int> AbstractMarkDirtyOnUpdateInheritedContext { get; set; }

                    [State(ReconcileOnSet = true)] private TweenBehavior<int> ConcreteReconcileOnSetState { get; set; }
                    [State(ReconcileOnUpdate = true)] private TweenBehavior<int> ConcreteReconcileOnUpdateState { get; set; }
                    [State(MarkDirtyOnSet = true)] private TweenBehavior<int> ConcreteMarkDirtyOnSetState { get; set; }
                    [State(MarkDirtyOnUpdate = true)] private TweenBehavior<int> ConcreteMarkDirtyOnUpdateState { get; set; }

                    [Prop(ReconcileOnSet = true)] private TweenBehavior<int> ConcreteReconcileOnSetProp { get; set; }
                    [Prop(ReconcileOnUpdate = true)] private TweenBehavior<int> ConcreteReconcileOnUpdateProp { get; set; }
                    [Prop(MarkDirtyOnSet = true)] private TweenBehavior<int> ConcreteMarkDirtyOnSetProp { get; set; }
                    [Prop(MarkDirtyOnUpdate = true)] private TweenBehavior<int> ConcreteMarkDirtyOnUpdateProp { get; set; }

                    [InheritedContext(ReconcileOnSet = true)] private TweenBehavior<int> ConcreteReconcileOnSetInheritedContext { get; set; }
                    [InheritedContext(ReconcileOnUpdate = true)] private TweenBehavior<int> ConcreteReconcileOnUpdateInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnSet = true)] private TweenBehavior<int> ConcreteMarkDirtyOnSetInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnUpdate = true)] private TweenBehavior<int> ConcreteMarkDirtyOnUpdateInheritedContext { get; set; }
                }
            }
            """;

        private const string NonGenericReactivityFixture = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class SampleWidget : Widget
                {
                    [State(ReconcileOnSet = true)] private Behavior AbstractReconcileOnSetState { get; set; }
                    [State(MarkDirtyOnSet = true)] private Behavior AbstractMarkDirtyOnSetState { get; set; }

                    [Prop(ReconcileOnSet = true)] private Behavior AbstractReconcileOnSetProp { get; set; }
                    [Prop(MarkDirtyOnSet = true)] private Behavior AbstractMarkDirtyOnSetProp { get; set; }

                    [InheritedContext(ReconcileOnSet = true)] private Behavior AbstractReconcileOnSetInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnSet = true)] private Behavior AbstractMarkDirtyOnSetInheritedContext { get; set; }

                    [State(ReconcileOnSet = true)] private EffectBehavior ConcreteReconcileOnSetState { get; set; }
                    [State(MarkDirtyOnSet = true)] private EffectBehavior ConcreteMarkDirtyOnSetState { get; set; }

                    [Prop(ReconcileOnSet = true)] private EffectBehavior ConcreteReconcileOnSetProp { get; set; }
                    [Prop(MarkDirtyOnSet = true)] private EffectBehavior ConcreteMarkDirtyOnSetProp { get; set; }

                    [InheritedContext(ReconcileOnSet = true)] private EffectBehavior ConcreteReconcileOnSetInheritedContext { get; set; }
                    [InheritedContext(MarkDirtyOnSet = true)] private EffectBehavior ConcreteMarkDirtyOnSetInheritedContext { get; set; }
                }
            }
            """;

        private const string DefaultFlagsFixture = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class SampleWidget : Widget
                {
                    [Prop] private Behavior<int> PlainProp { get; set; }
                    [State] private Behavior<int> PlainState { get; set; }
                }
            }
            """;

        private static List<Diagnostic> Errors(ImmutableArray<Diagnostic> diagnostics) =>
            diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        private static void AssertNoErrors(ImmutableArray<Diagnostic> diagnostics)
        {
            var errors = Errors(diagnostics);
            Assert.True(
                errors.Count == 0,
                "Expected zero compilation errors, got:\n"
                    + string.Join("\n", errors.Select(d => $"{d.Id}: {d.GetMessage()}"))
            );
        }

        [Fact]
        public void AllLifecyclePropertyShapes_CompilesWithoutErrors_AfterGeneration()
        {
            var (diagnostics, _) = GeneratorRunner.Run(LifecycleFixture);
            AssertNoErrors(diagnostics);
        }

        [Fact]
        public void GenericReactivityPropertyShapes_CompilesWithoutErrors_AfterGeneration()
        {
            var (diagnostics, _) = GeneratorRunner.Run(GenericReactivityFixture);
            AssertNoErrors(diagnostics);
        }

        [Fact]
        public void NonGenericReactivityPropertyShapes_CompilesWithoutErrors_AfterGeneration()
        {
            var (diagnostics, _) = GeneratorRunner.Run(NonGenericReactivityFixture);
            AssertNoErrors(diagnostics);
        }

        [Fact]
        public void PropAndStateShapes_EmitTypedBackingFieldChangingAndChanged_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            (string camel, string pascal, string type)[] shapes =
            [
                ("genericAbstractProp", "GenericAbstractProp", "Behavior<int>"),
                ("genericConcreteProp", "GenericConcreteProp", "TweenBehavior<int>"),
                ("nonGenericAbstractProp", "NonGenericAbstractProp", "Behavior"),
                ("nonGenericConcreteProp", "NonGenericConcreteProp", "EffectBehavior"),
                ("genericAbstractState", "GenericAbstractState", "Behavior<int>"),
                ("genericConcreteState", "GenericConcreteState", "TweenBehavior<int>"),
                ("nonGenericAbstractState", "NonGenericAbstractState", "Behavior"),
                ("nonGenericConcreteState", "NonGenericConcreteState", "EffectBehavior"),
            ];

            var propRegion = Region(generated, "Prop");
            var stateRegion = Region(generated, "State");
            foreach (var (camel, pascal, type) in shapes)
            {
                var region = camel.EndsWith("Prop") ? propRegion : stateRegion;
                Assert.Contains($"protected Action _{camel}Changing;", region);
                Assert.Contains($"protected Action _{camel}Changed;", region);
                Assert.Contains($"protected {type} _{camel};", region);
                Assert.Contains($"protected {type} _{pascal}", region);
            }
        }

        [Fact]
        public void PropAndStateSetters_SynchronizeAcrossLifecycleWithBehaviorActivation_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            Assert.Contains("#region Prop", generated);
            Assert.Contains("#region State", generated);
            Assert.Contains("switch (this.LifecycleState)", generated);
            Assert.Contains("case Widget.Lifecycle.Definition:", generated);
            Assert.Contains("case Widget.Lifecycle.Instance:", generated);
            Assert.Contains("@old.SemEquals(@new) == true) return;", generated);
            Assert.Contains("this.ActivateBehavior(@new);", generated);
            Assert.Contains("this.ReleaseBehavior(@old);", generated);
        }

        [Fact]
        public void InitMembersFromDefinition_UsesShapeSpecificBehaviorDefaults_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var initRegion = Region(generated, "InitMembersFromDefinition");
            Assert.Contains(
                "this.GenericAbstractProp = def.GenericAbstractProp ?? new FlexVerse.StaticBehavior<int>();",
                initRegion
            );
            Assert.Contains(
                "this.GenericConcreteProp = def.GenericConcreteProp ?? new TweenBehavior<int>();",
                initRegion
            );
            Assert.Contains(
                "this.NonGenericAbstractProp = def.NonGenericAbstractProp ?? FlexVerse.NullBehavior.Instance;",
                initRegion
            );
            Assert.Contains(
                "this.NonGenericConcreteProp = def.NonGenericConcreteProp ?? new EffectBehavior();",
                initRegion
            );
            Assert.Contains(
                "this.GenericAbstractState = def.GenericAbstractState ?? new FlexVerse.StaticBehavior<int>();",
                initRegion
            );
            Assert.Contains(
                "this.GenericConcreteState = def.GenericConcreteState ?? new TweenBehavior<int>();",
                initRegion
            );
            Assert.Contains(
                "this.NonGenericAbstractState = def.NonGenericAbstractState ?? FlexVerse.NullBehavior.Instance;",
                initRegion
            );
            Assert.Contains(
                "this.NonGenericConcreteState = def.NonGenericConcreteState ?? new EffectBehavior();",
                initRegion
            );
        }

        [Fact]
        public void ApplyPropsFromDefinition_AppliesPropsOnly_NotStates_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var applyRegion = Region(generated, "ApplyPropsFromDefinition");
            Assert.Contains("this.GenericAbstractProp = genericAbstractPropBehavior;", applyRegion);
            Assert.Contains(
                "this.NonGenericConcreteProp = nonGenericConcretePropBehavior;",
                applyRegion
            );
            Assert.DoesNotContain("GenericAbstractState", applyRegion);
            Assert.DoesNotContain("NonGenericConcreteState", applyRegion);
        }

        [Fact]
        public void CloneMembersFromDefinition_ClonesWithExactPropertyType_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var cloneRegion = Region(generated, "CloneMembersFromDefinition");
            Assert.Contains("CloneUntyped() as Behavior<int>;", cloneRegion);
            Assert.Contains("CloneUntyped() as TweenBehavior<int>;", cloneRegion);
            Assert.Contains("CloneUntyped() as Behavior;", cloneRegion);
            Assert.Contains("CloneUntyped() as EffectBehavior;", cloneRegion);
        }

        [Fact]
        public void ReleaseBehaviors_ReleasesPropsAndStates_ButNotInheritedContext_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var releaseRegion = Region(generated, "ReleaseBehaviors");
            Assert.Contains(
                "var oldGenericAbstractProp = this._genericAbstractProp;",
                releaseRegion
            );
            Assert.Contains("this.ReleaseBehavior(oldGenericAbstractProp);", releaseRegion);
            Assert.Contains(
                "var oldNonGenericConcreteState = this._nonGenericConcreteState;",
                releaseRegion
            );
            Assert.Contains("this.ReleaseBehavior(oldNonGenericConcreteState);", releaseRegion);
            Assert.DoesNotContain("InheritedContext", releaseRegion);
        }

        [Fact]
        public void InheritedContextProvider_RegistersProviderAndPreservesFieldInitializer_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var contextRegion = Region(generated, "InheritedContext");
            Assert.Contains(
                "protected Behavior<int> _genericAbstractInheritedContextProvider = 1;",
                contextRegion
            );
            Assert.Contains(
                "this.Engine.InheritedContextManager.RegisterProvider(this, \"GenericAbstractInheritedContextProvider\", @new);",
                contextRegion
            );
            Assert.Contains(
                "protected void _UpdateGenericAbstractInheritedContextProviderFromAncestor(Behavior<int> value)",
                contextRegion
            );
            Assert.Contains(
                "TweenBehavior<int> _genericConcreteInheritedContextProvider",
                contextRegion
            );
            Assert.Contains(
                "EffectBehavior _nonGenericConcreteInheritedContextProvider",
                contextRegion
            );

            var deinitRegion = Region(generated, "DeinitInheritedContext");
            Assert.Contains(
                "this.Engine.InheritedContextManager.ReleaseInheritedContext(this, \"GenericAbstractInheritedContextProvider\");",
                deinitRegion
            );
        }

        [Fact]
        public void InheritedContextConsumer_SubscribesWithoutRegistering_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(LifecycleFixture);

            var initRegion = Region(generated, "InitInheritedContext");
            Assert.Contains(
                "this.Engine.InheritedContextManager.SubscribeConsumer(this, \"GenericAbstractInheritedContextConsumer\", \"int\");",
                initRegion
            );

            var contextRegion = Region(generated, "InheritedContext");
            Assert.Contains(
                "protected void _UpdateGenericAbstractInheritedContextConsumerFromAncestor(Behavior<int> value)",
                contextRegion
            );

            Assert.Contains(
                "InheritedContextManager.RegisterProvider(this, \"GenericAbstractInheritedContextConsumer\"",
                contextRegion
            );
        }

        [Fact]
        public void GenericReactivityFlags_WireReconcileAndMarkDirtySignals_Oracle()
        {
            var (diagnostics, generated) = GeneratorRunner.Run(GenericReactivityFixture);
            AssertNoErrors(diagnostics);

            var reconciledSet = SetterOf(generated, "_AbstractReconcileOnSetProp");
            Assert.Contains("System.Action onSetAction = () =>", reconciledSet);
            Assert.Contains("this.Engine.ReconcileChildren(this);", reconciledSet);
            Assert.DoesNotContain("MarkNodeAsDirty", reconciledSet);
            Assert.Contains("this.Engine.Post(new SignalInfo(this, onSetAction));", reconciledSet);

            var reconciledUpdate = SetterOf(generated, "_AbstractReconcileOnUpdateProp");
            Assert.Contains(
                "@new.OnUpdate += (_) => this.Engine.Post(new SignalInfo(this, onUpdateAction));",
                reconciledUpdate
            );
            Assert.Contains("this.Engine.ReconcileChildren(this);", reconciledUpdate);
            Assert.DoesNotContain("onSetAction", reconciledUpdate);

            var dirtySet = SetterOf(generated, "_AbstractMarkDirtyOnSetProp");
            Assert.Contains("System.Action onSetAction = () =>", dirtySet);
            Assert.Contains("this.Engine.MarkNodeAsDirty(this);", dirtySet);
            Assert.DoesNotContain("ReconcileChildren", dirtySet);

            var dirtyUpdate = SetterOf(generated, "_AbstractMarkDirtyOnUpdateProp");
            Assert.Contains(
                "@new.OnUpdate += (_) => this.Engine.Post(new SignalInfo(this, onUpdateAction));",
                dirtyUpdate
            );
            Assert.Contains("this.Engine.MarkNodeAsDirty(this);", dirtyUpdate);
            Assert.DoesNotContain("ReconcileChildren", dirtyUpdate);

            Assert.Contains(
                "this.Engine.ReconcileChildren(this);",
                SetterOf(generated, "_ConcreteReconcileOnSetState")
            );
            Assert.Contains(
                "this.Engine.MarkNodeAsDirty(this);",
                SetterOf(generated, "_ConcreteMarkDirtyOnSetState")
            );
            Assert.Contains(
                "this.Engine.ReconcileChildren(this);",
                SetterOf(generated, "_AbstractReconcileOnSetInheritedContext")
            );
            Assert.Contains(
                "this.Engine.MarkNodeAsDirty(this);",
                SetterOf(generated, "_AbstractMarkDirtyOnSetInheritedContext")
            );
        }

        [Fact]
        public void NonGenericReactivityFlags_WireReconcileAndMarkDirtySignals_Oracle()
        {
            var (diagnostics, generated) = GeneratorRunner.Run(NonGenericReactivityFixture);
            AssertNoErrors(diagnostics);

            foreach (
                var property in new[]
                {
                    "_AbstractReconcileOnSetProp",
                    "_AbstractReconcileOnSetInheritedContext",
                    "_ConcreteReconcileOnSetProp",
                    "_ConcreteReconcileOnSetState",
                    "_ConcreteReconcileOnSetInheritedContext",
                }
            )
            {
                var setter = SetterOf(generated, property);
                Assert.Contains("System.Action onSetAction = () =>", setter);
                Assert.Contains("this.Engine.ReconcileChildren(this);", setter);
            }

            foreach (
                var property in new[]
                {
                    "_AbstractMarkDirtyOnSetProp",
                    "_AbstractMarkDirtyOnSetState",
                    "_AbstractMarkDirtyOnSetInheritedContext",
                    "_ConcreteMarkDirtyOnSetProp",
                    "_ConcreteMarkDirtyOnSetState",
                    "_ConcreteMarkDirtyOnSetInheritedContext",
                }
            )
            {
                var setter = SetterOf(generated, property);
                Assert.Contains("System.Action onSetAction = () =>", setter);
                Assert.Contains("this.Engine.MarkNodeAsDirty(this);", setter);
            }
        }

        [Fact]
        public void DefaultFlags_EmitNoReconcileOrDirtySignal_Oracle()
        {
            var (_, generated) = GeneratorRunner.Run(DefaultFlagsFixture);

            Assert.DoesNotContain("ReconcileChildren", generated);
            Assert.DoesNotContain("MarkNodeAsDirty", generated);
            Assert.DoesNotContain("SignalInfo", generated);
            Assert.Contains("this.ActivateBehavior(@new);", generated);
        }

        /// <summary>
        /// Extracts the body of a generated #region by name. Regions are
        /// unique per emitted widget, and each region closes before the next
        /// one opens.
        /// </summary>
        private static string Region(string generated, string name)
        {
            var marker = $"#region {name}";
            var start = generated.IndexOf(marker, StringComparison.Ordinal);
            Assert.True(start >= 0, $"Region '{name}' not found in generated source.");
            var end = generated.IndexOf("#endregion", start, StringComparison.Ordinal);
            Assert.True(end > start, $"Region '{name}' is not closed.");
            return generated[start..end];
        }

        /// <summary>
        /// Extracts the setter body of a synchronized property by its emitted
        /// declaration name (e.g. "_AbstractReconcileOnSetProp").
        /// </summary>
        private static string SetterOf(string generated, string propertyName)
        {
            var start = generated.IndexOf($" {propertyName}\n", StringComparison.Ordinal);
            Assert.True(start >= 0, $"Property '{propertyName}' not found in generated source.");
            var end = generated.IndexOf("\n        }\n", start, StringComparison.Ordinal);
            Assert.True(end > start, $"Setter of '{propertyName}' not terminated.");
            return generated[start..end];
        }
    }
}
