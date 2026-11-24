// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using System.Threading;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the WidgetExtractor.</summary>
    public static class WidgetExtractor
    {
        /// <summary>Performs the Build operation.</summary>
        public static WidgetModel Build(
            INamedTypeSymbol symbol,
            Compilation compilation,
            CancellationToken ct,
            ImmutableArray<Diagnostic>.Builder? diagnostics = null
        )
        {
            var usings = UsingExtractor.Extract(symbol, ct);

            var hierarchy = HierarchyExtractor.Extract(symbol);

            var properties = PropertyExtractor.Extract(symbol, compilation, ct, diagnostics);

            bool isBase = symbol.ToDisplayString() == "FlexVerse.Widget";

            var genericNameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            );
            var genericClassName = symbol.ToDisplayString(genericNameFormat);

            return new WidgetModel(
                ClassName: symbol.Name,
                DeclarationName: genericClassName,
                Namespace: symbol.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : symbol.ContainingNamespace.ToDisplayString(),
                IsBase: isBase,
                IsAbstract: symbol.IsAbstract,
                Usings: usings,
                Hierarchy: hierarchy,
                Properties: properties
            );
        }
    }
}
