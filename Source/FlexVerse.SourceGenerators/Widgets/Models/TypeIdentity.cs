// parents: @.frostline/graph/m-widget-models.xml

namespace FlexVerse.SourceGenerators.Widgets.Models
{
    /// <summary>Performs the Type Identity operation.</summary>
    public sealed record TypeIdentity(
        string Name,
        string DisplayName,
        string ArgName,
        string ArgFullName,
        bool IsGeneric,
        bool IsAbstract
    )
    {
        /// <summary>Gets the constructed name.</summary>
        public string ConstructedName => DisplayName;
    }
}
