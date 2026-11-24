// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the UsingExtractor.</summary>
    public static class UsingExtractor
    {
        /// <summary>Performs the Extract operation.</summary>
        public static ImmutableArray<string> Extract(INamedTypeSymbol symbol, CancellationToken ct)
        {
            var allUsingDirectives = new System.Collections.Generic.HashSet<string>();
            INamedTypeSymbol? currentSymbol = symbol;
            while (currentSymbol != null && currentSymbol.SpecialType != SpecialType.System_Object)
            {
                if (!currentSymbol.ContainingNamespace.IsGlobalNamespace)
                {
                    allUsingDirectives.Add(
                        $"using {currentSymbol.ContainingNamespace.ToDisplayString()};"
                    );
                }

                foreach (var syntaxRef in currentSymbol.DeclaringSyntaxReferences)
                {
                    var root = syntaxRef.GetSyntax(ct).SyntaxTree.GetRoot(ct);
                    if (root is CompilationUnitSyntax cu)
                    {
                        foreach (var usingDirective in cu.Usings)
                        {
                            allUsingDirectives.Add(usingDirective.WithoutTrivia().ToFullString());
                        }
                    }
                }
                currentSymbol = currentSymbol.BaseType;
            }
            return allUsingDirectives.ToImmutableArray();
        }
    }
}
