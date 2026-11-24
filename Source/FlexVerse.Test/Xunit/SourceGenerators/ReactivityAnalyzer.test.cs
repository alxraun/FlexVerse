// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators;
using FlexVerse.SourceGenerators.Widgets.Extraction;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test.SourceGenerators
{
    /// <summary>
    /// Diagnostics contract of the reactivity analyzer: FV0001 for invalid
    /// widget declarations, zero FV* diagnostics for valid ones. Runs
    /// FlexVerseGenerator in-process; fixture widgets compile against the real
    /// FlexVerse.dll (net48) plus RimWorld/Unity reference assemblies as
    /// metadata only.
    /// </summary>
    public class ReactivityAnalyzerTests
    {
        private const string ValidWidget = """
            using FlexVerse;

            namespace FVGen
            {
                public partial class SampleWidget : Widget
                {
                    [Prop] public Behavior<float> Speed { get; set; }
                    [State] public Behavior<float> Alpha { get; set; }
                }
            }
            """;

        private const string NonPartialWidget = """
            using FlexVerse;

            namespace FVGen
            {
                public class BrokenWidget : Widget
                {
                    [Prop] public Behavior<float> Speed { get; set; }
                }
            }
            """;

        [Fact]
        public void NonPartialWidgetClass_ReportsErrorFV0001_AndEmitsNoGeneratedSource()
        {
            var (diagnostics, generatedSource) = GeneratorRunner.Run(NonPartialWidget);

            var fv0001 = diagnostics
                .Where(d => d.Id == "FV0001")
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .ToList();
            Assert.Single(fv0001);
            Assert.Equal("FlexVerse", fv0001[0].Descriptor.Category);
            Assert.Contains("BrokenWidget", fv0001[0].GetMessage());

            Assert.True(
                string.IsNullOrEmpty(generatedSource),
                "A widget that failed the partial-keyword rule must not emit generated code."
            );
        }

        [Fact]
        public void ValidWidgetWithPropAndState_ProducesZeroFlexVerseDiagnostics()
        {
            var (diagnostics, _) = GeneratorRunner.Run(ValidWidget);

            var flexVerseDiagnostics = diagnostics
                .Where(d => d.Id.StartsWith("FV", StringComparison.Ordinal))
                .ToList();
            Assert.Empty(flexVerseDiagnostics);
        }
    }

    /// <summary>
    /// SCN-003: asserts the analyzer's own output — the Reactivity flags on
    /// extracted PropertyModel instances — for fixtures with and without
    /// ReconcileOnSet/ReconcileOnUpdate/MarkDirtyOnSet/MarkDirtyOnUpdate usage.
    /// </summary>
    public class ReactivityFlagTests
    {
        [Theory]
        [InlineData("return A();", "int A() => B(); int B() => Speed.Value;", true)]
        [InlineData("return A();", "int A() => Speed.Value + A();", true)]
        [InlineData("System.Func<int> later = () => Speed.Value; return 0;", "", false)]
        [InlineData("return Other.Read();", "FlagWidget Other; int Read() => Speed.Value;", false)]
        [InlineData("return Current;", "int Current => Speed.Value;", true)]
        [InlineData("int Local() => Speed.Value; return Local();", "", true)]
        [InlineData("int Local() => Speed.Value; return 0;", "", false)]
        public void HelperBoundary(string body, string helpers, bool expected)
        {
            var tree = CSharpSyntaxTree.ParseText(
                "class Behavior { public int Value { get; } } class FlagWidget { "
                    + "Behavior Speed { get; set; } int BuildRenderTree() { "
                    + body
                    + " } "
                    + helpers
                    + " }"
            );
            var compilation = CSharpCompilation.Create(
                "Boundary",
                new[] { tree },
                new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location) }
            );
            var defaults = ReactivityAnalyzer.AnalyzeClassDefaults(
                compilation.GetTypeByMetadataName("FlagWidget")!,
                compilation,
                CancellationToken.None
            );
            Assert.Equal(expected, defaults["Speed"].ReconcileOnUpdate);
        }

        private const string FlagWidget = """
            using System.Collections.Generic;
            using FlexVerse;

            namespace FVGen
            {
                public partial class FlagWidget : Widget
                {
                    [Prop] public Behavior<float> Speed { get; set; }
                    [Prop] public Behavior<float> Alpha { get; set; }
                    [Prop] public Behavior<float> Plain { get; set; }
                    [Prop(ReconcileOnSet = false, MarkDirtyOnUpdate = true)]
                    public Behavior<float> Forced { get; set; }
                    [Prop(MarkDirtyOnSet = true)] public new Behavior<Length> PaddingTop { get; set; }

                    protected override MeasureFunc MeasureFunc =>
                        (node, width, widthMode, height, heightMode) =>
                        {
                            var measured = Speed.Value;
                            return new Size(0, 0);
                        };

                    public override IEnumerable<Widget> BuildRenderTree()
                    {
                        var alphaBare = Alpha;
                        var alphaValue = Alpha.Value;
                        yield break;
                    }
                }
            }
            """;

        private static IReadOnlyDictionary<string, PropertyModel> ExtractProperties(
            out ImmutableArray<Diagnostic> diagnostics
        )
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                FlagWidget,
                new CSharpParseOptions(LanguageVersion.Latest)
            );
            var compilation = CSharpCompilation.Create(
                "FlexVerse.Generator.FlagFixtures",
                new[] { syntaxTree },
                TestPaths
                    .FlexVerseFixtureRefs()
                    .Select(r => MetadataReference.CreateFromFile(r))
                    .Cast<MetadataReference>(),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var symbol = compilation.GetTypeByMetadataName("FVGen.FlagWidget")!;
            Assert.NotNull(symbol);

            var builder = ImmutableArray.CreateBuilder<Diagnostic>();
            var model = WidgetExtractor.Build(symbol, compilation, CancellationToken.None, builder);
            diagnostics = builder.ToImmutable();

            return model.Properties.ToDictionary(p => p.Name, p => p);
        }

        [Fact]
        public void Analyzer_ClassifiesReactivityFlags_PerPropertyUsage()
        {
            var properties = ExtractProperties(out var diagnostics);

            Assert.Empty(diagnostics);

            var speed = properties["Speed"].Reactivity;
            Assert.False(speed.ReconcileOnSet);
            Assert.False(speed.ReconcileOnUpdate);
            Assert.True(speed.MarkDirtyOnSet);
            Assert.True(speed.MarkDirtyOnUpdate);

            var alpha = properties["Alpha"].Reactivity;
            Assert.True(alpha.ReconcileOnSet);
            Assert.True(alpha.ReconcileOnUpdate);
            Assert.False(alpha.MarkDirtyOnSet);
            Assert.False(alpha.MarkDirtyOnUpdate);

            var plain = properties["Plain"].Reactivity;
            Assert.False(plain.ReconcileOnSet);
            Assert.False(plain.ReconcileOnUpdate);
            Assert.False(plain.MarkDirtyOnSet);
            Assert.False(plain.MarkDirtyOnUpdate);
        }

        [Fact]
        public void ExplicitAttributeArguments_OverrideInferredDefaults()
        {
            var properties = ExtractProperties(out _);

            var forced = properties["Forced"].Reactivity;
            Assert.False(forced.ReconcileOnSet);
            Assert.False(forced.ReconcileOnUpdate);
            Assert.False(forced.MarkDirtyOnSet);
            Assert.True(forced.MarkDirtyOnUpdate);

            var padding = properties["PaddingTop"].Reactivity;
            Assert.False(padding.ReconcileOnSet);
            Assert.False(padding.ReconcileOnUpdate);
            Assert.True(padding.MarkDirtyOnSet);
            Assert.False(padding.MarkDirtyOnUpdate);
        }
    }
}
