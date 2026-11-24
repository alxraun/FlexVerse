// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ViewBoxShadowScenario : Scenario
    {
        public static new string Description => "View: BoxShadow affects its own surface.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 220.Point(),
                    Height = 130.Point(),
                    MarginAll = 24.Point(),
                    BackgroundColor = Color.gray,
                    BoxShadow = new Shadow(new Vector2(8, 8), 12, 2, Color.black),
                },
            ];
    }
}
