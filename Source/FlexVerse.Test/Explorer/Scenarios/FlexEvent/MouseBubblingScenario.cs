// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseBubblingScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new MbParent()];
        }

        public partial class MbParent : Widget
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
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,

                    Children = new WidgetList { new MbChild() },
                };
            }
        }

        public partial class MbChild : Widget
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
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new MbGrandchild() },
                };
            }
        }

        public partial class MbGrandchild : Widget
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
