// parents: @.frostline/graph/m-flexverse-property.xml

using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp.Extraction
{
    /// <summary>Represents the HeaderExtractor.</summary>
    public static class HeaderExtractor
    {
        /// <summary>Performs the Extract operation.</summary>
        public static string Extract(INamedTypeSymbol symbol)
        {
            var parts = new System.Collections.Generic.List<string>();

            parts.Add(GetAccessibilityString(symbol.DeclaredAccessibility));

            if (symbol.IsStatic)
                parts.Add("static");
            else
            {
                if (symbol.IsAbstract && symbol.TypeKind != TypeKind.Interface)
                    parts.Add("abstract");
                if (symbol.IsSealed && symbol.TypeKind == TypeKind.Class)
                    parts.Add("sealed");
            }

            parts.Add("partial");

            parts.Add(GetTypeKeyword(symbol));

            var nameFormat = new SymbolDisplayFormat(
                typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
            );

            parts.Add(symbol.ToDisplayString(nameFormat));

            if (!symbol.TypeParameters.IsEmpty)
            {
                var fullDeclFormat = new SymbolDisplayFormat(
                    typeQualificationStyle: SymbolDisplayTypeQualificationStyle.NameOnly,
                    genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters
                        | SymbolDisplayGenericsOptions.IncludeTypeConstraints,
                    miscellaneousOptions: SymbolDisplayMiscellaneousOptions.UseSpecialTypes
                );
                var fullDecl = symbol.ToDisplayString(fullDeclFormat);
                var nameDecl = symbol.ToDisplayString(nameFormat);

                if (fullDecl.StartsWith(nameDecl) && fullDecl.Length > nameDecl.Length)
                {
                    var constr = fullDecl.Substring(nameDecl.Length).Trim();
                    if (!string.IsNullOrEmpty(constr))
                    {
                        parts.Add(constr);
                    }
                }
            }

            return string.Join(" ", parts);
        }

        private static string GetAccessibilityString(Accessibility accessibility)
        {
            return accessibility switch
            {
                Accessibility.Public => "public",
                Accessibility.Internal => "internal",
                Accessibility.Protected => "protected",
                Accessibility.Private => "private",
                Accessibility.ProtectedAndInternal => "private protected",
                Accessibility.ProtectedOrInternal => "protected internal",
                _ => "internal",
            };
        }

        private static string GetTypeKeyword(INamedTypeSymbol symbol)
        {
            if (symbol.IsRecord)
                return symbol.TypeKind == TypeKind.Struct ? "record struct" : "record";
            return symbol.TypeKind switch
            {
                TypeKind.Class => "class",
                TypeKind.Struct => "struct",
                TypeKind.Interface => "interface",
                _ => "class",
            };
        }
    }
}
