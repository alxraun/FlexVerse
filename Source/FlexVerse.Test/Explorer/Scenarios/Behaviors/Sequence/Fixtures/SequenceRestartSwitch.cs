// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Restart flag switch for the live sequence strategy: two mutually
    /// exclusive buttons, the active one highlighted green. The flag is
    /// applied on top of the Proportional preset.
    /// </summary>
    public partial class SequenceRestartSwitch : Widget
    {
        private static readonly Color SelectedColor = SequenceStyle.Selected;
        private static readonly Color PlainColor = SequenceStyle.Surface;

        [Prop]
        public Behavior Sequence { get; set; }

        [State]
        private Behavior<bool> Restart { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    MarginBottom = 8f.Point(),
                    Children = new WidgetList
                    {
                        new SequenceButton
                        {
                            Height = 30f.Point(),
                            Label = "Restart ON",
                            BackgroundColor = Restart.Value ? SelectedColor : PlainColor,
                            OnClick = new Action(() => Apply(true)),
                            Width = 100.Point(),
                        },
                        new SequenceButton
                        {
                            Height = 30f.Point(),
                            Label = "Restart OFF",
                            BackgroundColor = !Restart.Value ? SelectedColor : PlainColor,
                            OnClick = new Action(() => Apply(false)),
                            Width = 100.Point(),
                            MarginLeft = 4f.Point(),
                        },
                    },
                },
            ];
        }

        private void Apply(bool value)
        {
            Restart = value;

            if (Sequence is SequenceBehavior<Vector2> sequence)
                sequence.Strategy = WithRestart(SequenceStrategy.Proportional, value);
        }
    }
}
