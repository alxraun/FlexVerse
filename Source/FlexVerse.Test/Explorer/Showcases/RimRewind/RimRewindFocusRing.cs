// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindFocusRing : Widget
    {
        [Prop]
        public Behavior<bool> Visible { get; set; } = false;

        [Prop]
        public Behavior<float> Radius { get; set; } = RimRewindSkin.CornerRadius;

        public RimRewindFocusRing()
        {
            PositionType = PositionTypes.Absolute;
            PositionLeft = 0f.Point();
            PositionRight = 0f.Point();
            PositionTop = 0f.Point();
            PositionBottom = 0f.Point();
            ZIndex = 1;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            Visible.Value
                ?
                [
                    new View
                    {
                        Width = 100f.Percent(),
                        Height = 100f.Percent(),
                        BorderAll = 3f.Point(),
                        BorderRadius = Radius,
                        BorderColor = RimRewindPalette.Secondary,
                    },
                ]
                : [];
    }
}
