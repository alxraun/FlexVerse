// parents: @.frostline/graph/p-flextools.xml

using FlexTools.Widgets;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools
{
    /// <summary>Mod entry point that hosts FlexTools settings and tools.</summary>
    public class FlexToolsMod : Mod
    {
        private const string SettingsUiKey = "FlexTools.Settings";

        /// <summary>Gets the shared FlexTools settings instance.</summary>
        public static FlexToolsSettings Settings { get; private set; }

        /// <summary>Initializes the mod and activates its enabled tools.</summary>
        public FlexToolsMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<FlexToolsSettings>();

            ToolManager.Initialize();
        }

        /// <summary>Gets the settings category name.</summary>
        public override string SettingsCategory() => "FlexVerse Tools";

        /// <summary>Draws the FlexTools settings window contents.</summary>
        public override void DoSettingsWindowContents(Rect inRect)
        {
            var debuggerSettings = Settings.GetToolSettings<WidgetInspector.InspectorSettings>();
            FVUI.Render(
                SettingsUiKey,
                inRect,
                new SettingsApp
                {
                    Settings = Settings,
                    DebuggerEnabled = Settings.IsToolEnabled("WidgetInspector"),
                    ShowBoxModel = debuggerSettings.ShowBoxModel,
                    ShowHeatmap = debuggerSettings.ShowHeatmap,
                    HeatDecayDuration = debuggerSettings.HeatDecayDuration,
                }
            );
        }

        /// <summary>Persists settings and releases the settings UI resources.</summary>
        public override void WriteSettings()
        {
            base.WriteSettings();
            FVUI.Release(SettingsUiKey);
        }
    }
}
