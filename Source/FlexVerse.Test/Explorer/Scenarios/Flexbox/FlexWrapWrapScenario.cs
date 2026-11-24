// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FlexWrapWrapScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                FlexDirection = FlexDirections.Row,
                FlexWrap = Wrap.Wrap,
                AlignContent = Align.Center,
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
                    }
                );
            }

            children.Add(
                new ExpectedBox
                {
                    Label = "A",
                    ExpectedTop = 110f.Point(),
                    ExpectedLeft = 20f.Point(),
                    ExpectedWidth = 80f.Point(),
                    ExpectedHeight = 80f.Point(),
                }
            );
            children.Add(
                new ExpectedBox
                {
                    Label = "B",
                    ExpectedTop = 110f.Point(),
                    ExpectedLeft = 120f.Point(),
                    ExpectedWidth = 80f.Point(),
                    ExpectedHeight = 80f.Point(),
                }
            );
            children.Add(
                new ExpectedBox
                {
                    Label = "C",
                    ExpectedTop = 110f.Point(),
                    ExpectedLeft = 220f.Point(),
                    ExpectedWidth = 80f.Point(),
                    ExpectedHeight = 80f.Point(),
                }
            );

            children.Add(
                new ExpectedBox
                {
                    Label = "D",
                    ExpectedTop = 210f.Point(),
                    ExpectedLeft = 20f.Point(),
                    ExpectedWidth = 80f.Point(),
                    ExpectedHeight = 80f.Point(),
                }
            );
            children.Add(
                new ExpectedBox
                {
                    Label = "E",
                    ExpectedTop = 210f.Point(),
                    ExpectedLeft = 120f.Point(),
                    ExpectedWidth = 80f.Point(),
                    ExpectedHeight = 80f.Point(),
                }
            );

            return children;
        }
    }
}
