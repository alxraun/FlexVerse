// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelTooltipScenario : Scenario
    {
        public static new string Description =>
            "Hover filled and empty labels; toggle their explicit tooltip.";

        [State]
        public Behavior<bool> Enabled { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Hover either rectangle. Tooltip is available even when content is empty.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Toggle Tooltip",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Enabled.Target = !Enabled.Value),
                },
                new View
                {
                    Width = 240f.Point(),
                    Height = 50f.Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Hover me",
                            Tooltip = Enabled.Value ? "Explicit tooltip" : "",
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    Width = 240f.Point(),
                    Height = 50f.Point(),
                    BackgroundColor = Color.gray,
                    MarginTop = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "",
                            Tooltip = Enabled.Value ? "Empty label tooltip" : "",
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
