// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Patterns/PropDrilling")]
    public partial class PropDrillingTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(PropDrillingGoodScenario),
                    typeof(PropDrillingBadScenario),
                },
            };
        }
    }
}
