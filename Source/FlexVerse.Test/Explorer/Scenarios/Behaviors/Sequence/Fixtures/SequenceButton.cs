// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>Workbench surface composed around a native button.</summary>
    public partial class SequenceButton : Widget
    {
        [Prop]
        public Behavior<string> Label { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<Action> OnClick { get; set; }

        [Prop]
        public Behavior<Color> BackgroundColor { get; set; } = SequenceStyle.Surface;

        [Prop]
        public Behavior<Color> Color { get; set; } = SequenceStyle.Text;

        [Prop]
        public Behavior<GameFont> Font { get; set; } = GameFont.Small;

        public SequenceButton() => Height = 32f.Point();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                    BackgroundColor = BackgroundColor,
                    Children = new WidgetList
                    {
                        new Button
                        {
                            Width = 100f.Percent(),
                            Height = 100f.Percent(),
                            Tooltip = Tooltip,
                            Label = Label,
                            OnClick = OnClick,
                            Color = Color,
                            Font = Font,
                            DrawButtonBackground = false,
                        },
                    },
                },
            ];
    }
}
