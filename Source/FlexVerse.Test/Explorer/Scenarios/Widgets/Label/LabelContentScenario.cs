// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelContentScenario : Scenario
    {
        public static new string Description =>
            "Change Content: the label and the marker below follow its natural size.";

        [State]
        public Behavior<int> Index { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Cycle short, long and empty text. The marker follows the measured height.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Next content",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Index.Target = (Index.Value + 1) % 3),
                },
                new View
                {
                    Width = 220f.Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content =
                                Index.Value == 0 ? "Short"
                                : Index.Value == 1
                                    ? "A longer sentence that needs several lines when the available width is narrow."
                                : "",
                            Width = 100f.Percent(),
                        },
                    },
                },
                new Label { Content = "Marker below the label", Color = Color.cyan },
            ];
    }
}
