// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindToggleSetting : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public RimRewindToggleSetting()
        {
            FlexDirection = FlexDirections.Row;
            JustifyContent = Justify.FlexEnd;
            AlignItems = Align.Center;
            Height = 40f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = _screen.Draft.Select(profile =>
                        Option.Value.Format(profile[Option.Value.Id])
                    ),
                    Font = GameFont.Tiny,
                    TextAlign = TextAnchor.MiddleRight,
                    MarginRight = 16f.Point(),
                    Color = _screen.Draft.Select(BuildColor),
                },
                new RimRewindCheck
                {
                    Checked = _screen.Draft.Select(profile => profile.Enabled(Option.Value.Id)),
                    Disabled = _screen.Draft.Select(profile =>
                        Option.Value.Unavailable?.Invoke(profile) != null
                    ),
                    OnValueChange = new Action<bool>(HandleToggle),
                },
            ];

        private Color BuildColor(RimRewindProfile profile) =>
            Option.Value.Unavailable?.Invoke(profile) != null
                ? RimRewindSkin.Alpha(RimRewindSkin.Muted, .5f)
            : profile.Enabled(Option.Value.Id) ? RimRewindSkin.Accent
            : RimRewindSkin.Muted;

        private void HandleToggle(bool value) =>
            _screen.HandleOptionEdit(Option.Value, value ? 1f : 0f);
    }
}
