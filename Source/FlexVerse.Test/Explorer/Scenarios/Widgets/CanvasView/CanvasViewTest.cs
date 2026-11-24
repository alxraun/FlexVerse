// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/CanvasView")]
    public partial class CanvasViewTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "CanvasView",
                    Suffix = "Scenario",
                    Scenarios = new List<Type>
                    {
                        typeof(CanvasViewPanScenario),
                        typeof(CanvasViewZoomBoundsScenario),
                        typeof(CanvasViewZoomScenario),
                    },
                },
            ];
    }
}
