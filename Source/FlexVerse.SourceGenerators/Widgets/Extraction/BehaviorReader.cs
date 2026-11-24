// parents: @.frostline/graph/m-reactivity-analyzer.xml

using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the BehaviorReader.</summary>
    public static class BehaviorReader
    {
        /// <summary>Performs the Read operation.</summary>
        public static TypeIdentity? Read(INamedTypeSymbol typeSymbol)
        {
            bool isGeneric = false;
            string? typeArgDisplayName = null;
            string? typeArgFullName = null;
            bool foundBehavior = false;

            INamedTypeSymbol? current = typeSymbol;
            while (current != null)
            {
                var originalDef = current.OriginalDefinition;
                var originalDefName = originalDef.ToDisplayString();

                if (originalDefName == "FlexVerse.Behavior<T>")
                {
                    isGeneric = true;
                    if (current.TypeArguments.Length == 1)
                    {
                        var typeArg = current.TypeArguments[0];
                        typeArgDisplayName = typeArg.ToDisplayString(
                            SymbolDisplayFormat.MinimallyQualifiedFormat
                        );
                        typeArgFullName = typeArg
                            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
                            .Replace("global::", "");
                    }
                    foundBehavior = true;
                    break;
                }

                var currentName = current.ToDisplayString();
                if (currentName == "FlexVerse.Behavior")
                {
                    isGeneric = false;
                    foundBehavior = true;
                    break;
                }

                current = current.BaseType;
            }

            if (!foundBehavior)
            {
                return null;
            }

            var displayName = typeSymbol.ToDisplayString(
                SymbolDisplayFormat.MinimallyQualifiedFormat
            );

            return new TypeIdentity(
                Name: typeSymbol.Name,
                DisplayName: displayName,
                ArgName: typeArgDisplayName ?? string.Empty,
                ArgFullName: typeArgFullName ?? string.Empty,
                IsGeneric: isGeneric,
                IsAbstract: typeSymbol.IsAbstract
            );
        }
    }
}
