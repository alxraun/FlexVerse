// parents: @.frostline/graph/m-flexverse-property.xml

using System.Collections.Immutable;
using System.Threading;
using FlexVerse.SourceGenerators.Fvp.Diagnostics;
using FlexVerse.SourceGenerators.Fvp.Extraction;
using FlexVerse.SourceGenerators.Fvp.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp
{
    /// <summary>Diagnostics-then-extraction pipeline — mirrors WidgetPipeline.</summary>
    public static class FvpPipeline
    {
        private static readonly DiagnosticEngine _diagnosticEngine = new();

        /// <summary>Initializes a new instance of the static class.</summary>
        public static (FvpModel? Model, ImmutableArray<Diagnostic> Diagnostics) Run(
            INamedTypeSymbol classSymbol,
            Compilation compilation,
            CancellationToken ct
        )
        {
            var diagnosticResult = _diagnosticEngine.Run(classSymbol);

            if (diagnosticResult.HasErrors)
            {
                return (null, diagnosticResult.Diagnostics);
            }

            var model = FvpExtractor.Build(classSymbol, ct);

            return (model, diagnosticResult.Diagnostics);
        }
    }
}
