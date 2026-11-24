// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Preferences tab with filter and overlay settings.</summary>
    public partial class SettingsView : ScrollView
    {
        /// <summary>Initializes the view layout.</summary>
        public SettingsView()
        {
            FlexGrow = 1f;
            FlexShrink = 1f;
            MinHeight = 0f.Point();
            PaddingAll = 16f.Point();
        }

        /// <summary>Gets or sets the inspector settings.</summary>
        [Prop]
        public Behavior<InspectorSettings> Settings { get; set; }

        /// <summary>Gets or sets whether the box model overlay is shown.</summary>
        [Prop]
        public Behavior<bool> ShowBoxModel { get; set; }

        /// <summary>Gets or sets whether the update heatmap is shown.</summary>
        [Prop]
        public Behavior<bool> ShowHeatmap { get; set; }

        /// <summary>Gets or sets the heatmap decay duration in seconds.</summary>
        [Prop]
        public Behavior<float> HeatDecayDuration { get; set; }

        /// <summary>Gets or sets the active property category filters.</summary>
        [Prop]
        public Behavior<HashSet<string>> ActiveFilters { get; set; }

        /// <summary>Gets or sets the filter change callback.</summary>
        [Prop]
        public Behavior<Action<HashSet<string>>> OnFiltersChanged { get; set; }

        /// <summary>Builds the filter and overlay preference controls.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new Label
            {
                Content = "PROPERTY CATEGORIES",
                Font = GameFont.Tiny,
                TextAlign = TextAnchor.MiddleLeft,
                MarginBottom = 8f.Point(),
            };
            foreach (var kind in new[] { "Prop", "State", "FVP", "InheritedContext" })
                yield return new SettingsToggle
                {
                    Key = kind,
                    Label = $"Show [{kind}]",
                    Value = ActiveFilters.Select(filters => filters?.Contains(kind) ?? false),
                    OnToggle = (Action<bool>)(_ => ToggleFilter(kind)),
                };
            yield return new Label
            {
                Content = "DEBUG OVERLAYS",
                Font = GameFont.Tiny,
                TextAlign = TextAnchor.MiddleLeft,
                MarginTop = 20f.Point(),
                MarginBottom = 8f.Point(),
            };
            yield return new SettingsToggle
            {
                Label = "Box Model Overlay",
                Value = ShowBoxModel,
                OnToggle = (Action<bool>)(value => Settings.Value.ShowBoxModel = value),
            };
            yield return new SettingsToggle
            {
                Label = "Property Update Heatmap",
                Value = ShowHeatmap,
                OnToggle = (Action<bool>)(value => Settings.Value.ShowHeatmap = value),
            };
            yield return new Label
            {
                Content = HeatDecayDuration.Select(value => $"HEAT DECAY DURATION: {value:F2}s"),
                Font = GameFont.Tiny,
                TextAlign = TextAnchor.MiddleLeft,
                MarginTop = 12f.Point(),
            };
            yield return new HorizontalSlider
            {
                Value = HeatDecayDuration,
                MinValue = 0.1f,
                MaxValue = 2f,
                Disabled = ShowHeatmap.Select(show => !show),
                OnValueChange = (Action<float>)(value => Settings.Value.HeatDecayDuration = value),
            };
        }

        private void ToggleFilter(string kind)
        {
            var next = new HashSet<string>(ActiveFilters.Value);
            if (!next.Add(kind))
                next.Remove(kind);
            OnFiltersChanged.Value?.Invoke(next);
        }
    }
}
