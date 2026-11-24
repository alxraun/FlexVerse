// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelTextOverflowScenario : Scenario
    {
        public static new string Description => "Compare Clip and Ellipsis, then enable wrapping.";

        [State]
        public Behavior<bool> Wrapping { get; set; } = false;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Without wrapping: Clip cuts, Ellipsis adds dots. With wrapping: both wrap.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Toggle TextWrap",
                    Height = 32f.Point(),
                    OnClick = new Action(() => Wrapping.Target = !Wrapping.Value),
                },
                new Label { Content = $"TextWrap: {Wrapping.Value}" },
                new View
                {
                    Width = 240f.Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content =
                                "Clip: A longer sentence that needs several lines when the available width is narrow.",
                            TextWrap = Wrapping,
                            TextOverflow = TextOverflowMode.Clip,
                            Width = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    Width = 240f.Point(),
                    BackgroundColor = Color.gray,
                    MarginTop = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content =
                                "Ellipsis: A longer sentence that needs several lines when the available width is narrow.",
                            TextWrap = Wrapping,
                            TextOverflow = TextOverflowMode.Ellipsis,
                            Width = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
