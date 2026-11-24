// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/FlexProperties")]
    public partial class FlexPropertiesTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(FlexGrowSimpleScenario),
                    typeof(FlexGrowProportionalScenario),
                    typeof(FlexShrinkScenario),
                    typeof(FlexBasisScenario),
                },
            };
        }
    }
}
