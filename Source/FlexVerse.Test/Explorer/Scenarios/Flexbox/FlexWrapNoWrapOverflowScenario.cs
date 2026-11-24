// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FlexWrapNoWrapOverflowScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                FlexDirection = FlexDirections.Row,
                FlexWrap = Wrap.NoWrap,
                AlignItems = Align.Center,
                Width = 400f.Point(),
                Height = 400f.Point(),
                PaddingAll = 10f.Point(),
                Children = BuildChildren(),
            };
        }

        private WidgetList BuildChildren()
        {
            var children = new WidgetList();

            for (int i = 0; i < 5; i++)
            {
                children.Add(
                    new ActualBox
                    {
                        Label = $"{(char)('A' + i)}",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        FlexShrink = 0f,
                    }
                );
            }

            float currentX = 20f;
            for (int i = 0; i < 5; i++)
            {
                children.Add(
                    new ExpectedBox
                    {
                        Label = $"{(char)('A' + i)}",
                        ExpectedTop = 160f.Point(),
                        ExpectedLeft = currentX.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    }
                );
                currentX += 80f + 20f;
            }

            return children;
        }
    }
}
