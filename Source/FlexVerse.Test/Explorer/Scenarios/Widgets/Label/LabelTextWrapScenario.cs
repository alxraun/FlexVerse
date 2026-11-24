// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelTextWrapScenario : Scenario
    {
        public static new string Description =>
            "Toggle automatic wrapping; explicit line breaks remain in both modes.";

        [State]
        public Behavior<bool> Wrapping { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Wrapping changes height. Text keeps its padding; explicit line breaks remain.",
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
                    Width = 220f.Point(),
                    PaddingAll = 12f.Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content =
                                "A longer sentence that needs several lines when the available width is narrow.",
                            TextWrap = Wrapping,
                            Width = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    Width = 220f.Point(),
                    PaddingAll = 12f.Point(),
                    BackgroundColor = Color.gray,
                    MarginTop = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "First line\nSecond line",
                            TextWrap = Wrapping,
                            Width = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
