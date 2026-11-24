// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Properties/InheritedContext")]
    public partial class InheritedContextTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(InheritedContextSimpleConsumerScenario),
                    typeof(InheritedContextDeepConsumerScenario),
                    typeof(InheritedContextBecomeProviderScenario),
                    typeof(InheritedContextUpdateProviderScenario),
                    typeof(InheritedContextSetProviderScenario),
                    typeof(InheritedContextNoProviderScenario),
                    typeof(InheritedContextOnMountScenario),
                    typeof(InheritedContextShadowingProviderScenario),
                    typeof(InheritedContextConditionalProviderScenario),
                },
            };
        }
    }
}
