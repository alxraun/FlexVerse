// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Behaviors/Sequence")]
    public partial class SequenceTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new ScenarioHost
                {
                    Scenarios = new List<Type>
                    {
                        typeof(SequenceOrderScenario),
                        typeof(SequenceCorridorScenario),
                        typeof(SequenceStrategyScenario),
                        typeof(SequenceRestartScenario),
                        typeof(SequenceInstantScenario),
                        typeof(SequenceProgressScenario),
                        typeof(SequenceZigzagScenario),
                    },
                },
            ];
        }
    }
}
