// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSearchField : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindSearchField()
        {
            FlexGrow = 1f;
            FlexBasis = 0f.Point();
            MinWidth = 0f.Point();
            Height = 48f.Point();
            BackgroundColor = RimRewindPalette.SurfaceHigh;
            BorderAll = 0f.Point();
            BorderColor = RimRewindSkin.Line;
            BorderRadius = RimRewindSkin.CornerRadius;
            PaddingHorizontal = 16f.Point();
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindTextInput
                {
                    Value = _screen.Query,
                    Placeholder = "Search settings across all sections…",
                    FlexGrow = 1f,
                    FlexShrink = 1f,
                    MinWidth = 0f.Point(),
                    Height = 40f.Point(),
                    OnValueChange = new Action<string>(_screen.HandleSearch),
                },
                new RimRewindButton
                {
                    Label = "×",
                    Compact = true,
                    Quiet = true,
                    Width = 40f.Point(),
                    Height = 40f.Point(),
                    Disabled = _screen.Query.Select(string.IsNullOrEmpty),
                    Tooltip = "Clear search",
                    OnActivate = new Action(HandleClear),
                },
            ];

        private void HandleClear() => _screen.HandleSearch("");
    }
}
