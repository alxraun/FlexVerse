// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindMenuHighlight : Widget
    {
        [Prop]
        public Behavior<float> Position { get; set; } = 0f;

        public RimRewindMenuHighlight()
        {
            PositionType = PositionTypes.Absolute;
            PositionTop = 0f.Point();
            PositionLeft = 8f.Point();
            PositionRight = 8f.Point();
            Height = 40f.Point();
            ZIndex = -1;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    PositionType = PositionTypes.Absolute,
                    PositionLeft = 0f.Point(),
                    PositionRight = 0f.Point(),
                    PositionTop = Position.Select(value => (8f + value).Point()),
                    Height = 40f.Point(),
                    BackgroundColor = RimRewindSkin.GoldDark,
                    BorderRadius = RimRewindSkin.CornerRadius,
                    BorderLeft = 0f.Point(),
                    BorderColor = RimRewindSkin.Accent,
                },
            ];
    }
}
