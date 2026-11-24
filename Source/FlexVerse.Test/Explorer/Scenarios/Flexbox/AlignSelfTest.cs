// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/AlignSelf")]
    public partial class AlignSelfTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(AlignSelfFlexStartScenario),
                    typeof(AlignSelfCenterScenario),
                    typeof(AlignSelfFlexEndScenario),
                    typeof(AlignSelfStretchScenario),
                },
            };
        }
    }
}
