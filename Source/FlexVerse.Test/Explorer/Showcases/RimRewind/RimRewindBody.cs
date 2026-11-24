// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindBody : View
    {
        public RimRewindBody()
        {
            FlexDirection = FlexDirections.Row;
            FlexGrow = 1f;
            FlexShrink = 1f;
            MinHeight = 0f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [new RimRewindSidebar(), new RimRewindSettingsPage()];
    }
}
