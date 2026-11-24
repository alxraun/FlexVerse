// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class TimelineIfScenario : Scenario
    {
        public static new string Description =>
            "Choose full or half width before Run. Changing the choice during motion does not change the selected branch.";

        [State(ReconcileOnSet = true)]
        public Behavior Playback { get; set; }

        [State]
        public Behavior<float> First { get; set; } = 0f;

        [State]
        public Behavior<bool> Gate { get; set; } = false;

        [State]
        public Behavior<string> Result { get; set; } = "Ready";

        private void Start()
        {
            Playback = NullBehavior.Instance;
            First = 0f;
            Result = "Running";
            Playback = Timeline.Create(
                Timeline.Sequence(
                    Timeline.If(
                        () => Gate.Value,
                        Timeline.Set(Refs.First, Animate.To(1f, 4f)),
                        Timeline.Set(Refs.First, Animate.To(0.5f, 4f))
                    ),
                    Timeline.Call(() => Result = "Reached end")
                )
            );
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Button
                {
                    Label = "Run fresh timeline",
                    OnClick = new Action(Start),
                    Height = 32f.Point(),
                },
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    Children = new WidgetList
                    {
                        new Checkbox
                        {
                            Checked = Gate,
                            OnValueChange = new Action<bool>(value => Gate = value),
                        },
                        new Label { Content = "Choose full width" },
                    },
                },
                new View
                {
                    Width = 360f.Point(),
                    Height = 24f.Point(),
                    BackgroundColor = Color.gray,
                    Children = new WidgetList
                    {
                        new View
                        {
                            Width = (First.Value * 360f).Point(),
                            Height = 24f.Point(),
                            BackgroundColor = Color.cyan,
                        },
                    },
                },
                new Label { Content = $"First: {First.Value:F2} | {Result.Value}" },
                new Label { Content = $"Timeline updating: {Playback.IsUpdating}" },
            ];
    }
}
