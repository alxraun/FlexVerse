// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TweenSlidersScenario : Scenario
    {
        private static readonly Vector2 DefaultValue = new Vector2(0.5f, 0.5f);

        [State]
        private Behavior<Vector2> ConstantDurationSliderValue { get; set; } =
            Animate.State(
                DefaultValue,
                duration: 0.5f,
                minDuration: 0.5f,
                maxDuration: 0.5f,
                easing: Easing.Linear
            );

        [State]
        private Behavior<Vector2> ConstantSpeedValue { get; set; } =
            Animate.State(
                DefaultValue,
                duration: 0.5f,
                minDuration: 0f,
                maxDuration: float.PositiveInfinity,
                easing: Easing.Linear
            );

        [State]
        private Behavior<Vector2> SpeedFloorSliderValue { get; set; } =
            Animate.State(
                DefaultValue,
                duration: 0.5f,
                minDuration: 0f,
                maxDuration: 0.5f,
                easing: Easing.Linear
            );

        [State]
        private Behavior<Vector2> SpeedCeilingSliderValue { get; set; } =
            Animate.State(
                DefaultValue,
                duration: 0.5f,
                minDuration: 0.5f,
                maxDuration: float.PositiveInfinity,
                easing: Easing.Linear
            );

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    FlexDirection = FlexDirections.Column,
                    Children = new WidgetList
                    {
                        new View
                        {
                            FlexDirection = FlexDirections.Row,
                            Children = new WidgetList
                            {
                                new AreaSlider
                                {
                                    Value = ConstantDurationSliderValue,
                                    XRange = new FloatRange(0f, 1f),
                                    YRange = new FloatRange(0f, 1f),
                                    OnValueChange = new Action<Vector2>(
                                        (newValue) => ConstantDurationSliderValue.Target = newValue
                                    ),
                                    Label = "Constant Duration",
                                    LabelColor = new Color(1f, 1f, 1f, 0.5f),
                                    MarginAll = 5.Point(),
                                    Width = 300.Point(),
                                    Height = 300.Point(),
                                },
                                new AreaSlider
                                {
                                    Value = ConstantSpeedValue,
                                    XRange = new FloatRange(0f, 1f),
                                    YRange = new FloatRange(0f, 1f),
                                    OnValueChange = new Action<Vector2>(
                                        (newValue) => ConstantSpeedValue.Target = newValue
                                    ),
                                    Label = "Constant Speed",
                                    LabelColor = new Color(1f, 1f, 1f, 0.5f),
                                    MarginAll = 5.Point(),
                                    Width = 300.Point(),
                                    Height = 300.Point(),
                                },
                            },
                        },
                        new View
                        {
                            FlexDirection = FlexDirections.Row,
                            Children = new WidgetList
                            {
                                new AreaSlider
                                {
                                    Value = SpeedFloorSliderValue,
                                    XRange = new FloatRange(0f, 1f),
                                    YRange = new FloatRange(0f, 1f),
                                    OnValueChange = new Action<Vector2>(
                                        (newValue) => SpeedFloorSliderValue.Target = newValue
                                    ),
                                    Label = "Speed Floor",
                                    LabelColor = new Color(1f, 1f, 1f, 0.5f),
                                    MarginAll = 5.Point(),
                                    Width = 300.Point(),
                                    Height = 300.Point(),
                                },
                                new AreaSlider
                                {
                                    Value = SpeedCeilingSliderValue,
                                    XRange = new FloatRange(0f, 1f),
                                    YRange = new FloatRange(0f, 1f),
                                    OnValueChange = new Action<Vector2>(
                                        (newValue) => SpeedCeilingSliderValue.Target = newValue
                                    ),
                                    Label = "Speed Ceiling",
                                    LabelColor = new Color(1f, 1f, 1f, 0.5f),
                                    MarginAll = 5.Point(),
                                    Width = 300.Point(),
                                    Height = 300.Point(),
                                },
                            },
                        },
                    },
                },
            ];
        }
    }
}
