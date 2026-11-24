// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>Canvas with explicit, independently switchable reference layers.</summary>
    public partial class SequenceViewport : Widget
    {
        [Prop]
        public Behavior<SequenceScene> Scene { get; set; }

        [State]
        private Behavior<bool> Previous { get; set; } = true;

        [State]
        private Behavior<bool> Trace { get; set; } = true;

        public SequenceViewport()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            FlexGrow = 1;
            FlexShrink = 1;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequenceCanvas
                {
                    Scene = Scene,
                    ShowPrevious = Previous,
                    ShowTrail = Trace,
                },
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    AlignItems = Align.Center,
                    MarginTop = 8f.Point(),
                    MarginBottom = 8f.Point(),
                    Children = new WidgetList
                    {
                        new Label
                        {
                            Content = "LAYERS",
                            Font = GameFont.Tiny,
                            Color = SequenceStyle.Muted,
                            MarginRight = 10f.Point(),
                        },
                        BuildToggle(
                            "Previous",
                            "Plan before the last retarget gesture; fades after 8 seconds",
                            Previous.Value,
                            SequenceStyle.Previous,
                            TogglePrevious
                        ),
                        BuildToggle(
                            "Trace",
                            "Actual sampled positions; fades after 5 seconds",
                            Trace.Value,
                            SequenceStyle.Value,
                            ToggleTrace
                        ),
                        new Label
                        {
                            Content = "Cyan: remaining route",
                            Color = SequenceStyle.Accent,
                            Font = GameFont.Tiny,
                            MarginLeft = 12f.Point(),
                        },
                    },
                },
            ];

        private static Widget BuildToggle(
            string label,
            string tooltip,
            bool selected,
            Color color,
            Action action
        ) =>
            new SequenceButton
            {
                Label = label,
                Tooltip = tooltip,
                Width = 84f.Point(),
                Height = 26f.Point(),
                Font = GameFont.Tiny,
                Color = selected ? color : SequenceStyle.Muted,
                BackgroundColor = selected ? SequenceStyle.Surface : SequenceStyle.Canvas,
                OnClick = action,
                MarginRight = 4f.Point(),
            };

        private void TogglePrevious() => Previous.Target = !Previous.Value;

        private void ToggleTrace() => Trace.Target = !Trace.Value;
    }
}
