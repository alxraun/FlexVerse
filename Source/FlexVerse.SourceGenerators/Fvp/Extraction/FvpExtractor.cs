// parents: @.frostline/graph/m-flexverse-property.xml

using System.Threading;
using FlexVerse.SourceGenerators.Fvp.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp.Extraction
{
    /// <summary>Represents the FvpExtractor.</summary>
    public static class FvpExtractor
    {
        /// <summary>Performs the Build operation.</summary>
        public static FvpModel Build(INamedTypeSymbol symbol, CancellationToken ct)
        {
            var nameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            );
            return new FvpModel(
                ClassName: symbol.Name,
                DeclarationName: symbol.ToDisplayString(nameFormat),
                Namespace: symbol.ContainingNamespace.IsGlobalNamespace
                    ? string.Empty
                    : symbol.ContainingNamespace.ToDisplayString(),
                Usings: UsingExtractor.Extract(symbol, ct),
                Hierarchy: HierarchyExtractor.Extract(symbol),
                Properties: FvpPropertyExtractor.Extract(symbol, ct),
                IsSealed: symbol.IsSealed,
                HasFvpBase: symbol.BaseType is { } parent && HasFvpProperties(parent)
            );
        }

        /// <summary>Performs the Has Fvp Properties operation.</summary>
        public static bool HasFvpProperties(INamedTypeSymbol symbol)
        {
            for (var current = symbol; current != null; current = current.BaseType)
            {
                foreach (var member in current.GetMembers())
                {
                    if (
                        member
                            is IPropertySymbol
                            {
                                IsStatic: false,
                                Type: INamedTypeSymbol type
                            } property
                        && FvpAttributeReader.HasFvpAttribute(property)
                        && BehaviorReader.Read(type) != null
                    )
                        return true;
                }
            }
            return false;
        }
    }
}
