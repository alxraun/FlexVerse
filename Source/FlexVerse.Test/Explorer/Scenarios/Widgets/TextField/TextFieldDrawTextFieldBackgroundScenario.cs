// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TextFieldDrawTextFieldBackgroundScenario : Scenario
    {
        public static new string Description =>
            "A View supplies the surface while the native editor draws without its stock background.";

        [State]
        private Behavior<string> TextValue { get; set; } = "";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 300.Point(),
                    Height = 40.Point(),
                    BackgroundColor = Color.gray,
                    BorderRadius = 8f,
                    Children = new WidgetList
                    {
                        new TextField
                        {
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                            Value = TextValue,
                            DrawTextFieldBackground = false,
                            OnValueChange = (Action<string>)(v => TextValue = v),
                        },
                    },
                },
            ];
    }
}
