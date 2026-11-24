// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Side rail of a sequence scenario: the fixed-width right column with
    /// status, scenario controls, the explanatory note and the event log.
    /// </summary>
    public partial class SequenceRail : View
    {
        public SequenceRail()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            Width = 300.Point();
        }
    }
}
