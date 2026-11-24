// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Widgets/TextField")]
    public partial class TextFieldTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ScenarioHost
                {
                    Prefix = "TextField",
                    Suffix = "Scenario",
                    Scenarios = new List<Type>
                    {
                        typeof(TextFieldDisabledScenario),
                        typeof(TextFieldDrawTextFieldBackgroundScenario),
                        typeof(TextFieldMaxLengthScenario),
                        typeof(TextFieldPlaceholderScenario),
                        typeof(TextFieldSubmitCancelScenario),
                        typeof(TextFieldValidatorScenario),
                        typeof(TextFieldValueScenario),
                    },
                },
            ];
    }
}
