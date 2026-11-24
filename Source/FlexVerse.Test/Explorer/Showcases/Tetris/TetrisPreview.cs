// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class TetrisPreview : Widget
    {
        [Prop]
        public Behavior<TetrisKind> Kind { get; set; }

        public override void OnGUI(Rect rect)
        {
            var unit = Mathf.Min(rect.width / 5f, rect.height / 4f);
            var cells = TetrisShapes.Cells(Kind.Value, 0);
            var x =
                rect.center.x
                - unit
                    * (
                        Kind.Value == TetrisKind.I ? 2f
                        : Kind.Value == TetrisKind.O ? 1f
                        : 1.5f
                    );
            var y = rect.center.y - unit;
            for (var i = 0; i < cells.Count; i++)
                Widgets.DrawBoxSolid(
                    new Rect(x + cells[i].X * unit, y + cells[i].Y * unit, unit - 3f, unit - 3f),
                    TetrisStyle.Ink(Kind.Value)
                );
        }
    }
}
