// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ViewBorderRadiusScenario : Scenario
    {
        public static new string Description => "View: BorderRadius affects its own surface.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 220.Point(),
                    Height = 130.Point(),
                    MarginAll = 24.Point(),
                    BackgroundColor = Color.gray,
                    BorderRadius = 28f,
                    BorderTopLeftRadius = 0f,
                },
            ];
    }
}
