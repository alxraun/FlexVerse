// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelTextTransformScenario : Scenario
    {
        public static new string Description =>
            "Compare None, Uppercase, Lowercase and Capitalize.";

        [State]
        public Behavior<TextTransformMode> Mode { get; set; } = TextTransformMode.None;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = "The same source: hELLo wORLD. Each label uses its natural width.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new Button
                {
                    Label = "Cycle transform on mounted label",
                    Height = 32f.Point(),
                    OnClick = new Action(() =>
                        Mode.Target = (TextTransformMode)(((int)Mode.Value + 1) % 4)
                    ),
                },
                new Label { Content = $"TextTransform: {Mode.Value}" },
                new View
                {
                    BackgroundColor = Color.gray,
                    MarginBottom = 12f.Point(),
                    Children = new WidgetList
                    {
                        new Label { Content = "hELLo wORLD", TextTransform = Mode },
                    },
                },
                new View
                {
                    AlignItems = Align.FlexStart,
                    Children = new WidgetList(
                        Enum.GetValues(typeof(TextTransformMode))
                            .Cast<TextTransformMode>()
                            .Select(mode => new View
                            {
                                BackgroundColor = Color.gray,
                                MarginBottom = 8f.Point(),
                                Children = new WidgetList
                                {
                                    new Label { Content = "hELLo wORLD", TextTransform = mode },
                                },
                            })
                    ),
                },
            ];
    }
}
