// parents: @.frostline/graph/p-flextools.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.Widgets
{
    /// <summary>Root view of the FlexTools settings window.</summary>
    public partial class SettingsApp : View
    {
        /// <summary>Gets or sets the shared FlexTools settings.</summary>
        [Prop]
        public Behavior<FlexToolsSettings> Settings { get; set; }

        /// <summary>Gets or sets whether the widget inspector tool is enabled.</summary>
        [Prop]
        public Behavior<bool> DebuggerEnabled { get; set; }

        /// <summary>Gets or sets whether the box model overlay is shown.</summary>
        [Prop]
        public Behavior<bool> ShowBoxModel { get; set; }

        /// <summary>Gets or sets whether the update heatmap is shown.</summary>
        [Prop]
        public Behavior<bool> ShowHeatmap { get; set; }

        /// <summary>Gets or sets the heatmap decay duration in seconds.</summary>
        [Prop]
        public Behavior<float> HeatDecayDuration { get; set; }

        /// <summary>Gets or sets the identifier of the selected tool page.</summary>
        [State]
        public Behavior<string> SelectedTool { get; set; } = "WidgetInspector";

        /// <summary>Initializes the settings window layout.</summary>
        public SettingsApp()
        {
            Width = 100f.Percent();
            Height = 100f.Percent();
            FlexDirection = FlexDirections.Row;
            BackgroundColor = SettingsTheme.Background;
            BorderAll = 1f.Point();
            BorderColor = SettingsTheme.Border;
            BorderRadius = 8f;
        }

        /// <summary>Builds the sidebar navigation and selected tool page.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Key = "Sidebar",
                    Width = 210f.Point(),
                    PaddingAll = 14f.Point(),
                    BackgroundColor = SettingsTheme.Sidebar,
                    BorderRight = 1f.Point(),
                    BorderColor = SettingsTheme.Border,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = "TOOLS",
                            Font = GameFont.Tiny,
                            Color = SettingsTheme.Muted,
                            MarginBottom = 16f.Point(),
                            MarginLeft = 8f.Point(),
                        },
                        new ToolNavigationItem
                        {
                            Key = "WidgetInspector",
                            Title = "Widget Inspector",
                            ToolEnabled = DebuggerEnabled,
                            Selected = SelectedTool.Select(id => id == "WidgetInspector"),
                            OnSelect = (Action)(() => SelectedTool = "WidgetInspector"),
                        },
                    },
                },
                SelectedTool.Value == "WidgetInspector"
                    ? new WidgetInspectorSettingsPage
                    {
                        Key = "WidgetInspectorSettings",
                        Settings = Settings,
                        ToolEnabled = DebuggerEnabled,
                        ShowBoxModel = ShowBoxModel,
                        ShowHeatmap = ShowHeatmap,
                        HeatDecayDuration = HeatDecayDuration,
                    }
                    : null,
            ];
    }
}
