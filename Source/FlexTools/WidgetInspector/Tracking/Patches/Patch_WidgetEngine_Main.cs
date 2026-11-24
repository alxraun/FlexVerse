// parents: @.frostline/graph/m-dbg-track.xml

using FlexVerse;
using HarmonyLib;

namespace FlexTools.WidgetInspector.Tracking.Patches
{
    /// <summary>Observes each engine after its UI pass completes.</summary>
    [HarmonyPatch(typeof(WidgetEngine), nameof(WidgetEngine.ProcessUI))]
    [HarmonyPatchCategory(WidgetInspector.HarmonyCategory)]
    internal static class Patch_WidgetEngine_Main
    {
        /// <summary>Runs tool observation on the finished engine.</summary>
        [HarmonyPostfix]
        public static void Postfix(WidgetEngine __instance) =>
            ToolManager.GetTool<WidgetInspector>()?.Observe(__instance);
    }
}
