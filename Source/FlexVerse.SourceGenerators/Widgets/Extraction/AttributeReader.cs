// parents: @.frostline/graph/m-reactivity-analyzer.xml

using FlexVerse.SourceGenerators.Widgets.Models;
using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Widgets.Extraction
{
    /// <summary>Represents the AttributeReader.</summary>
    public static class AttributeReader
    {
        /// <summary>Defines the Prop Attribute Full Name constant.</summary>
        public const string PropAttributeFullName = "FlexVerse.PropAttribute";

        /// <summary>Defines the State Attribute Full Name constant.</summary>
        public const string StateAttributeFullName = "FlexVerse.StateAttribute";

        /// <summary>Defines the Fvp Attribute Full Name constant.</summary>
        public const string FvpAttributeFullName = "FlexVerse.FVPAttribute";

        /// <summary>Defines the Inherited Context Attribute Full Name constant.</summary>
        public const string InheritedContextAttributeFullName =
            "FlexVerse.InheritedContextAttribute";

        /// <summary>Performs the Try Resolve Kind operation.</summary>
        public static bool TryResolveKind(
            IPropertySymbol propertySymbol,
            out PropertyKind? kind,
            out AttributeData? primaryAttribute
        )
        {
            kind = null;
            primaryAttribute = null;

            if (HasStateAttribute(propertySymbol))
            {
                kind = PropertyKind.State;
                primaryAttribute = RoslynUtils.GetAttribute(propertySymbol, StateAttributeFullName);
            }
            else if (HasInheritedContextAttribute(propertySymbol))
            {
                kind = PropertyKind.InheritedContext;
                primaryAttribute = RoslynUtils.GetAttribute(
                    propertySymbol,
                    InheritedContextAttributeFullName
                );
            }
            else if (HasPropAttribute(propertySymbol))
            {
                kind = PropertyKind.Prop;
                primaryAttribute = RoslynUtils.GetAttribute(propertySymbol, PropAttributeFullName);
            }
            else if (HasFvpAttribute(propertySymbol))
            {
                kind = PropertyKind.Fvp;
                primaryAttribute = RoslynUtils.GetAttribute(propertySymbol, FvpAttributeFullName);
            }

            if (!kind.HasValue && propertySymbol.OverriddenProperty is { } inherited)
                return TryResolveKind(inherited, out kind, out primaryAttribute);

            return kind.HasValue;
        }

        /// <summary>Performs the Get Nullable Bool Override operation.</summary>
        public static bool? GetNullableBoolOverride(
            AttributeData attributeData,
            string propertyName
        )
        {
            return RoslynUtils.GetArgument(attributeData, propertyName);
        }

        private static bool HasPropAttribute(ISymbol symbol) =>
            RoslynUtils.GetAttribute(symbol, PropAttributeFullName) != null;

        private static bool HasStateAttribute(ISymbol symbol) =>
            RoslynUtils.GetAttribute(symbol, StateAttributeFullName) != null;

        private static bool HasFvpAttribute(ISymbol symbol) =>
            RoslynUtils.GetAttribute(symbol, FvpAttributeFullName) != null;

        private static bool HasInheritedContextAttribute(ISymbol symbol) =>
            RoslynUtils.GetAttribute(symbol, InheritedContextAttributeFullName) != null;
    }
}
