// parents: @.frostline/graph/m-reactivity-analyzer.xml

using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets
{
    /// <summary>Represents the DiagnosticRules.</summary>
    public static class DiagnosticRules
    {
        private const string Category = "FlexVerse";

        /// <summary>Stores the type must be partial.</summary>
        public static readonly DiagnosticDescriptor TypeMustBePartial = new(
            id: "FV0001",
            title: "Widget must be declared partial",
            messageFormat: "Type '{0}' must be declared as partial to be a valid Widget",
            category: Category,
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true
        );
    }
}
