// parents: @.frostline/graph/p-flextools.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.Widgets
{
    /// <summary>Row that pairs a label and description with a checkbox.</summary>
    public partial class SettingsToggleRow : Widget
    {
        /// <summary>Gets or sets the row title.</summary>
        [Prop]
        public Behavior<string> Title { get; set; }

        /// <summary>Gets or sets the row description text.</summary>
        [Prop]
        public Behavior<string> Description { get; set; }

        /// <summary>Gets or sets the checkbox value.</summary>
        [Prop]
        public Behavior<bool> Value { get; set; }

        /// <summary>Gets or sets the callback invoked when the value changes.</summary>
        [Prop]
        public Behavior<Action<bool>> OnValueChange { get; set; }

        /// <summary>Gets or sets a value indicating whether the row is disabled.</summary>
        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        /// <summary>Initializes the row layout.</summary>
        public SettingsToggleRow()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            MinHeight = 48f.Point();
        }

        /// <summary>Builds the label column and checkbox.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    FlexGrow = 1f,
                    FlexShrink = 1f,
                    MinWidth = 0f.Point(),
                    MarginRight = 20f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = Title,
                            Color = Disabled.Select(disabled =>
                                disabled ? SettingsTheme.Disabled : SettingsTheme.Text
                            ),
                            MarginBottom = 3f.Point(),
                        },
                        new Label
                        {
                            TextAlign = TextAnchor.MiddleLeft,
                            Content = Description,
                            Font = GameFont.Tiny,
                            Color = Disabled.Select(disabled =>
                                disabled ? SettingsTheme.Disabled : SettingsTheme.Muted
                            ),
                        },
                    },
                },
                new Checkbox
                {
                    Checked = Value,
                    Disabled = Disabled,
                    OnValueChange = OnValueChange,
                    Tooltip = Description,
                },
            ];
    }
}
