// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Row of tab buttons for the inspector views.</summary>
    public partial class InspectorTabBar : View
    {
        /// <summary>Gets or sets the active tab index.</summary>
        [Prop]
        public Behavior<int> ActiveTab { get; set; }

        /// <summary>Gets or sets the tab change callback.</summary>
        [Prop]
        public Behavior<Action<int>> OnTabChanged { get; set; }

        /// <summary>Builds the tab buttons.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new InspectorTabButton
                {
                    Title = "STRUCTURE",
                    Index = 0,
                    ActiveIndex = ActiveTab,
                    OnClick = OnTabChanged,
                    FlexGrow = 1f,
                },
                new InspectorTabButton
                {
                    Title = "PREFERENCES",
                    Index = 1,
                    ActiveIndex = ActiveTab,
                    OnClick = OnTabChanged,
                    FlexGrow = 1f,
                },
            ];
        }

        /// <summary>Initializes the row layout.</summary>
        public InspectorTabBar() => FlexDirection = FlexDirections.Row;
    }
}
