// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindEmptyState : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindEmptyState()
        {
            PaddingAll = 24f.Point();
            BackgroundColor = RimRewindSkin.Surface;
            BorderRadius = RimRewindSkin.CornerRadius;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                RimRewindSkin.BuildHeading(
                    _screen.ChangedOnly.Select(changed =>
                        changed ? "No modified settings" : "Nothing found"
                    )
                ),
                new Label
                {
                    Content =
                        "Change the query or reset the filters to see the remaining settings.",
                    Color = RimRewindSkin.Muted,
                    TextAlign = TextAnchor.MiddleLeft,
                    MarginBottom = 16f.Point(),
                },
                new RimRewindButton
                {
                    Label = "Reset search and filters",
                    Width = 240f.Point(),
                    Compact = true,
                    OnActivate = new Action(_screen.HandleClearFilters),
                },
            ];
    }
}
