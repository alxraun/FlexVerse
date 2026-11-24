// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Combines the title bar and tab bar.</summary>
    public partial class InspectorHeader : View
    {
        /// <summary>Gets or sets the active tab index.</summary>
        [Prop]
        public Behavior<int> ActiveTab { get; set; }

        /// <summary>Gets or sets the tab change callback.</summary>
        [Prop]
        public Behavior<Action<int>> OnTabChanged { get; set; }

        /// <summary>Builds the title bar and tab bar.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new InspectorTitleBar(),
                new InspectorTabBar
                {
                    Height = 36f.Point(),
                    FlexShrink = 0,
                    ActiveTab = ActiveTab,
                    OnTabChanged = OnTabChanged,
                },
            ];
        }
    }
}
