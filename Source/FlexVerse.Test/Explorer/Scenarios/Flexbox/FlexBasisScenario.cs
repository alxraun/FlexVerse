// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FlexBasisScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                FlexDirection = FlexDirections.Row,
                Width = 400f.Point(),
                Height = 200f.Point(),
                PaddingAll = 10f.Point(),
                AlignItems = Align.Center,
                Children = new WidgetList
                {
                    new ActualBox
                    {
                        Label = "A (basis:100)",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        FlexBasis = 100f.Point(),
                        FlexGrow = 1,
                    },
                    new ActualBox
                    {
                        Label = "B",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "A (basis:100)",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 20f.Point(),
                        ExpectedWidth = 260f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 300f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
