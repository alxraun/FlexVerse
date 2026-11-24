// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseClickEventsScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new MceBox()];
        }

        public partial class MceBox : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } =
                Animate.State(Color.red, 0.2f, Easing.CubicEaseOut);

            public override void OnMouseDown(FlexEvent e)
            {
                Messages.Message("Box Mouse Down", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(1f, 0f, 0f, 0.5f),
                    Color.red,
                    0.2f,
                    Easing.CubicEaseIn
                );
            }

            public override void OnMouseUp(FlexEvent e)
            {
                Messages.Message("Box Mouse Up", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(1f, 0f, 0f, 0.5f),
                    Color.red,
                    0.2f,
                    Easing.CubicEaseIn
                );
            }

            public override void OnScrollWheel(FlexEvent e)
            {
                Messages.Message("Box Scroll Wheel", MessageTypeDefOf.NeutralEvent);
                Messages.Message("Delta: " + e.ScrollDelta, MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(1f, 0f, 0f, 0.5f),
                    Color.red,
                    0.2f,
                    Easing.CubicEaseIn
                );
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 60f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new Label { Content = "Click Me" } },
                };
            }
        }
    }
}
