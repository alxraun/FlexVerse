// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Behaviors/Timeline")]
    public partial class TimelineTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Scenarios = new List<Type>
                    {
                        typeof(TimelineSequenceScenario),
                        typeof(TimelineParallelScenario),
                        typeof(TimelineWaitUntilScenario),
                        typeof(TimelineIfScenario),
                        typeof(TimelineWhileScenario),
                        typeof(TimelineStopScenario),
                        typeof(TimelineReplacementScenario),
                    },
                },
            ];
    }
}
