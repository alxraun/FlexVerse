// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Widgets.Diagnostics.Rules
{
    /// <summary>Represents the TypeMustBePartialRule.</summary>
    public sealed class TypeMustBePartialRule : IRule<INamedTypeSymbol>
    {
        /// <summary>Performs the Check operation.</summary>
        public ImmutableArray<Diagnostic> Check(INamedTypeSymbol symbol)
        {
            if (!IsPartial(symbol))
            {
                return ImmutableArray.Create(
                    Diagnostic.Create(
                        DiagnosticRules.TypeMustBePartial,
                        symbol.Locations.FirstOrDefault(),
                        symbol.Name
                    )
                );
            }

            return ImmutableArray<Diagnostic>.Empty;
        }

        private static bool IsPartial(INamedTypeSymbol typeSymbol)
        {
            foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
            {
                if (
                    syntaxRef.GetSyntax() is TypeDeclarationSyntax typeDecl
                    && typeDecl.Modifiers.Any(SyntaxKind.PartialKeyword)
                )
                {
                    return true;
                }
            }
            return false;
        }
    }
}
