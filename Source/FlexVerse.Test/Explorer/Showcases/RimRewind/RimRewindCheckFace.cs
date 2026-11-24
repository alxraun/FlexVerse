// parents: @.frostline/graph/m-explorer.xml
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindCheckFace : Widget
    {
        [Prop]
        public Behavior<float> Mark { get; set; } = 0f;

        [Prop]
        public Behavior<float> Hover { get; set; } = 0f;

        [Prop]
        public Behavior<bool> Checked { get; set; } = false;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<bool> Focused { get; set; } = false;

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var progress = Mathf.Clamp01(Mark.Value);
            var dim = Disabled.Value ? .38f : 1f;
            var track = new Rect(rect.center.x - 26f, rect.center.y - 16f, 52f, 32f);
            if (Focused.Value && !Disabled.Value)
                RimRewindSkin.DrawRounded(track.ExpandedBy(3f), RimRewindPalette.Secondary, 5f);
            var edge = Color.Lerp(RimRewindPalette.Outline, RimRewindPalette.Primary, progress);
            var fill = Color.Lerp(
                RimRewindPalette.SurfaceHighest,
                RimRewindPalette.Primary,
                progress
            );
            RimRewindSkin.DrawRounded(track, RimRewindSkin.Alpha(edge, dim), 5f);
            RimRewindSkin.DrawRounded(track.ContractedBy(2f), RimRewindSkin.Alpha(fill, dim), 1f);
            var radius = 8f + progress * 4f;
            var center = new Vector2(
                Mathf.Lerp(track.x + 16f, track.xMax - 16f, progress),
                track.center.y
            );
            var ink = Color.Lerp(RimRewindPalette.Outline, RimRewindPalette.OnPrimary, progress);
            if (!Disabled.Value && Hover.Value > .001f)
                RimRewindSkin.DrawRounded(
                    new Rect(center.x - 18f, center.y - 18f, 36f, 36f),
                    RimRewindSkin.Alpha(ink, Mathf.Clamp01(Hover.Value) * .08f),
                    18f
                );
            RimRewindSkin.DrawRounded(
                new Rect(center.x - radius, center.y - radius, radius * 2f, radius * 2f),
                RimRewindSkin.Alpha(ink, dim),
                radius
            );
            var markInk = RimRewindSkin.Alpha(RimRewindPalette.Primary, dim * progress);
            if (progress > .001f)
            {
                var a = center + new Vector2(-5f, 0f);
                var b = center + new Vector2(-1f, 4f);
                var c = center + new Vector2(6f, -4f);

                var t1 = Mathf.Clamp01(progress / .35f);
                DrawStroke(a, Vector2.Lerp(a, b, t1), markInk);
                if (progress > .35f)
                    DrawStroke(b, Vector2.Lerp(b, c, (progress - .35f) / .65f), markInk);
            }
        }

        /// <summary>
        /// Continuous antialiased stroke: overlapping disc caps merge the two
        /// segments at the elbow and round both ends of the check.
        /// </summary>
        private static void DrawStroke(Vector2 from, Vector2 to, Color color)
        {
            var steps = Mathf.Max(1, Mathf.CeilToInt((to - from).magnitude * 2f));
            for (var i = 0; i <= steps; i++)
            {
                var p = Vector2.Lerp(from, to, (float)i / steps);
                RimRewindSkin.DrawRounded(new Rect(p.x - 1f, p.y - 1f, 2f, 2f), color, 1f);
            }
        }
    }
}
