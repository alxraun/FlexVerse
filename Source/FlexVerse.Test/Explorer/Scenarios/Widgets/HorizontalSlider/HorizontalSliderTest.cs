// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/HorizontalSlider")]
    public partial class HorizontalSliderTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "HorizontalSlider",
                    Suffix = "Scenario",
                    Scenarios = new List<Type> { typeof(HorizontalSliderValueScenario) },
                },
            ];
    }
}
