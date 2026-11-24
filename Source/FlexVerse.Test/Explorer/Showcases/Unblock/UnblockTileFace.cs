// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class UnblockTileFace : Widget
    {
        [Prop]
        public Behavior<UnblockBlock> Block { get; set; }

        [Prop]
        public Behavior<Color> Shade { get; set; }

        [Prop]
        public Behavior<Texture2D> Glow { get; set; }

        [Prop]
        public Behavior<Color> BorderTint { get; set; }

        [Prop]
        public Behavior<bool> Moving { get; set; }

        [Prop]
        public Behavior<bool> Escaped { get; set; }

        [Prop]
        public Behavior<float> Pulse { get; set; } = 1f;

        public override void OnGUI(Rect rect)
        {
            rect.width = Mathf.Max(0f, rect.width - 3f);
            rect.height = Mathf.Max(0f, rect.height - 3f);
            var hero = Block.Value.IsHero;
            var border = hero ? UnblockVisuals.RedBorder : BorderTint.Value;
            if (Glow.Value != null && (hero || Moving.Value))
            {
                var tint = hero ? UnblockVisuals.Red : UnblockVisuals.Cyan;
                tint.a = hero
                    ? Escaped.Value
                        ? 1f
                        : 0.65f
                    : 0.4f;
                UnblockVisuals.DrawGlow(rect.ExpandedBy(hero ? 28f : 14f), Glow.Value, tint);
            }
            Widgets.DrawBoxSolid(rect, border);
            Widgets.DrawBoxSolid(rect.ContractedBy(1f), hero ? UnblockVisuals.Red : Shade.Value);
            Widgets.DrawBoxSolid(
                new Rect(rect.x + 1f, rect.y + 1f, rect.width - 2f, 1f),
                new Color(1f, 1f, 1f, Moving.Value ? 0.85f : 0.7f)
            );
            if (hero)
                DrawHeroGrip(rect);
            else
                DrawGrip(rect);
        }

        private void DrawHeroGrip(Rect rect)
        {
            var notch = new Color(1f, 1f, 1f, 0.45f);
            Widgets.DrawBoxSolid(
                new Rect(rect.x + 8f, rect.center.y - rect.height * 0.25f, 4f, rect.height * 0.5f),
                notch
            );
            Widgets.DrawBoxSolid(
                new Rect(
                    rect.xMax - 12f,
                    rect.center.y - rect.height * 0.25f,
                    4f,
                    rect.height * 0.5f
                ),
                notch
            );
            Widgets.DrawBoxSolid(
                new Rect(rect.center.x - 5f, rect.center.y - 5f, 10f, 10f),
                Color.white
            );
            Widgets.DrawBoxSolid(
                new Rect(rect.center.x - 2f, rect.center.y - 2f, 4f, 4f),
                new Color(0.86f, 0.15f, 0.15f, Pulse.Value)
            );
        }

        private void DrawGrip(Rect rect)
        {
            var block = Block.Value;
            var count = Mathf.Max(block.Width, block.Height) >= 3 ? 3 : 2;
            for (var i = 0; i < count; i++)
            {
                var offset = (i - (count - 1) * 0.5f) * 12f;
                var x = rect.center.x - 3f + (block.IsHorizontal ? offset : 0f);
                var y = rect.center.y - 3f + (block.IsHorizontal ? 0f : offset);
                Widgets.DrawBoxSolid(new Rect(x, y, 6f, 6f), UnblockVisuals.Grip);
            }
        }
    }
}
