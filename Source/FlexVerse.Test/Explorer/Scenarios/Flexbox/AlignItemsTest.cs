// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Flexbox/AlignItems")]
    public partial class AlignItemsTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(AlignItemsFlexStartScenario),
                    typeof(AlignItemsCenterScenario),
                    typeof(AlignItemsFlexEndScenario),
                    typeof(AlignItemsStretchScenario),
                },
            };
        }
    }
}
