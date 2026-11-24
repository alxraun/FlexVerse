// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/View")]
    public partial class ViewTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "View",
                    Suffix = "Scenario",
                    Scenarios = new List<Type>
                    {
                        typeof(ViewBackgroundImageScenario),
                        typeof(ViewBorderRadiusScenario),
                        typeof(ViewBorderScenario),
                        typeof(ViewBoxShadowScenario),
                    },
                },
            ];
    }
}
