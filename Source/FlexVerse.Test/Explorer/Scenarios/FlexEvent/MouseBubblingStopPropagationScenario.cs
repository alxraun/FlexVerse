// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseBubblingStopPropagationScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new MbspParent()];
        }

        public partial class MbspParent : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } = Color.red;

            public override void OnMouseDown(FlexEvent e)
            {
                Messages.Message("Parent Clicked", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(1f, 0f, 0f, 0.5f),
                    Color.red,
                    0.2f,
                    Easing.CubicEaseIn
                );
                e.StopPropagation();
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,

                    Children = new WidgetList { new MbspChild() },
                };
            }
        }

        public partial class MbspChild : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } = Color.green;

            public override void OnMouseDown(FlexEvent e)
            {
                Messages.Message("Child Clicked", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(0f, 1f, 0f, 0.5f),
                    Color.green,
                    0.2f,
                    Easing.CubicEaseIn
                );
                e.StopPropagation();
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new MbspGrandchild() },
                };
            }
        }

        public partial class MbspGrandchild : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } = Color.blue;

            public override void OnMouseDown(FlexEvent e)
            {
                Messages.Message("Grandchild Clicked", MessageTypeDefOf.NeutralEvent);
                CurrentColor = Animate.FromTo(
                    new Color(0f, 0f, 1f, 0.5f),
                    Color.blue,
                    0.2f,
                    Easing.CubicEaseIn
                );
                e.StopPropagation();
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new Label { Content = "Click Me" } },
                };
            }
        }
    }
}
