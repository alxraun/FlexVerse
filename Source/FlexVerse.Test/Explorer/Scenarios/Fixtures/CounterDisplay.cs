// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class CounterDisplay : Widget
    {
        [Prop]
        public Behavior<int> Count { get; set; }

        [Prop]
        public Behavior<Action> OnIncrement { get; set; }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new View
            {
                PaddingAll = 10f.Point(),
                BackgroundColor = new Color(0.2f, 0.2f, 0.2f),
                JustifyContent = Justify.Center,
                AlignItems = Align.Center,
                OnClick = OnIncrement,
                Children = new WidgetList
                {
                    new Label { Content = "CounterDisplay" },
                    new Label { Content = $"Count: {Count.Value}" },
                },
            };
        }
    }
}
