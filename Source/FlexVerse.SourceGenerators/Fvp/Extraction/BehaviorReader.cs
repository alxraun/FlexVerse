// parents: @.frostline/graph/m-flexverse-property.xml

using FlexVerse.SourceGenerators.Fvp.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp.Extraction
{
    /// <summary>Represents the BehaviorReader.</summary>
    public static class BehaviorReader
    {
        /// <summary>Performs the Read operation.</summary>
        public static TypeIdentity? Read(INamedTypeSymbol symbol)
        {
            for (var current = symbol; current != null; current = current.BaseType)
            {
                var name = current.OriginalDefinition.ToDisplayString();
                if (name == "FlexVerse.Behavior<T>")
                    return new TypeIdentity(
                        symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        current
                            .TypeArguments[0]
                            .ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        true
                    );
                if (name == "FlexVerse.Behavior")
                    return new TypeIdentity(
                        symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                        "",
                        false
                    );
            }
            return null;
        }
    }
}
