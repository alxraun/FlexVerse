// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class AlignContentFlexStartScenario : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new GridView
            {
                FlexDirection = FlexDirections.Row,
                FlexWrap = Wrap.Wrap,
                AlignContent = Align.FlexStart,
                Width = 400f.Point(),
                Height = 400f.Point(),
                PaddingAll = 10f.Point(),
                Children = new WidgetList
                {
                    new ActualBox
                    {
                        Label = "A",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        MarginAll = 10f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "B",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        MarginAll = 10f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "C",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        MarginAll = 10f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "D",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        MarginAll = 10f.Point(),
                    },
                    new ActualBox
                    {
                        Label = "E",
                        Width = 80f.Point(),
                        Height = 80f.Point(),
                        MarginAll = 10f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "A",
                        ExpectedTop = 20f.Point(),
                        ExpectedLeft = 20f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "B",
                        ExpectedTop = 20f.Point(),
                        ExpectedLeft = 120f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "C",
                        ExpectedTop = 20f.Point(),
                        ExpectedLeft = 220f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "D",
                        ExpectedTop = 120f.Point(),
                        ExpectedLeft = 20f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                    new ExpectedBox
                    {
                        Label = "E",
                        ExpectedTop = 120f.Point(),
                        ExpectedLeft = 120f.Point(),
                        ExpectedWidth = 80f.Point(),
                        ExpectedHeight = 80f.Point(),
                    },
                },
            };
        }
    }
}
