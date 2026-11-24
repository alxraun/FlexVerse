// parents: @.frostline/graph/m-widget-models.xml

namespace FlexVerse.SourceGenerators.Widgets.Models
{
    /// <summary>Represents the Initializer struct.</summary>
    public readonly record struct Initializer(string Raw)
    {
        /// <summary>Gets a value indicating whether the initializer is the default.</summary>
        public bool IsDefault => Raw is "default" or "default()";

        /// <summary>Gets a value indicating whether the initializer is null.</summary>
        public bool IsNull => Raw is "null";

        /// <summary>Gets a value indicating whether the initializer is empty.</summary>
        public bool IsEmpty => string.IsNullOrWhiteSpace(Raw);

        /// <summary>Gets a value indicating whether the property is defined.</summary>
        public bool IsDefined => !IsEmpty && !IsDefault && !IsNull;

        /// <summary>Gets the code.</summary>
        public string Code => IsDefined ? Raw : string.Empty;

        /// <summary>Gets the assignment.</summary>
        public string Assignment => IsDefined ? $" = {Raw}" : string.Empty;
    }
}
