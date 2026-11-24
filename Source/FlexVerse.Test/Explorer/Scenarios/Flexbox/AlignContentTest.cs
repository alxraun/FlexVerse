// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/AlignContent")]
    public partial class AlignContentTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(AlignContentFlexStartScenario),
                    typeof(AlignContentCenterScenario),
                    typeof(AlignContentFlexEndScenario),
                    typeof(AlignContentStretchScenario),
                    typeof(AlignContentSpaceBetweenScenario),
                    typeof(AlignContentSpaceAroundScenario),
                },
            };
        }
    }
}
