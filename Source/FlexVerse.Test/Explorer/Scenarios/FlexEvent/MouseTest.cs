// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/FlexEvent/Mouse")]
    public partial class MouseTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(MouseBubblingScenario),
                    typeof(MouseBubblingStopPropagationScenario),
                    typeof(MouseHoverScenario),
                    typeof(MouseClickEventsScenario),
                    typeof(MouseDragAndMoveScenario),
                    typeof(MouseClickInScrollViewScenario),
                },
            };
        }
    }
}
