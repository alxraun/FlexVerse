// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class TetrisCanvas : View
    {
        public TetrisCanvas() => Overflow = Overflows.Hidden;

        public override void OnGUI(Rect rect)
        {
            Widgets.DrawBoxSolid(rect, TetrisStyle.Surface);
            for (var x = 0; x <= TetrisWell.Columns; x++)
                Widgets.DrawBoxSolid(
                    new Rect(rect.x + rect.width * x / TetrisWell.Columns, rect.y, 1f, rect.height),
                    TetrisStyle.Grid
                );
            for (var y = 0; y <= TetrisWell.Rows; y++)
                Widgets.DrawBoxSolid(
                    new Rect(rect.x, rect.y + rect.height * y / TetrisWell.Rows, rect.width, 1f),
                    TetrisStyle.Grid
                );
        }
    }
}
