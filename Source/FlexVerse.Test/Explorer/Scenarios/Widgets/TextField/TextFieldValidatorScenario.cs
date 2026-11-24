// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldValidatorScenario : Scenario
    {
        public static new string Description =>
            "Proposed edits longer than five characters are rejected.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new TextField
                {
                    Width = 300.Point(),
                    Height = 36.Point(),
                    Value = TextValue,
                    Validator = (Func<string, bool>)(v => v.Length <= 5),
                    OnValueChange = (Action<string>)(v => TextValue = v),
                },
            ];
    }
}
