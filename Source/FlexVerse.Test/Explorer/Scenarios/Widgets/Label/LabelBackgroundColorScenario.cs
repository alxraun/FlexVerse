// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelBackgroundColorScenario : Scenario
    {
        public static new string Description =>
            "Compose View backgrounds around filled and empty labels.";

        [State]
        public Behavior<bool> Colored { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = "Both View backgrounds change; the second label has empty content.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Toggle background",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Colored.Target = !Colored.Value),
                },
                new View
                {
                    Width = 240f.Point(),
                    Height = 50f.Point(),
                    BackgroundColor = Colored.Value ? Color.gray : Color.clear,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Filled",
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    Width = 240f.Point(),
                    Height = 50f.Point(),
                    BackgroundColor = Colored.Value ? Color.gray : Color.clear,
                    MarginTop = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "",
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
