// parents: @.frostline/graph/m-widget-emitter.xml

using System.Collections.Immutable;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Diagnostics;
using FlexVerse.SourceGenerators.Widgets.Extraction;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets
{
    /// <summary>Represents the WidgetPipeline.</summary>
    public static class WidgetPipeline
    {
        private static readonly DiagnosticEngine _diagnosticEngine = new();

        /// <summary>Initializes a new instance of the static class.</summary>
        public static (WidgetModel? Widget, ImmutableArray<Diagnostic> Diagnostics) Run(
            INamedTypeSymbol widgetSymbol,
            Compilation compilation,
            CancellationToken ct
        )
        {
            var diagnosticResult = _diagnosticEngine.Run(widgetSymbol);

            if (diagnosticResult.HasErrors)
            {
                return (null, diagnosticResult.Diagnostics);
            }

            var diagnosticsBuilder = ImmutableArray.CreateBuilder<Diagnostic>();
            diagnosticsBuilder.AddRange(diagnosticResult.Diagnostics);

            var model = WidgetExtractor.Build(widgetSymbol, compilation, ct, diagnosticsBuilder);

            return (model, diagnosticsBuilder.ToImmutable());
        }
    }
}
