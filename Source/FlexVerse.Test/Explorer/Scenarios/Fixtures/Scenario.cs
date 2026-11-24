// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class Scenario : Widget
    {
        public static string Description => "No description provided.";

        public Scenario()
        {
            FlexDirection = FlexDirections.Column;
            FlexGrow = 1;
            AlignItems = Align.Center;
            JustifyContent = Justify.Center;
        }
    }
}
