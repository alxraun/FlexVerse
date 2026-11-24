// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class TetrisMino : Widget
    {
        [Prop]
        public Behavior<TetrisMinoMotion> Motion { get; set; }

        [Prop]
        public Behavior<Texture2D> Glow { get; set; }

        public override void OnGUI(Rect rect)
        {
            var motion = Motion.Value;
            var alpha = motion.Opacity.Value;
            if (alpha <= 0.005f)
                return;
            rect = rect.ContractedBy(1.5f);
            if (rect.width <= 0f || rect.height <= 0f)
                return;
            var color = Color.Lerp(
                TetrisStyle.Ink(motion.Kind),
                TetrisStyle.Red,
                motion.Emphasis.Value
            );
            color = Color.Lerp(color, Color.white, motion.Flash.Value);
            color.a = alpha;
            if (motion.IsGhost)
            {
                DrawOutline(rect, color);
                return;
            }
            if (motion.Emphasis.Value > 0.5f || motion.Flash.Value > 0.5f)
                TetrisStyle.Glow(
                    rect.ExpandedBy(10f),
                    Glow.Value,
                    new Color(color.r, color.g, color.b, 0.4f * alpha)
                );
            Widgets.DrawBoxSolid(rect, color);
            Widgets.DrawBoxSolid(
                new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 1f),
                new Color(1f, 1f, 1f, 0.65f * alpha)
            );
            if (rect.width >= 12f)
                Widgets.DrawBoxSolid(
                    new Rect(rect.center.x - 2f, rect.center.y - 2f, 4f, 4f),
                    new Color(0.2f, 0.3f, 0.4f, 0.25f * alpha)
                );
        }

        private static void DrawOutline(Rect rect, Color color)
        {
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 1f), color);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 1f, rect.width, 1f), color);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 1f, rect.height), color);
            Widgets.DrawBoxSolid(new Rect(rect.xMax - 1f, rect.y, 1f, rect.height), color);
        }
    }
}
