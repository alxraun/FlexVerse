// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class AreaSliderValueScenario : Scenario
    {
        public static new string Description =>
            "Dragging proposes a two-dimensional value accepted by the owner.";

        [State]
        private Behavior<Vector2> SliderValue { get; set; } = new Vector2(0.3f, 0.6f);

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new AreaSlider
                {
                    Width = 160.Point(),
                    Height = 160.Point(),
                    Value = SliderValue,
                    OnValueChange = (Action<Vector2>)(v => SliderValue = v),
                },
            ];
    }
}
