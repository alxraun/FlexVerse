// parents: @.frostline/graph/m-flexverse-property.xml

using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp
{
    /// <summary>Represents the DiagnosticRules.</summary>
    public static class DiagnosticRules
    {
        private const string Category = "FlexVerse";

        /// <summary>Stores the type must be partial.</summary>
        public static readonly DiagnosticDescriptor TypeMustBePartial = new(
            id: "FV0002",
            title: "FVP type must be declared partial",
            messageFormat: "Type '{0}' must be declared as partial to use [FVP] properties",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
