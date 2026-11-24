// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingsPage : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindSettingsPage()
        {
            FlexGrow = 1f;
            FlexBasis = 0f.Point();
            MinWidth = 0f.Point();
            MinHeight = 0f.Point();
            FlexShrink = 1f;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindSearchBar { MarginBottom = 12f.Point() },
                new RimRewindDocumentScrollView
                {
                    OnUserScroll = new Action(_screen.CancelNavigation),
                },
            ];
    }
}
