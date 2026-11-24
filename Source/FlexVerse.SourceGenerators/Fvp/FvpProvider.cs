// parents: @.frostline/graph/m-flexverse-property.xml

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Fvp.Extraction;
using FlexVerse.SourceGenerators.Fvp.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Fvp
{
    /// <summary>Registers the incremental [FVP] pipeline, mirroring WidgetProvider.</summary>
    internal static class FvpProvider
    {
        /// <summary>Creates the incremental pipeline for [FVP] members.</summary>
        public static IncrementalValuesProvider<(
            FvpModel? Model,
            ImmutableArray<Diagnostic> Diagnostics
        )> CreatePipeline(IncrementalGeneratorInitializationContext context)
        {
            var candidateClasses = context
                .SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => GetSemanticTarget(ctx, ct)
                )
                .Where(static cds => cds is not null)
                .Select(static (cds, _) => cds!);

            return candidateClasses
                .Combine(context.CompilationProvider)
                .Select(
                    static (
                        (ClassDeclarationSyntax cds, Compilation compilation) tuple,
                        CancellationToken ct
                    ) =>
                    {
                        var symbol = GetClassSymbol(tuple.cds, tuple.compilation, ct);
                        return FvpPipeline.Run(symbol, tuple.compilation, ct);
                    }
                );
        }

        private static ClassDeclarationSyntax? GetSemanticTarget(
            GeneratorSyntaxContext context,
            CancellationToken cancellationToken
        )
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            var classSymbol = context.SemanticModel.GetDeclaredSymbol(
                classDeclarationSyntax,
                cancellationToken
            );

            if (classSymbol is null)
            {
                return null;
            }

            if (IsWidget(classSymbol))
            {
                return null;
            }

            return FvpExtractor.HasFvpProperties(classSymbol) ? classDeclarationSyntax : null;
        }

        private static bool IsWidget(INamedTypeSymbol symbol)
        {
            for (var current = symbol; current != null; current = current.BaseType)
            {
                if (current.ToDisplayString() == "FlexVerse.Widget")
                    return true;
            }

            return false;
        }

        private static INamedTypeSymbol GetClassSymbol(
            ClassDeclarationSyntax cds,
            Compilation compilation,
            CancellationToken ct
        )
        {
            var semanticModel = compilation.GetSemanticModel(cds.SyntaxTree);
            return semanticModel.GetDeclaredSymbol(cds, ct)
                ?? throw new System.InvalidOperationException(
                    $"Could not get symbol for class {cds.Identifier.Text}"
                );
        }
    }
}
