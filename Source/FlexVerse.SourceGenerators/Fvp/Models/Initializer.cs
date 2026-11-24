// parents: @.frostline/graph/m-flexverse-property.xml

namespace FlexVerse.SourceGenerators.Fvp.Models
{
    /// <summary>Represents the Initializer struct.</summary>
    public readonly record struct Initializer(string Raw)
    {
        /// <summary>Gets a value indicating whether the property is defined.</summary>
        public bool IsDefined =>
            !string.IsNullOrWhiteSpace(Raw) && Raw is not ("default" or "default()" or "null");

        /// <summary>Gets the assignment.</summary>
        public string Assignment => IsDefined ? $" = {Raw}" : "";
    }
}
