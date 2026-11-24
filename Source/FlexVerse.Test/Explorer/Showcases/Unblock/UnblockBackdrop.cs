// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class UnblockBackdrop : Widget
    {
        [Prop]
        public Behavior<Texture2D> Glow { get; set; }

        public override void OnGUI(Rect rect)
        {
            if (Glow.Value == null)
                return;
            var halo = new Rect(rect.center.x - 425f, rect.center.y - 275f, 850f, 550f);
            UnblockVisuals.DrawGlow(halo, Glow.Value, new Color(0.86f, 0.15f, 0.15f, 0.05f));
        }
    }

    public partial class UnblockCanvas : View
    {
        public UnblockCanvas() => Overflow = Overflows.Hidden;

        public override void OnGUI(Rect rect)
        {
            for (var x = 0; x < UnblockChoreography.Columns; x++)
                Widgets.DrawBoxSolid(
                    new Rect(
                        rect.x + rect.width * x / UnblockChoreography.Columns,
                        rect.y,
                        1f,
                        rect.height
                    ),
                    UnblockVisuals.Grid
                );
            for (var y = 0; y < UnblockChoreography.Rows; y++)
                Widgets.DrawBoxSolid(
                    new Rect(
                        rect.x,
                        rect.y + rect.height * y / UnblockChoreography.Rows,
                        rect.width,
                        1f
                    ),
                    UnblockVisuals.Grid
                );
        }
    }
}
