// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>Lifecycle only: idle does not promise that Value equals Target.</summary>
    public partial class SequenceStatusLine : Widget
    {
        [Prop]
        public Behavior Sequence { get; set; }

        [Prop]
        public Behavior<SequenceScene> Scene { get; set; }

        public SequenceStatusLine()
        {
            Width = 100f.Percent();
            Height = 34f.Point();
            MarginTop = 6f.Point();
            MarginBottom = 10f.Point();
        }

        public override void OnGUI(Rect rect)
        {
            var behavior = Scene?.Value?.Sequence ?? Sequence;
            if (behavior is null)
                return;
            var color = GUI.color;
            var font = Text.Font;
            var anchor = Text.Anchor;
            try
            {
                var active = behavior.State == Behavior.Lifecycle.Active;
                var overlap = Scene?.Value?.UpdatingChildren > 1;
                var tint =
                    overlap ? SequenceStyle.Warning
                    : behavior.IsUpdating ? SequenceStyle.Accent
                    : active ? SequenceStyle.Success
                    : SequenceStyle.Muted;
                Widgets.DrawBoxSolid(rect, SequenceStyle.Surface);
                Widgets.DrawBoxSolid(new Rect(rect.x + 12f, rect.center.y - 3f, 6f, 6f), tint);
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.MiddleLeft;
                GUI.color = tint;
                Widgets.Label(
                    new Rect(rect.x + 28f, rect.y, rect.width - 40f, rect.height),
                    overlap ? "OVERLAP  /  MULTIPLE CHILDREN RUNNING"
                        : behavior.IsUpdating ? "RUNNING  /  ACTIVE"
                        : active ? "IDLE  /  ACTIVE"
                        : "INACTIVE"
                );
            }
            finally
            {
                GUI.color = color;
                Text.Font = font;
                Text.Anchor = anchor;
            }
        }
    }
}
