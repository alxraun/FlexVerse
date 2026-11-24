// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class CanvasViewZoomBoundsScenario : Scenario
    {
        public static new string Description => "Wheel zoom stays between 0.5 and 2.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new CanvasView
                {
                    Width = 420.Point(),
                    Height = 250.Point(),
                    MinZoom = 0.5f,
                    MaxZoom = 2f,
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
