// parents: @.frostline/graph/p-flextools.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.Widgets
{
    /// <summary>Settings page for the widget inspector tool.</summary>
    public partial class WidgetInspectorSettingsPage : ScrollView
    {
        /// <summary>Gets or sets the shared FlexTools settings.</summary>
        [Prop]
        public Behavior<FlexToolsSettings> Settings { get; set; }

        /// <summary>Gets or sets whether the tool is enabled.</summary>
        [Prop]
        public Behavior<bool> ToolEnabled { get; set; }

        /// <summary>Gets or sets whether the box model overlay is shown.</summary>
        [Prop]
        public Behavior<bool> ShowBoxModel { get; set; }

        /// <summary>Gets or sets whether the update heatmap is shown.</summary>
        [Prop]
        public Behavior<bool> ShowHeatmap { get; set; }

        /// <summary>Gets or sets the heatmap decay duration in seconds.</summary>
        [Prop]
        public Behavior<float> HeatDecayDuration { get; set; }

        /// <summary>Initializes the page layout.</summary>
        public WidgetInspectorSettingsPage()
        {
            FlexGrow = 1f;
            FlexShrink = 1f;
            MinWidth = 0f.Point();
            MinHeight = 0f.Point();
            PaddingAll = 20f.Point();
        }

        /// <summary>Builds the tool, overlay, and heatmap timing sections.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    TextAlign = TextAnchor.MiddleLeft,
                    Content = "Widget Inspector",
                    Font = GameFont.Medium,
                    Color = SettingsTheme.Text,
                    MarginBottom = 6f.Point(),
                },
                new Label
                {
                    TextAlign = TextAnchor.MiddleLeft,
                    Content = "Inspect live widget trees, reactive properties and layout updates.",
                    Color = SettingsTheme.Muted,
                    MarginBottom = 18f.Point(),
                },
                new ToolSettingsSection
                {
                    Title = "TOOL",
                    Content = new WidgetList
                    {
                        new SettingsToggleRow
                        {
                            Title = "Enable Widget Inspector",
                            Description = "Run the background service and debugging patches.",
                            Value = ToolEnabled,
                            OnValueChange = (Action<bool>)SetToolEnabled,
                        },
                        new Button
                        {
                            Label = "Show / hide inspector (F12)",
                            Height = 34f.Point(),
                            MarginTop = 14f.Point(),
                            Disabled = ToolEnabled.Select(enabled => !enabled),
                            Color = ToolEnabled.Select(enabled =>
                                enabled ? SettingsTheme.Text : SettingsTheme.Disabled
                            ),
                            OnClick = (Action)ToggleInspector,
                        },
                    },
                },
                new ToolSettingsSection
                {
                    Title = "OVERLAYS",
                    Content = new WidgetList
                    {
                        new SettingsToggleRow
                        {
                            Title = "Box model overlay",
                            Description = "Show margin, border, padding and content bounds.",
                            Value = ShowBoxModel,
                            Disabled = ToolEnabled.Select(enabled => !enabled),
                            OnValueChange = (Action<bool>)SetShowBoxModel,
                        },
                        new SettingsToggleRow
                        {
                            Title = "Update heatmap",
                            Description = "Highlight widgets when they update or change layout.",
                            Value = ShowHeatmap,
                            Disabled = ToolEnabled.Select(enabled => !enabled),
                            MarginTop = 16f.Point(),
                            OnValueChange = (Action<bool>)SetShowHeatmap,
                        },
                    },
                },
                new ToolSettingsSection
                {
                    Title = "HEATMAP TIMING",
                    Content = new WidgetList
                    {
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = HeatDecayDuration.Select(value =>
                                $"Heat decay duration   {value:F2} s"
                            ),
                            Color = Group.Combine(
                                ToolEnabled,
                                ShowHeatmap,
                                (enabled, heatmap) =>
                                    enabled && heatmap ? SettingsTheme.Text : SettingsTheme.Disabled
                            ),
                        },
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = "How long update highlights remain visible (0.10–2.00 s).",
                            Font = GameFont.Tiny,
                            Color = Group.Combine(
                                ToolEnabled,
                                ShowHeatmap,
                                (enabled, heatmap) =>
                                    enabled && heatmap
                                        ? SettingsTheme.Muted
                                        : SettingsTheme.Disabled
                            ),
                            MarginTop = 3f.Point(),
                            MarginBottom = 12f.Point(),
                        },
                        new HorizontalSlider
                        {
                            Value = HeatDecayDuration,
                            MinValue = 0.1f,
                            MaxValue = 2f,
                            Height = 20f.Point(),
                            Disabled = Group.Combine(
                                ToolEnabled,
                                ShowHeatmap,
                                (enabled, heatmap) => !enabled || !heatmap
                            ),
                            OnValueChange = (Action<float>)SetHeatDecayDuration,
                        },
                    },
                },
            ];

        private void SetToolEnabled(bool enabled)
        {
            Settings.Value.SetToolEnabled("WidgetInspector", enabled);
            ToolManager.RefreshTool("WidgetInspector", enabled);
        }

        private void ToggleInspector()
        {
            if (ToolEnabled.Value)
                ToolManager.GetTool<WidgetInspector.WidgetInspector>()?.ToggleWindow();
        }

        private void SetShowBoxModel(bool value)
        {
            if (ToolEnabled.Value)
                Settings.Value.GetToolSettings<WidgetInspector.InspectorSettings>().ShowBoxModel =
                    value;
        }

        private void SetShowHeatmap(bool value)
        {
            if (ToolEnabled.Value)
                Settings.Value.GetToolSettings<WidgetInspector.InspectorSettings>().ShowHeatmap =
                    value;
        }

        private void SetHeatDecayDuration(float value)
        {
            if (ToolEnabled.Value && ShowHeatmap.Value)
                Settings
                    .Value.GetToolSettings<WidgetInspector.InspectorSettings>()
                    .HeatDecayDuration = value;
        }
    }
}
