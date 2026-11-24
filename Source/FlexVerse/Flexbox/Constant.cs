// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    class Constant
    {
        internal const int EdgeCount = 9;
        internal const int ExperimentalFeatureCount = 1;
        internal const int MeasureModeCount = 3;

        /// <summary>Keeps the cached-result cache small enough for even complex layouts.</summary>
        internal const int MaxCachedResultCount = 16;

        internal const int measureModeCount = 3;
        internal static readonly string[] measureModeNames = ["UNDEFINED", "EXACTLY", "AT_MOST"];
        internal static readonly string[] layoutModeNames =
        [
            "LAY_UNDEFINED",
            "LAY_EXACTLY",
            "LAY_AT_MOST",
        ];

        internal static readonly Node nodeDefaults = Flex.CreateDefaultNode();
        internal static readonly Config configDefaults = Flex.CreateDefaultConfig();

        internal const float defaultFlexGrow = 0;
        internal const float defaultFlexShrink = 0;
        internal const float webDefaultFlexShrink = 1;
    }
}
