// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldMaxLengthScenario : Scenario
    {
        public static new string Description =>
            "The native field accepts at most eight characters.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new TextField
                {
                    Width = 300.Point(),
                    Height = 36.Point(),
                    Value = TextValue,
                    MaxLength = 8,
                    OnValueChange = (Action<string>)(v => TextValue = v),
                },
            ];
    }
}
