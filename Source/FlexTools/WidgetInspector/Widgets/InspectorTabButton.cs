// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Single selectable tab button in the inspector header.</summary>
    public partial class InspectorTabButton : Widget
    {
        /// <summary>Gets or sets the tab title.</summary>
        [Prop]
        public Behavior<string> Title { get; set; }

        /// <summary>Gets or sets the tab index.</summary>
        [Prop]
        public Behavior<int> Index { get; set; }

        /// <summary>Gets or sets the active tab index.</summary>
        [Prop]
        public Behavior<int> ActiveIndex { get; set; }

        /// <summary>Gets or sets the click callback receiving the tab index.</summary>
        [Prop]
        public Behavior<Action<int>> OnClick { get; set; }

        /// <summary>Builds the highlighted tab button.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    Key = "InspectorTabButton_Container",
                    FlexGrow = 1f,
                    AlignItems = Align.Center,
                    JustifyContent = Justify.Center,
                    BackgroundColor = Group.Combine(
                        ActiveIndex,
                        Index,
                        (active, index) =>
                            active == index ? new Color(0.2f, 0.2f, 0.2f) : Color.clear
                    ),
                    BorderBottom = Group.Combine(
                        ActiveIndex,
                        Index,
                        (active, index) => (active == index ? 2f : 0f).Point()
                    ),
                    BorderColor = Group.Combine(
                        ActiveIndex,
                        Index,
                        (active, index) =>
                            active == index ? new Color(0.145f, 0.388f, 0.922f) : Color.clear
                    ),
                    OnClick = (Action)(() => OnClick.Value?.Invoke(Index.Value)),
                    Children = new WidgetList([
                        new Label
                        {
                            Key = "InspectorTabButton_Label",
                            Content = Title,
                            Font = GameFont.Tiny,
                            Color = Group.Combine(
                                ActiveIndex,
                                Index,
                                (active, index) =>
                                    active == index
                                        ? new Color(0.145f, 0.388f, 0.922f)
                                        : new Color(0.5f, 0.5f, 0.5f)
                            ),
                        },
                    ]),
                },
            ];
        }
    }
}
