// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelAutoTooltipScenario : Scenario
    {
        public static new string Description =>
            "Automatic tooltip appears only for elided text; explicit tooltip takes priority.";

        [State]
        public Behavior<bool> Automatic { get; set; } = true;

        [State]
        public Behavior<bool> Wide { get; set; } = false;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Hover both labels. Expand: automatic tooltip disappears; explicit tooltip remains.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Toggle automatic tooltip",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Automatic.Target = !Automatic.Value),
                },
                new Button
                {
                    Label = "Toggle narrow / wide",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Wide.Target = !Wide.Value),
                },
                new Label { Content = $"AutoTooltip: {Automatic.Value}" },
                new View
                {
                    Width = (Wide.Value ? 500f : 140f).Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Full text shown automatically",
                            TextWrap = false,
                            TextOverflow = TextOverflowMode.Ellipsis,
                            AutoTooltip = Automatic,
                            Width = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    Width = (Wide.Value ? 500f : 140f).Point(),
                    BackgroundColor = Color.gray,
                    MarginTop = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Full text with explicit tooltip",
                            TextWrap = false,
                            TextOverflow = TextOverflowMode.Ellipsis,
                            AutoTooltip = Automatic,
                            Tooltip = "Explicit tooltip wins",
                            Width = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
