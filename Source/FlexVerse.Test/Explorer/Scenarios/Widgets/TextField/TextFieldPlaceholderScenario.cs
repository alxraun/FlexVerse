// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldPlaceholderScenario : Scenario
    {
        public static new string Description =>
            "Placeholder is visible for an empty unfocused field.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new TextField
                {
                    Width = 300.Point(),
                    Height = 36.Point(),
                    Value = TextValue,
                    Placeholder = "Enter text",
                    OnValueChange = (Action<string>)(v => TextValue = v),
                },
            ];
    }
}
