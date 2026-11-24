// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Row pairing a label with a checkbox toggle.</summary>
    public partial class SettingsToggle : View
    {
        /// <summary>Initializes the row layout.</summary>
        public SettingsToggle()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            MinHeight = 36f.Point();
            MarginBottom = 4f.Point();
        }

        /// <summary>Gets or sets the toggle label.</summary>
        [Prop]
        public Behavior<string> Label { get; set; }

        /// <summary>Gets or sets the checked value.</summary>
        [Prop]
        public Behavior<bool> Value { get; set; }

        /// <summary>Gets or sets the toggle callback.</summary>
        [Prop]
        public Behavior<Action<bool>> OnToggle { get; set; }

        /// <summary>Builds the label and checkbox.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = Label,
                    FlexGrow = 1f,
                    Font = GameFont.Tiny,
                    TextAlign = TextAnchor.MiddleLeft,
                },
                new Checkbox { Checked = Value, OnValueChange = OnToggle },
            ];
    }
}
