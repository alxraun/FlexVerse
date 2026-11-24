// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/AreaSlider")]
    public partial class AreaSliderTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "AreaSlider",
                    Suffix = "Scenario",
                    Scenarios = new List<Type> { typeof(AreaSliderValueScenario) },
                },
            ];
    }
}
