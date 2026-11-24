// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindBrand : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        private RimRewind _screen;

        public RimRewindBrand()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            FlexGrow = 1f;
            FlexBasis = 0f.Point();
            FlexShrink = 1f;
            MinWidth = 0f.Point();
        }

        public override void OnMount()
        {
            _screen = Session.Value;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    Width = 56f.Point(),
                    Height = 56f.Point(),
                    MarginRight = 16f.Point(),
                    BorderAll = 0f.Point(),
                    BorderColor = RimRewindSkin.Accent,
                    BorderRadius = RimRewindSkin.CornerRadius,
                    BackgroundColor = RimRewindPalette.PrimaryContainer,
                    Children = new WidgetList
                    {
                        new RimRewindSigil { Pulse = _screen.BrandPulse, Height = 56f.Point() },
                    },
                },
                BuildWordmark(),
            ];

        private static Widget BuildWordmark() =>
            new View
            {
                FlexGrow = 1f,
                MinWidth = 0f.Point(),
                Children = new WidgetList
                {
                    new Label
                    {
                        Content = "<size=30><b>RimRewind</b></size>",
                        Color = RimRewindSkin.Text,
                        TextAlign = TextAnchor.MiddleLeft,
                        Height = 40f.Point(),
                        TextWrap = false,
                    },
                    RimRewindSkin.BuildCaption("Time can be rewritten. You set the rules."),
                },
            };
    }
}
