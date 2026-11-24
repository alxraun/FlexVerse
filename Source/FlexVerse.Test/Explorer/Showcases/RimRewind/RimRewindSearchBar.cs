// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSearchBar : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindSearchBar()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            Height = 48f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindSearchField(),
                new RimRewindButton
                {
                    Label = "Modified",
                    Compact = true,
                    Width = 112f.Point(),
                    MarginLeft = 8f.Point(),
                    Radius = RimRewindSkin.CornerRadius,
                    Height = 40f.Point(),
                    Selected = _screen.ChangedOnly,
                    Tooltip = "Show unapplied changes",
                    OnActivate = new Action(() =>
                        _screen.ChangedOnly.Target = !_screen.ChangedOnly.Value
                    ),
                },
                new RimRewindButton
                {
                    Label = "Advanced",
                    Compact = true,
                    Width = 122f.Point(),
                    MarginLeft = 8f.Point(),
                    Radius = RimRewindSkin.CornerRadius,
                    Height = 40f.Point(),
                    Selected = _screen.Advanced,
                    OnActivate = new Action(() =>
                        _screen.Advanced.Target = !_screen.Advanced.Value
                    ),
                },
            ];
    }
}
