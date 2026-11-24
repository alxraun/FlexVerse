// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Properties/BehaviorLifetime")]
    public partial class BehaviorLifetimeTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(BehaviorLifetimeMountAndUnmountSoleRefHolderScenario),
                    typeof(BehaviorLifetimeMountAndUnmountNonSoleRefHolderScenario),
                    typeof(BehaviorLifetimeStateReleaseLastRefScenario),
                    typeof(BehaviorLifetimePropReleaseNonLastRefScenario),
                },
            };
        }
    }
}
