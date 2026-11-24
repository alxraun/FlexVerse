// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Extraction;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test.SourceGenerators
{
    public class WidgetLayoutGeneratorTests
    {
        [Fact]
        public void DecorativePrefixesAndHiddenBaseName_CompileWithoutLayoutDispatch()
        {
            var result = GeneratorRunner.Run(
                """
                using FlexVerse;
                public partial class Decorative : Widget
                {
                    [Prop] public Behavior<float> BorderColor { get; set; }
                    [Prop] public Behavior<float> PaddingGlow { get; set; }
                    [Prop] public new Behavior<Length> Width { get; set; }
                }
                """
            );
            result.AssertNoErrors();
            Assert.DoesNotContain("StyleSet", result.GeneratedSource);
            Assert.DoesNotContain("Unsubscribe", result.GeneratedSource);
        }

        [Fact]
        public void LayoutSlots_KeepBaseIdentityThroughInheritanceAndOverrides()
        {
            var (model, diagnostics) = Extract(
                """
                namespace FlexVerse
                {
                    public abstract partial class Widget
                    {
                        [Prop] public virtual Behavior<Length> Width { get; set; }
                    }
                }
                public partial class Derived : FlexVerse.Widget
                {
                    public override FlexVerse.Behavior<FlexVerse.Length> Width { get; set; }
                }
                """,
                "Derived"
            );
            Assert.Empty(diagnostics);
            var width = Assert.Single(model.Properties);
            Assert.Equal(PropertyKind.Prop, width.Kind);
            Assert.Equal("FlexVerse.Widget", width.SlotOwner);
            Assert.True(width.IsOverride);
        }

        private static (WidgetModel, ImmutableArray<Diagnostic>) Extract(string source, string type)
        {
            var compilation = CSharpCompilation.Create(
                "LayoutExtraction",
                new[] { CSharpSyntaxTree.ParseText(source) },
                TestPaths.FlexVerseFixtureRefs().Select(r => MetadataReference.CreateFromFile(r)),
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();
            var model = WidgetExtractor.Build(
                compilation.GetTypeByMetadataName(type)!,
                compilation,
                CancellationToken.None,
                diagnostics
            );
            return (model, diagnostics.ToImmutable());
        }
    }
}
