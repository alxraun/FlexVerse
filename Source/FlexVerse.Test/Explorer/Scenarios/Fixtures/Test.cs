// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class Test : Widget
    {
        public static string Description => "No description provided.";

        public Test()
        {
            FlexGrow = 1;
            AlignItems = Align.Center;
            JustifyContent = Justify.Center;
        }
    }
}
