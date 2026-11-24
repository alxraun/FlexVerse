// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexTools.WidgetInspector.Inspection;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Row representing one widget node in the hierarchy tree.</summary>
    public partial class HierarchyItem : Widget
    {
        /// <summary>Initializes the item layout.</summary>
        public HierarchyItem() => FlexShrink = 0;

        /// <summary>Gets or sets the widget snapshot node.</summary>
        [Prop]
        public Behavior<DebugNode> Node { get; set; }

        /// <summary>Gets or sets the nesting depth.</summary>
        [Prop]
        public Behavior<int> Depth { get; set; }

        /// <summary>Gets or sets whether the node is expanded.</summary>
        [Prop]
        public Behavior<bool> IsExpanded { get; set; }

        /// <summary>Gets or sets whether the node is selected.</summary>
        [Prop]
        public Behavior<bool> IsSelected { get; set; }

        /// <summary>Gets or sets the selection callback.</summary>
        [Prop]
        public Behavior<Action> OnSelect { get; set; }

        /// <summary>Gets or sets the expand toggle callback.</summary>
        [Prop]
        public Behavior<Action> OnToggleExpand { get; set; }

        /// <summary>Builds the expand button and node row.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    FlexGrow = 1f,
                    FlexDirection = FlexDirections.Row,
                    Height = 28f.Point(),
                    AlignItems = Align.Center,
                    PaddingLeft = Depth.Select(depth => (depth * 10f + 4f).Point()),
                    PaddingRight = 8f.Point(),
                    BackgroundColor = IsSelected.Select(selected =>
                        selected ? new Color(0.145f, 0.388f, 0.922f, 0.55f) : Color.clear
                    ),
                    Children = new WidgetList([
                        new Button
                        {
                            Width = 24f.Point(),
                            Height = 24f.Point(),
                            Label = Group.Combine(
                                Node,
                                IsExpanded,
                                (node, expanded) =>
                                    node?.Children.Count > 0 ? (expanded ? "▼" : "▶") : "·"
                            ),
                            Disabled = Node.Select(node =>
                                node == null || node.Children.Count == 0
                            ),
                            OnClick = OnToggleExpand,
                        },
                        new Button
                        {
                            FlexGrow = 1f,
                            Height = 28f.Point(),
                            DrawButtonBackground = false,
                            Label = Node.Select(node =>
                                node == null
                                    ? "<removed>"
                                    : $"#{node.InstanceID:X} {node.TypeName}  {node.KeyDisplay}"
                            ),
                            OnClick = OnSelect,
                            TextAlign = TextAnchor.MiddleLeft,
                            Font = GameFont.Tiny,
                            Color = IsSelected.Select(selected =>
                                selected ? Color.white : new Color(0.83f, 0.83f, 0.85f)
                            ),
                        },
                    ]),
                },
            ];
        }
    }
}
