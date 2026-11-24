// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseDragAndMoveScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new MdamBox()];
        }

        public partial class MdamBox : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } =
                Animate.State(Color.red, 0.2f, Easing.CubicEaseOut);

            public override void OnMouseDrag(FlexEvent e)
            {
                Messages.Message("Box Mouse Drag", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(1f, 0f, 0f, 0.5f),
                    Color.red,
                    0.2f,
                    Easing.CubicEaseIn
                );
            }

            public override void OnMouseMove(FlexEvent e)
            {
                Messages.Message("Box Mouse Move", MessageTypeDefOf.NeutralEvent);
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
                    Children = new WidgetList { new Label { Content = "Drag Me" } },
                };
            }
        }
    }
}
