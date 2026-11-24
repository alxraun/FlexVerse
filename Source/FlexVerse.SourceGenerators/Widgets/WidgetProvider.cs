// parents: @.frostline/graph/m-widget-emitter.xml

using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Widgets
{
    internal static class WidgetProvider
    {
        /// <summary>Creates the incremental pipeline for widget classes.</summary>
        public static IncrementalValuesProvider<(
            WidgetModel? Widget,
            ImmutableArray<Diagnostic> Diagnostics
        )> CreatePipeline(IncrementalGeneratorInitializationContext context)
        {
            var widgetClasses = context
                .SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (node, _) => node is ClassDeclarationSyntax,
                    transform: static (ctx, ct) => GetSemanticTargetForWidget(ctx, ct)
                )
                .Where(static cds => cds is not null)
                .Select(static (cds, _) => cds!);

            return widgetClasses
                .Combine(context.CompilationProvider)
                .Select(
                    static (
                        (ClassDeclarationSyntax cds, Compilation compilation) tuple,
                        CancellationToken ct
                    ) =>
                    {
                        var symbol = GetClassSymbol(tuple.cds, tuple.compilation, ct);
                        return WidgetPipeline.Run(symbol, tuple.compilation, ct);
                    }
                );
        }

        private static ClassDeclarationSyntax? GetSemanticTargetForWidget(
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

            return IsWidget(classSymbol) ? classDeclarationSyntax : null;
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
