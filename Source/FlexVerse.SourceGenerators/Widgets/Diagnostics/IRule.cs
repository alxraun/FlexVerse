// parents: @.frostline/graph/m-reactivity-analyzer.xml

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Diagnostics
{
    /// <summary>Defines the IRule contract.</summary>
    public interface IRule<TSymbol>
        where TSymbol : ISymbol
    {
        ImmutableArray<Diagnostic> Check(TSymbol symbol);
    }
}
