// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FlexShrinkScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                FlexDirection = FlexDirections.Row,
                Width = 200f.Point(),
                Height = 200f.Point(),
                PaddingAll = 10f.Point(),
                AlignItems = Align.Center,
                Children = new WidgetList
                {
                    new ActualBox
                    {
                        Label = "A",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        FlexShrink = 1,
                    },
                    new ActualBox
                    {
                        Label = "B",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        FlexShrink = 1,
                    },
                    new ActualBox
                    {
                        Label = "C",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        FlexShrink = 1,
                    },
                    new ExpectedBox
                    {
                        Label = "A",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 20f.Point(),
                        ExpectedWidth = 40f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 80f.Point(),
                        ExpectedWidth = 40f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "C",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 140f.Point(),
                        ExpectedWidth = 40f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
