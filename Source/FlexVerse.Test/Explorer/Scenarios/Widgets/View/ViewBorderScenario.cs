// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ViewBorderScenario : Scenario
    {
        public static new string Description => "View: Border affects its own surface.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 220.Point(),
                    Height = 130.Point(),
                    MarginAll = 24.Point(),
                    BackgroundColor = Color.gray,
                    BorderAll = 3.Point(),
                    BorderLeft = 12.Point(),
                    BorderColor = Color.cyan,
                },
            ];
    }
}
