// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using FlexVerse.SourceGenerators.Widgets.Diagnostics.Rules;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Diagnostics
{
    /// <summary>Represents the DiagnosticEngine.</summary>
    public sealed class DiagnosticEngine
    {
        private readonly ImmutableArray<IRule<INamedTypeSymbol>> _rules;

        /// <summary>Initializes a new instance of the DiagnosticEngine class.</summary>
        public DiagnosticEngine()
        {
            _rules = ImmutableArray.Create<IRule<INamedTypeSymbol>>(new TypeMustBePartialRule());
        }

        /// <summary>Performs the Run operation.</summary>
        public DiagnosticResult Run(INamedTypeSymbol typeSymbol)
        {
            var diagnostics = ImmutableArray.CreateBuilder<Diagnostic>();

            foreach (var rule in _rules)
            {
                var results = rule.Check(typeSymbol);
                diagnostics.AddRange(results);
            }

            return CreateResult(diagnostics.ToImmutable());
        }

        private static DiagnosticResult CreateResult(ImmutableArray<Diagnostic> diagnostics)
        {
            var hasErrors = false;
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    hasErrors = true;
                    break;
                }
            }
            return new DiagnosticResult(diagnostics, hasErrors);
        }
    }

    /// <summary>Represents the DiagnosticResult struct.</summary>
    public readonly struct DiagnosticResult
    {
        /// <summary>Gets the diagnostics.</summary>
        public ImmutableArray<Diagnostic> Diagnostics { get; }

        /// <summary>Gets a value indicating whether there are errors.</summary>
        public bool HasErrors { get; }

        /// <summary>Initializes a new instance of the DiagnosticResult class.</summary>
        public DiagnosticResult(ImmutableArray<Diagnostic> diagnostics, bool hasErrors)
        {
            Diagnostics = diagnostics;
            HasErrors = hasErrors;
        }
    }
}
