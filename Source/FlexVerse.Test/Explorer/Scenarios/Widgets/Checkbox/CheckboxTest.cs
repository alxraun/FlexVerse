// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/Checkbox")]
    public partial class CheckboxTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "Checkbox",
                    Suffix = "Scenario",
                    Scenarios = new List<Type> { typeof(CheckboxCheckedScenario) },
                },
            ];
    }
}
