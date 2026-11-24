// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class JustifyContentFlexEndScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                JustifyContent = Justify.FlexEnd,
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
                        ExpectedTop = 110f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 210f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "C",
                        ExpectedTop = 310f.Point(),
                        ExpectedLeft = 160f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
