// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ColorDisplay : Widget
    {
        private static readonly Color DefaultColor = Color.grey;

        [Prop]
        public Behavior<string> Content { get; set; } = "ColorDisplay";

        [Prop]
        public Behavior<Color> DisplayColor { get; set; } = DefaultColor;

        [State]
        private Behavior<Color> CurrentColor { get; set; } =
            Animate.State(value: DefaultColor, duration: 0.2f, Easing.CubicEaseOut);

        private void OnChangeColor()
        {
            CurrentColor.Target =
                (CurrentColor.Target == DefaultColor)
                    ? (DisplayColor.Target != default)
                        ? DisplayColor.Target
                        : DefaultColor
                    : DefaultColor;
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new View
            {
                PaddingAll = 10f.Point(),
                MarginAll = 5f.Point(),
                JustifyContent = Justify.Center,
                AlignItems = Align.Center,
                BackgroundColor = CurrentColor,
                OnClick = new Action(OnChangeColor),
                Children = new WidgetList { new Label { Content = Content } },
            };
        }
    }
}
