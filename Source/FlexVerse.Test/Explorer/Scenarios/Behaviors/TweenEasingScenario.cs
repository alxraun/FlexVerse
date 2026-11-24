// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TweenEasingScenario : Scenario
    {
        private static readonly Length StartPos = 10f.Point();
        private static readonly Length EndPos = 450f.Point();

        private const float DisplayAspectRatio = 3f / 2f;

        [State]
        private Behavior<Length> LinearBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.Linear);

        [State]
        private Behavior<Length> QuadEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuadEaseIn);

        [State]
        private Behavior<Length> QuadEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuadEaseOut);

        [State]
        private Behavior<Length> QuadEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuadEaseInOut);

        [State]
        private Behavior<Length> CubicEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CubicEaseIn);

        [State]
        private Behavior<Length> CubicEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CubicEaseOut);

        [State]
        private Behavior<Length> CubicEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CubicEaseInOut);

        [State]
        private Behavior<Length> SineEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.SineEaseIn);

        [State]
        private Behavior<Length> SineEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.SineEaseOut);

        [State]
        private Behavior<Length> SineEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.SineEaseInOut);

        [State]
        private Behavior<Length> BackEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.BackEaseIn);

        [State]
        private Behavior<Length> BackEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.BackEaseOut);

        [State]
        private Behavior<Length> BackEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.BackEaseInOut);

        [State]
        private Behavior<Length> CircEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CircEaseIn);

        [State]
        private Behavior<Length> CircEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CircEaseOut);

        [State]
        private Behavior<Length> CircEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.CircEaseInOut);

        [State]
        private Behavior<Length> ExpoEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ExpoEaseIn);

        [State]
        private Behavior<Length> ExpoEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ExpoEaseOut);

        [State]
        private Behavior<Length> ExpoEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ExpoEaseInOut);

        [State]
        private Behavior<Length> ElasticEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ElasticEaseIn);

        [State]
        private Behavior<Length> ElasticEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ElasticEaseOut);

        [State]
        private Behavior<Length> ElasticEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.ElasticEaseInOut);

        [State]
        private Behavior<Length> QuartEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuartEaseIn);

        [State]
        private Behavior<Length> QuartEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuartEaseOut);

        [State]
        private Behavior<Length> QuartEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuartEaseInOut);

        [State]
        private Behavior<Length> QuintEaseInBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuintEaseIn);

        [State]
        private Behavior<Length> QuintEaseOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuintEaseOut);

        [State]
        private Behavior<Length> QuintEaseInOutBoxPosition { get; set; } =
            Animate.State(StartPos, 1f, Easing.QuintEaseInOut);

        private readonly List<(string Name, Behavior<Length> Behavior)> _allEasings = new();

        public override void OnMount()
        {
            InitializeEasings();
        }

        private void InitializeEasings()
        {
            _allEasings.Clear();
            _allEasings.AddRange([
                ("Linear", LinearBoxPosition),
                ("QuadEaseIn", QuadEaseInBoxPosition),
                ("QuadEaseOut", QuadEaseOutBoxPosition),
                ("QuadEaseInOut", QuadEaseInOutBoxPosition),
                ("CubicEaseIn", CubicEaseInBoxPosition),
                ("CubicEaseOut", CubicEaseOutBoxPosition),
                ("CubicEaseInOut", CubicEaseInOutBoxPosition),
                ("SineEaseIn", SineEaseInBoxPosition),
                ("SineEaseOut", SineEaseOutBoxPosition),
                ("SineEaseInOut", SineEaseInOutBoxPosition),
                ("BackEaseIn", BackEaseInBoxPosition),
                ("BackEaseOut", BackEaseOutBoxPosition),
                ("BackEaseInOut", BackEaseInOutBoxPosition),
                ("CircEaseIn", CircEaseInBoxPosition),
                ("CircEaseOut", CircEaseOutBoxPosition),
                ("CircEaseInOut", CircEaseInOutBoxPosition),
                ("ExpoEaseIn", ExpoEaseInBoxPosition),
                ("ExpoEaseOut", ExpoEaseOutBoxPosition),
                ("ExpoEaseInOut", ExpoEaseInOutBoxPosition),
                ("ElasticEaseIn", ElasticEaseInBoxPosition),
                ("ElasticEaseOut", ElasticEaseOutBoxPosition),
                ("ElasticEaseInOut", ElasticEaseInOutBoxPosition),
                ("QuartEaseIn", QuartEaseInBoxPosition),
                ("QuartEaseOut", QuartEaseOutBoxPosition),
                ("QuartEaseInOut", QuartEaseInOutBoxPosition),
                ("QuintEaseIn", QuintEaseInBoxPosition),
                ("QuintEaseOut", QuintEaseOutBoxPosition),
                ("QuintEaseInOut", QuintEaseInOutBoxPosition),
            ]);
        }

        private void TogglePosition()
        {
            foreach (var (_, behavior) in _allEasings)
            {
                behavior.Target = behavior.Target == StartPos ? EndPos : StartPos;
            }
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new ScrollView
                {
                    ScrollVertical = true,
                    MarginVertical = 10f.Point(),
                    Children = new WidgetList(
                        _allEasings.Select(easing => new View
                        {
                            FlexDirection = FlexDirections.Row,
                            OnClick = new Action(TogglePosition),
                            MarginBottom = 5f.Point(),
                            Children = new WidgetList
                            {
                                new GridView
                                {
                                    Width = 500f.Point(),
                                    Height = 40f.Point(),
                                    JustifyContent = Justify.Center,
                                    ShowAxes = false,
                                    Children = new WidgetList
                                    {
                                        new Label
                                        {
                                            Content = easing.Name,
                                            Color = new Color(1f, 1f, 1f, 0.5f),
                                        },
                                        new ActualBox
                                        {
                                            PositionType = PositionTypes.Absolute,
                                            Width = 20f.Point(),
                                            Height = 20f.Point(),
                                            PositionLeft = easing.Behavior,
                                        },
                                    },
                                },
                                new AnimationDisplay
                                {
                                    MarginLeft = 10f.Point(),
                                    AspectRatio = DisplayAspectRatio,
                                    Behavior = easing.Behavior,
                                    ShowGrid = false,
                                    ShowLabels = false,
                                    ShowLiveReadout = false,
                                },
                            },
                        })
                    ),
                },
            ];
        }
    }
}
