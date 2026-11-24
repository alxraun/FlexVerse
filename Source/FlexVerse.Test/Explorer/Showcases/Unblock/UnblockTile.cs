// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class UnblockTile : Widget
    {
        [Prop]
        public Behavior<UnblockBlock> Block { get; set; }

        [Prop]
        public Behavior<int> Beat { get; set; }

        [Prop]
        public Behavior<Texture2D> Glow { get; set; }

        [Prop]
        public Behavior<float> Pulse { get; set; } = 1f;

        [State]
        public Behavior<Vector2> Motion { get; set; }

        [State]
        public Behavior<Vector2> FaceScale { get; set; }

        [State]
        public Behavior<Color> BorderTint { get; set; }

        [State]
        public Behavior Retarget { get; set; }

        public override void OnMount()
        {
            Motion = Spring.State(
                Block.Value.At(Beat.Value),
                tension: 150f,
                friction: 18f,
                mass: 0.8f
            );
            FaceScale = Spring.State(Vector2.one, tension: 150f, friction: 18f, mass: 0.8f);
            BorderTint = Spring.State(
                new Color(1f, 1f, 1f, 0.25f),
                tension: 150f,
                friction: 18f,
                mass: 0.8f
            );
            Retarget = Effect.Create(UpdateTarget, Beat);
        }

        private void UpdateTarget()
        {
            var block = Block.Value;
            Motion.Target = block.At(Beat.Value);
            BorderTint.Target = HasMoved() ? UnblockVisuals.Cyan : new Color(1f, 1f, 1f, 0.25f);
            if (block.IsHero)
                FaceScale.Target =
                    Beat.Value == 11 ? new Vector2(1.04f, 1.04f)
                    : HasMoved() ? new Vector2(1.02f, 1.02f)
                    : Vector2.one;
        }

        private bool HasMoved() =>
            Block.Value.At(Beat.Value)
            != Block.Value.At(
                (Beat.Value + UnblockChoreography.Beats - 1) % UnblockChoreography.Beats
            );

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new UnblockTileFace
                {
                    Block = Block,
                    Shade = Block.Value.Color,
                    Glow = Glow,
                    Pulse = Pulse,
                    Moving = HasMoved(),
                    BorderTint = BorderTint,
                    Escaped = Beat.Value == 11,
                    Scale = FaceScale,
                    PositionType = PositionTypes.Absolute,
                    PositionLeft = (Motion.Value.x * 100f / UnblockChoreography.Columns).Percent(),
                    PositionTop = (Motion.Value.y * 100f / UnblockChoreography.Rows).Percent(),
                    Width = (Block.Value.Width * 100f / UnblockChoreography.Columns).Percent(),
                    Height = (Block.Value.Height * 100f / UnblockChoreography.Rows).Percent(),
                },
            ];
    }
}
