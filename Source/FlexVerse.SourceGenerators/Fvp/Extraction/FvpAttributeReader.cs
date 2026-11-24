// parents: @.frostline/graph/m-flexverse-property.xml

using Microsoft.CodeAnalysis;

namespace FlexVerse.SourceGenerators.Fvp.Extraction
{
    /// <summary>Exact-name resolution of the [FVP] attribute — mirrors Widget's</summary>
    public static class FvpAttributeReader
    {
        /// <summary>Defines the Fvp Attribute Full Name constant.</summary>
        public const string FvpAttributeFullName = "FlexVerse.FVPAttribute";

        /// <summary>Performs the Has Fvp Attribute operation.</summary>
        public static bool HasFvpAttribute(IPropertySymbol propertySymbol)
        {
            for (
                var current = propertySymbol;
                current != null;
                current = current.OverriddenProperty
            )
            {
                if (RoslynUtils.GetAttribute(current, FvpAttributeFullName) != null)
                    return true;
            }
            return false;
        }
    }
}
