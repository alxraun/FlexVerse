// parents: @.frostline/graph/m-dbg-track.xml

using HarmonyLib;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Tracking.Patches
{
    /// <summary>Toggles the inspector window when F12 is pressed.</summary>
    [HarmonyPatch(typeof(Root), nameof(Root.OnGUI))]
    [HarmonyPatchCategory(WidgetInspector.HarmonyCategory)]
    internal static class Patch_Root_OnGUI
    {
        /// <summary>Consumes F12 and toggles the inspector window.</summary>
        [HarmonyPrefix]
        public static void Prefix()
        {
            if (Event.current?.type != EventType.KeyDown || Event.current.keyCode != KeyCode.F12)
                return;
            var tool = ToolManager.GetTool<WidgetInspector>();
            if (tool == null)
                return;
            tool.ToggleWindow();
            Event.current.Use();
        }
    }
}
