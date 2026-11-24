// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Strategy picker: three preset buttons (Proportional / FixedOffset /
    /// LastOnly) plus the Restart checkbox, applied to the live sequence
    /// on click. The selected preset is highlighted green; the checkbox
    /// re-applies the current preset with the new restart flag.
    /// </summary>
    public partial class SequenceStrategyPicker : Widget
    {
        private static readonly SequenceStrategy[] Presets =
        [
            SequenceStrategy.Proportional,
            SequenceStrategy.FixedOffset,
            SequenceStrategy.LastOnly,
        ];

        private static readonly string[] Labels = ["Proportional", "FixedOffset", "LastOnly"];

        private static readonly Color SelectedColor = SequenceStyle.Selected;
        private static readonly Color PlainColor = SequenceStyle.Surface;
        private static readonly Color LabelColor = SequenceStyle.Muted;

        [Prop]
        public Behavior Sequence { get; set; }

        [State]
        private Behavior<int> Index { get; set; } = 0;

        [State]
        private Behavior<bool> Restart { get; set; } = true;

        public SequenceStrategyPicker()
        {
            FlexDirection = FlexDirections.Column;
            Width = 100f.Percent();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    MarginTop = 8f.Point(),
                    Children = BuildPresetButtons(Index.Value),
                },
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    AlignItems = Align.Center,
                    MarginTop = 6f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "Restart from first child",
                            Color = LabelColor,
                            MarginRight = 8f.Point(),
                        },
                        new Checkbox
                        {
                            Checked = Restart,
                            OnValueChange = new Action<bool>(ApplyRestart),
                        },
                    },
                },
            ];
        }

        private WidgetList BuildPresetButtons(int selectedIndex)
        {
            var buttons = new WidgetList();
            for (int i = 0; i < Presets.Length; i++)
                buttons.Add(BuildPresetButton(i, selectedIndex));

            return buttons;
        }

        private Widget BuildPresetButton(int index, int selectedIndex) =>
            new SequenceButton
            {
                Height = 30f.Point(),
                Label = Labels[index],
                BackgroundColor = selectedIndex == index ? SelectedColor : PlainColor,
                OnClick = new Action(() => ApplyStrategy(index)),
                Width = 96.Point(),
                MarginLeft = (index > 0 ? 4f : 0f).Point(),
            };

        private void ApplyStrategy(int index)
        {
            Index = index;
            Apply();
        }

        private void ApplyRestart(bool value)
        {
            Restart = value;
            Apply();
        }

        private void Apply()
        {
            if (Sequence is SequenceBehavior<Vector2> sequence)
                sequence.Strategy = WithRestart(Presets[Index.Value], Restart.Value);
        }
    }
}
