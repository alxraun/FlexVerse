// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSliderTrack : Widget
    {
        [Prop]
        public Behavior<float> Fill { get; set; } = 0f;

        [Prop]
        public Behavior<float> Hover { get; set; } = 0f;

        [Prop]
        public Behavior<float> Pressure { get; set; } = 0f;

        [Prop]
        public Behavior<bool> Focused { get; set; } = false;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var track = new Rect(
                rect.x + 9f,
                rect.center.y - 4f,
                Mathf.Max(1f, rect.width - 18f),
                8f
            );
            var dim = Disabled.Value ? .38f : 1f;
            var fraction = Mathf.Clamp01(Fill.Value);
            var x = track.x + fraction * track.width;
            RimRewindSkin.DrawRounded(
                track,
                RimRewindSkin.Alpha(RimRewindPalette.SecondaryContainer, dim),
                RimRewindSkin.CornerRadius
            );
            RimRewindSkin.DrawRounded(
                new Rect(track.x, track.y, Mathf.Max(0f, x - track.x), track.height),
                RimRewindSkin.Alpha(RimRewindPalette.Primary, dim),
                RimRewindSkin.CornerRadius
            );

            var cap = Mathf.Min(
                RimRewindSkin.CornerRadius,
                Mathf.Max(0f, x - track.x - RimRewindSkin.CornerRadius)
            );
            if (cap > 0f)
                Widgets.DrawBoxSolid(
                    new Rect(x - cap, track.y, cap, track.height),
                    RimRewindSkin.Alpha(RimRewindPalette.Primary, dim)
                );
            var height = 24f + Mathf.Clamp01(Hover.Value) * 4f - Mathf.Clamp01(Pressure.Value) * 4f;
            if (Focused.Value && !Disabled.Value)
                RimRewindSkin.DrawRounded(
                    new Rect(x - 6f, rect.center.y - height * .5f - 4f, 12f, height + 8f),
                    RimRewindSkin.Alpha(RimRewindPalette.Primary, .16f),
                    6f
                );
            RimRewindSkin.DrawRounded(
                new Rect(x - 2f, rect.center.y - height * .5f, 4f, height),
                RimRewindSkin.Alpha(RimRewindPalette.Primary, dim),
                2f
            );
            for (var i = 1; i < 4; i++)
            {
                var tick = i / 4f;
                if (Mathf.Abs(tick - fraction) < .025f)
                    continue;
                RimRewindSkin.DrawRounded(
                    new Rect(track.x + track.width * tick - 1f, track.center.y - 1f, 2f, 2f),
                    RimRewindSkin.Alpha(
                        tick < fraction
                            ? RimRewindPalette.OnPrimary
                            : RimRewindPalette.OnSecondaryContainer,
                        dim * .5f
                    ),
                    1f
                );
            }
        }
    }
}
