// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class UnblockGate : Widget
    {
        [Prop]
        public Behavior<int> Beat { get; set; }

        [Prop]
        public Behavior<Texture2D> Glow { get; set; }

        [Prop]
        public Behavior<float> Pulse { get; set; } = 1f;

        [State]
        public Behavior<float> Brightness { get; set; } = 0f;

        [State]
        public Behavior Lighting { get; set; }

        public UnblockGate()
        {
            PositionType = PositionTypes.Absolute;
            PositionRight = 0f.Point();
            PositionTop = (100f * UnblockChoreography.ExitRow / UnblockChoreography.Rows).Percent();
            Width = 12f.Point();
            Height = (100f / UnblockChoreography.Rows).Percent();
        }

        public override void OnMount()
        {
            Brightness = Spring.State(0f, tension: 150f, friction: 18f, mass: 0.8f);
            Lighting = Effect.Create(
                () =>
                    Brightness.Target =
                        Beat.Value == 11 ? 0.95f
                        : Beat.Value > 6 ? 0.5f
                        : 0f,
                Beat
            );
        }

        public override void OnGUI(Rect rect)
        {
            if (Glow.Value != null)
                UnblockVisuals.DrawGlow(
                    rect.ExpandedBy(35f),
                    Glow.Value,
                    new Color(0.937f, 0.267f, 0.267f, Brightness.Value)
                );
            Widgets.DrawBoxSolid(rect, new Color(0.937f, 0.267f, 0.267f, 0.2f));
            var border = new Color(0.937f, 0.267f, 0.267f, 0.9f);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, 2f, rect.height), border);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.y, rect.width, 2f), border);
            Widgets.DrawBoxSolid(new Rect(rect.x, rect.yMax - 2f, rect.width, 2f), border);
            Widgets.DrawBoxSolid(
                new Rect(rect.center.x - 2f, rect.y + rect.height * 0.2f, 4f, rect.height * 0.6f),
                new Color(0.973f, 0.443f, 0.443f, Pulse.Value)
            );
        }
    }
}
