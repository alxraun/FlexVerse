// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the HierarchyExtractor.</summary>
    public static class HierarchyExtractor
    {
        /// <summary>Performs the Extract operation.</summary>
        public static ImmutableArray<StructureModel> Extract(INamedTypeSymbol symbol)
        {
            var result = ImmutableArray.CreateBuilder<StructureModel>();
            var current = symbol;

            while (current != null)
            {
                result.Add(
                    new StructureModel(
                        Header: HeaderExtractor.Extract(current),
                        Name: current.Name,
                        IsGeneric: current.IsGenericType
                    )
                );
                current = current.ContainingType;
            }

            result.Reverse();

            return result.ToImmutable();
        }
    }
}
