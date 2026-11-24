// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class CanvasViewZoomScenario : Scenario
    {
        public static new string Description => "Use the wheel to zoom around the cursor.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new CanvasView
                {
                    Width = 420.Point(),
                    Height = 250.Point(),
                    Pan = new Vector2(30, 20),
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
