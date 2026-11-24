// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindHeader : View
    {
        public RimRewindHeader()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            Height = 64f.Point();
            MarginBottom = 12f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [new RimRewindBrand(), new RimRewindScopeSelector()];
    }
}
