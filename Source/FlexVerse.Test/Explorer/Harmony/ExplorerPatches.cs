// parents: @.frostline/graph/m-explorer.xml

using HarmonyLib;
using LudeonTK;
using Verse;

namespace FlexVerse.Test.Explorer
{
    /// <summary>
    /// Interactive-run ergonomics patches, applied only when the game is NOT
    /// launched with -pickle-run (the same guard as the log opt-out,
    /// M-EXPLORER.REQ-005): automated runs are untouched — markers and stock
    /// game behavior stay the evidence contract.
    /// </summary>
    [HarmonyPatch]
    internal static class ExplorerPatches
    {
        private static bool Prepare()
        {
            return !ExplorerMod.IsAutomatedRun();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UIRoot), nameof(UIRoot.UIRootOnGUI))]
        internal static void UIRootOnGUI_Prefix()
        {
            Explorer.HandleFullscreenInput();
        }

        /// <summary>
        /// Auto-open the Explorer once a scene is reachable: the manual
        /// branch starts on the screen, not in the debug menu.
        /// </summary>
        [HarmonyPostfix]
        [HarmonyPatch(typeof(Messages), "Notify_LoadedLevelChanged")]
        internal static void Notify_LoadedLevelChanged_Postfix()
        {
            if (GenScene.InEntryScene || GenScene.InPlayScene)
            {
                Explorer.ToggleWindow();
            }
        }

        /// <summary>
        /// Keep the dev log window closed at startup: error-level noise that
        /// is expected outside Steam (SteamAPI init failure) and mod-metadata
        /// warnings must not steal focus from the Explorer. The messages
        /// still land in Player.log — only the auto-open is suppressed; the
        /// window stays reachable from the debug menu.
        /// </summary>
        [HarmonyPrefix]
        [HarmonyPatch(typeof(EditWindow_Log), "TryAutoOpen")]
        internal static bool TryAutoOpen_Prefix()
        {
            return false;
        }
    }
}
