// parents: @.frostline/graph/p-flextools.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.Widgets
{
    /// <summary>Sidebar item that selects a tool settings page.</summary>
    public partial class ToolNavigationItem : Widget
    {
        /// <summary>Gets or sets the item title.</summary>
        [Prop]
        public Behavior<string> Title { get; set; }

        /// <summary>Gets or sets whether the item is selected.</summary>
        [Prop]
        public Behavior<bool> Selected { get; set; }

        /// <summary>Gets or sets whether the tool is enabled.</summary>
        [Prop]
        public Behavior<bool> ToolEnabled { get; set; }

        /// <summary>Gets or sets the callback invoked on selection.</summary>
        [Prop]
        public Behavior<Action> OnSelect { get; set; }

        /// <summary>Initializes the item layout.</summary>
        public ToolNavigationItem()
        {
            FlexDirection = FlexDirections.Column;
            FlexShrink = 0f;
        }

        /// <summary>Builds the selectable navigation card.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    BackgroundColor = Selected.Value ? SettingsTheme.Selected : Color.clear,
                    BorderRadius = 6f,
                    BorderLeft = 3f.Point(),
                    BorderColor = Selected.Value ? SettingsTheme.Accent : Color.clear,
                    PaddingAll = 12f.Point(),
                    OnClick = OnSelect,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = Title,
                            Color = Selected.Value ? SettingsTheme.Text : SettingsTheme.Muted,
                            MarginBottom = 4f.Point(),
                        },
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = ToolEnabled.Select(enabled =>
                                enabled ? "Enabled" : "Disabled"
                            ),
                            Font = GameFont.Tiny,
                            Color = ToolEnabled.Select(enabled =>
                                enabled ? SettingsTheme.Accent : SettingsTheme.Muted
                            ),
                        },
                    },
                },
            ];
    }
}
