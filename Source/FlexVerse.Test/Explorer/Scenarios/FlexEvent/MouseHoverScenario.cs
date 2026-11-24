// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseHoverScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new MhParent()];
        }

        public partial class MhParent : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } =
                Animate.State(Color.red, 0.2f, Easing.CubicEaseOut);

            public override void OnMouseEnter(FlexEvent e)
            {
                Messages.Message("Parent Mouse Enter", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = new Color(1f, 0f, 0f, 0.5f);
            }

            public override void OnMouseLeave(FlexEvent e)
            {
                Messages.Message("Parent Mouse Leave", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = Color.red;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,

                    Children = new WidgetList { new MhChild() },
                };
            }
        }

        public partial class MhChild : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } =
                Animate.State(Color.green, 0.2f, Easing.CubicEaseOut);

            public override void OnMouseEnter(FlexEvent e)
            {
                Messages.Message("Child Mouse Enter", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = new Color(0f, 1f, 0f, 0.5f);
            }

            public override void OnMouseLeave(FlexEvent e)
            {
                Messages.Message("Child Mouse Leave", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = Color.green;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new MhGrandchild() },
                };
            }
        }

        public partial class MhGrandchild : Widget
        {
            [State]
            private Behavior<Color> CurrentColor { get; set; } =
                Animate.State(Color.blue, 0.2f, Easing.CubicEaseOut);

            public override void OnMouseEnter(FlexEvent e)
            {
                Messages.Message("Grandchild Mouse Enter", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = new Color(0f, 0f, 1f, 0.5f);
            }

            public override void OnMouseLeave(FlexEvent e)
            {
                Messages.Message("Grandchild Mouse Leave", MessageTypeDefOf.NeutralEvent);
                CurrentColor.Target = Color.blue;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = CurrentColor,
                    Children = new WidgetList { new Label { Content = "Hover Me" } },
                };
            }
        }
    }
}
