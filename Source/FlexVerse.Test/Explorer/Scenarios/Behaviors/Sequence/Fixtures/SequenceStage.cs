// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Interactive stage of a sequence scenario: the left column holding the
    /// canvas and the child lifecycle indicators. Grows to
    /// fill the window next to the fixed-width SequenceRail.
    /// </summary>
    public partial class SequenceStage : View
    {
        public SequenceStage()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            FlexShrink = 1;
            MinWidth = 0f.Point();
            FlexGrow = 1;
            MarginRight = 16f.Point();
        }
    }
}
