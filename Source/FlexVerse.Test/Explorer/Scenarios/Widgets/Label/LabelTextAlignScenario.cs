// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelTextAlignScenario : Scenario
    {
        public static new string Description => "Compare all nine anchors inside equal rectangles.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "The anchor name should occupy its matching position in each rectangle.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new View
                {
                    Width = 540f.Point(),
                    FlexDirection = FlexDirections.Row,
                    FlexWrap = Wrap.Wrap,
                    Children = new WidgetList(
                        Enum.GetValues(typeof(TextAnchor))
                            .Cast<TextAnchor>()
                            .Select(anchor => new View
                            {
                                Width = 170f.Point(),
                                Height = 70f.Point(),
                                MarginAll = 4f.Point(),
                                BackgroundColor = Color.gray,
                                Children = new WidgetList
                                {
                                    new Label
                                    {
                                        Content = anchor.ToString(),
                                        TextAlign = anchor,
                                        Width = 100f.Percent(),
                                        Height = 100f.Percent(),
                                    },
                                },
                            })
                    ),
                },
            ];
    }
}
