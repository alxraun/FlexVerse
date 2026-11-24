// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindMenuOption : Widget
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<string> Label { get; set; } = "";

        [Prop]
        public Behavior<int> Index { get; set; }

        private RimRewind _screen;

        public RimRewindMenuOption()
        {
            Height = 40f.Point();
            FlexShrink = 0f;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButton
                {
                    Label = Label,
                    Quiet = true,
                    Radius = RimRewindSkin.CornerRadius,
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                    TextAlign = TextAnchor.MiddleLeft,
                    Flat = true,
                    OnActivate = new Action(HandleSelection),
                },
            ];

        public override void OnMouseEnter(FlexEvent e) => _screen.HandleMenuFocus(Index.Value);

        private void HandleSelection() => _screen.HandleMenuChoose(Index.Value);
    }
}
