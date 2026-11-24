// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoGrid : View
    {
        [Prop]
        public Behavior<BentoPanelMotion> Motion { get; set; }

        public BentoGrid()
        {
            FlexDirection = FlexDirections.Row;
            FlexGrow = 1f;
            MinHeight = 0f.Point();
            MinWidth = 0f.Point();
            PaddingTop = 4f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            Enumerable.Range(0, 3).Select(BuildColumn).ToArray();

        private Widget BuildColumn(int column) =>
            new View
            {
                Key = column,
                FlexDirection = FlexDirections.Column,
                FlexBasis = 0f.Point(),
                MinWidth = 0f.Point(),
                MinHeight = 0f.Point(),
                FlexGrow = Motion.Value.Bias.Select(bias => 0.6f + (column + 1) * bias),
                Children = new WidgetList(
                    Enumerable.Range(0, 3).Select(row => BuildCell(column, row))
                ),
            };

        private Widget BuildCell(int column, int row) =>
            new BentoRect
            {
                Key = row,
                MarginAll = 2f.Point(),
                FlexBasis = 0f.Point(),
                Ink = (column + row) % 3 == 0 ? Motion.Value.Ink : Motion.Value.MutedInk,
                FlexGrow = Group.Combine(
                    Motion.Value.Fill,
                    Motion.Value.Bias,
                    (fill, bias) =>
                        0.45f
                        + (row % 2 == 0 ? fill : 1f - fill) * 1.5f
                        + bias * (column + 1) * 0.2f
                ),
            };
    }
}
