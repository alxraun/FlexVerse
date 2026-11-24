// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldSubmitCancelScenario : Scenario
    {
        public static new string Description =>
            "Enter submits; Escape cancels. Neither changes the controlled value.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        [State]
        private Behavior<string> Status { get; set; } = "Edit and press Enter or Escape";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new TextField
                {
                    Width = 300.Point(),
                    Height = 36.Point(),
                    Value = TextValue,
                    OnValueChange = (Action<string>)(v => TextValue = v),
                    OnSubmit = (Action)(() => Status = "Submitted"),
                    OnCancel = (Action)(() => Status = "Cancelled"),
                },
                new Label { Content = Status },
            ];
    }
}
