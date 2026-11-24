// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/Label")]
    public partial class LabelTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "Label",
                    Suffix = "Scenario",
                    Scenarios = new List<Type>
                    {
                        typeof(LabelContentScenario),
                        typeof(LabelFontScenario),
                        typeof(LabelTextAlignScenario),
                        typeof(LabelTextTransformScenario),
                        typeof(LabelTextWrapScenario),
                        typeof(LabelTextOverflowScenario),
                        typeof(LabelColorScenario),
                        typeof(LabelBackgroundColorScenario),
                        typeof(LabelTooltipScenario),
                        typeof(LabelAutoTooltipScenario),
                    },
                },
            ];
    }
}
