// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/FlexWrap")]
    public partial class FlexWrapTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(FlexWrapNoWrapShrinkScenario),
                    typeof(FlexWrapNoWrapOverflowScenario),
                    typeof(FlexWrapWrapScenario),
                    typeof(FlexWrapWrapReverseScenario),
                },
            };
        }
    }
}
