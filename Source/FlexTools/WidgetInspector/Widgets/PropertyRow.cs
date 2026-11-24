// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexTools.WidgetInspector.Models;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Expandable row showing one property and its behavior.</summary>
    public partial class PropertyRow : View
    {
        /// <summary>Gets or sets the property snapshot to display.</summary>
        [Prop]
        public Behavior<DebugPropertyNode> Property { get; set; }

        /// <summary>Gets or sets whether the behavior detail is open.</summary>
        [State]
        public Behavior<bool> IsOpen { get; set; } = false;

        /// <summary>Builds the header and optional behavior detail.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            var property = Property.Value;
            if (property == null)
                return [];
            return
            [
                new PropertyRowHeader
                {
                    Property = Property,
                    OnClick = (Action)(() => IsOpen = !IsOpen.Value),
                },
                IsOpen.Value && property.BehaviorNode != null
                    ? new BehaviorItem { Node = property.BehaviorNode, PaddingAll = 8f.Point() }
                    : null,
            ];
        }
    }
}
