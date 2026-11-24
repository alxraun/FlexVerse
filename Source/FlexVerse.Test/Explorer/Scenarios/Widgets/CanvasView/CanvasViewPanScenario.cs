// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class CanvasViewPanScenario : Scenario
    {
        public static new string Description =>
            "Drag empty space or use the middle button to pan without content bounds.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new CanvasView
                {
                    Width = 420.Point(),
                    Height = 250.Point(),
                    Zoom = 1f,
                    Children = new WidgetList
                    {
                        new View
                        {
                            Width = 700.Point(),
                            Height = 400.Point(),
                            BackgroundColor = Color.gray,
                            Children = new WidgetList
                            {
                                new Label
                                {
                                    Content = "Canvas content",
                                    Width = 160.Point(),
                                    Height = 40.Point(),
                                },
                            },
                        },
                    },
                },
            ];
    }
}
