// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class AlignItemsStretchScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                AlignItems = Align.Stretch,
                JustifyContent = Justify.Center,
                Width = 400f.Point(),
                Height = 400f.Point(),
                Children = new WidgetList
                {
                    new ActualBox { Label = "A", Height = 80f.Point() },
                    new ActualBox { Label = "B", Height = 80f.Point() },
                    new ActualBox { Label = "C", Height = 80f.Point() },
                    new ExpectedBox
                    {
                        Label = "A",
                        ExpectedTop = 60f.Point(),
                        ExpectedLeft = 10f.Point(),
                        ExpectedWidth = 380f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 160f.Point(),
                        ExpectedLeft = 10f.Point(),
                        ExpectedWidth = 380f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "C",
                        ExpectedTop = 260f.Point(),
                        ExpectedLeft = 10f.Point(),
                        ExpectedWidth = 380f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
