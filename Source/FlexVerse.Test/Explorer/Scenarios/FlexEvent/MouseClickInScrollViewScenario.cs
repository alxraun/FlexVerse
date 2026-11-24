// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class MouseClickInScrollViewScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    Width = 400f.Point(),
                    Height = 600f.Point(),
                    BorderAll = 1f.Point(),
                    PaddingAll = 10f.Point(),
                    BorderColor = new Color(Color.grey.r, Color.grey.g, Color.grey.b, 0.8f),
                    Children = new WidgetList { new ScrollView { Children = BuildItems() } },
                },
            ];
        }

        private WidgetList BuildItems()
        {
            var items = new WidgetList();
            for (int i = 0; i < 50; i++)
            {
                items.Add(new McisvItem { Index = i });
            }
            return items;
        }

        public partial class McisvItem : Widget
        {
            [Prop]
            public Behavior<int> Index { get; set; }

            [State]
            private Behavior<Color> BackgroundColor { get; set; } = Color.grey;

            public McisvItem()
            {
                FlexShrink = 0;
            }

            public override void OnMouseDown(FlexEvent e)
            {
                HandleInput(e);
            }

            private void HandleInput(FlexEvent e)
            {
                Messages.Message($"Clicked item {Index.Value}", MessageTypeDefOf.NeutralEvent);
                BackgroundColor = Animate.FromTo(Color.cyan, Color.grey, 0.3f);
                e.StopPropagation();
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    FlexGrow = 1,
                    Height = 25f.Point(),
                    MarginBottom = 5f.Point(),
                    BackgroundColor = BackgroundColor,
                    AlignItems = Align.Center,
                    JustifyContent = Justify.Center,
                    Children = new WidgetList { new Label { Content = $"Item {Index.Value}" } },
                };
            }
        }
    }
}
