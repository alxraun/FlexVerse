// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Properties/BehaviorEvent")]
    public partial class BehaviorEventTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(BehaviorBindEventUnmountStopsScenario),
                    typeof(BehaviorBindEventSwapStopsScenario),
                    typeof(BehaviorBindEventPropertyScopeScenario),
                },
            };
        }
    }
}
