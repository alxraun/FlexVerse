// parents: @.frostline/graph/m-flexverse-root.xml

using HarmonyLib;
using Verse;

namespace FlexVerse
{
    /// <summary>Provides the RimWorld mod entry point and settings bootstrap.</summary>
    public class FlexVerseMod : Mod
    {
        /// <summary>Gets the shared FlexVerse settings instance.</summary>
        public static FlexVerseSettings Settings { get; private set; }

        /// <summary>Loads settings and applies framework Harmony patches.</summary>
        public FlexVerseMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<FlexVerseSettings>();

            var harmony = new Harmony("FlexVerse");

            harmony.PatchCategory(typeof(FlexVerseMod).Assembly, "FlexVerse");

#if DEBUG
            harmony.PatchCategory(typeof(FlexVerseMod).Assembly, "Debug");
#endif
        }
    }
}
