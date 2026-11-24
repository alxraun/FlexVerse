// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class HorizontalSliderValueScenario : Scenario
    {
        public static new string Description =>
            "Dragging proposes a scalar value accepted by the owner.";

        [State]
        private Behavior<float> SliderValue { get; set; } = 0.4f;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new HorizontalSlider
                {
                    Width = 300.Point(),
                    Height = 24.Point(),
                    Value = SliderValue,
                    MinValue = 0f,
                    MaxValue = 1f,
                    OnValueChange = (Action<float>)(v => SliderValue = v),
                },
            ];
    }
}
