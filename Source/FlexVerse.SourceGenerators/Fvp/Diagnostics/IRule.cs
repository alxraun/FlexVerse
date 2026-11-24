// parents: @.frostline/graph/m-flexverse-property.xml

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp.Diagnostics
{
    /// <summary>Defines the IRule contract.</summary>
    public interface IRule<TSymbol>
        where TSymbol : ISymbol
    {
        ImmutableArray<Diagnostic> Check(TSymbol symbol);
    }
}
