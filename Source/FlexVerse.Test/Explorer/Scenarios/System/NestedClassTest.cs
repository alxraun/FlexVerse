// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/System/NestedClass")]
    public partial class NestedClassTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(NestedClassInComponentClassScenario),
                    typeof(NestedClassInPlainClassScenario),
                    typeof(NestedClassInStaticClassScenario),
                    typeof(NestedClassInStructScenario),
                    typeof(NestedClassDeeplyNestedScenario),
                },
            };
        }
    }
}
