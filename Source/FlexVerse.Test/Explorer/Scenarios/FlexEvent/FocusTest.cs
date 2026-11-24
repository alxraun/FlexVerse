// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/FlexEvent/Focus")]
    public partial class FocusTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(FocusSubmitCancelScenario),
                    typeof(FocusKeyBubblingScenario),
                },
            };
        }
    }
}
