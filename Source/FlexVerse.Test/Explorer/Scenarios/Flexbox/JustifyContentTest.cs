// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/JustifyContent")]
    public partial class JustifyContentTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(JustifyContentFlexStartScenario),
                    typeof(JustifyContentCenterScenario),
                    typeof(JustifyContentFlexEndScenario),
                    typeof(JustifyContentSpaceBetweenScenario),
                    typeof(JustifyContentSpaceAroundScenario),
                },
            };
        }
    }
}
