// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/System/StateInit")]
    public partial class StateInitTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Prefix = "",
                Suffix = "Scenario",
                Scenarios = new List<Type>
                {
                    typeof(StateInitFieldCtorBadScenario),
                    typeof(StateInitFieldOnMountGoodScenario),
                    typeof(StateInitStateCtorGoodScenario),
                },
            };
        }
    }
}
