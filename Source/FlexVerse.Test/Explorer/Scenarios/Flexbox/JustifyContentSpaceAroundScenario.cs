// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class JustifyContentSpaceAroundScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                JustifyContent = Justify.SpaceAround,
                AlignItems = Align.Center,
                Width = 400f.Point(),
                Height = 400f.Point(),
                Children = new WidgetList
                {
                    new ActualBox
                    {
                        Label = "A",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "B",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "C",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "A",
                        ExpectedTop = 27f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 160f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "C",
                        ExpectedTop = 293f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
