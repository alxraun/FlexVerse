// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelColorScenario : Scenario
    {
        public static new string Description =>
            "Compare opaque, half-transparent and invisible text on the same background.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content =
                        "Only text visibility changes; all three background rectangles remain visible.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new View
                {
                    BackgroundColor = Color.gray,
                    Width = 240f.Point(),
                    Height = 40f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Alpha 1.0",
                            Color = new Color(1f, 1f, 1f, 1f),
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    BackgroundColor = Color.gray,
                    Width = 240f.Point(),
                    Height = 40f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Alpha 0.5",
                            Color = new Color(1f, 1f, 1f, 0.5f),
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
                new View
                {
                    BackgroundColor = Color.gray,
                    Width = 240f.Point(),
                    Height = 40f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Alpha 0.0",
                            Color = Color.clear,
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                        },
                    },
                },
            ];
    }
}
