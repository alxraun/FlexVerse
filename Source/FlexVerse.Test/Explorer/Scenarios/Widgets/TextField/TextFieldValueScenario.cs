// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldValueScenario : Scenario
    {
        public static new string Description =>
            "Edits propose values; the owner accepts them through OnValueChange.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new TextField
                {
                    Width = 300.Point(),
                    Height = 36.Point(),
                    Value = TextValue,
                    OnValueChange = (Action<string>)(v => TextValue = v),
                },
            ];
    }
}
