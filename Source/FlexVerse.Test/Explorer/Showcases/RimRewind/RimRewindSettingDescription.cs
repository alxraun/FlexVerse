// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingDescription : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildTitle(),
                new Label
                {
                    Content = Option.Value.Description,
                    Font = GameFont.Small,
                    TextAlign = TextAnchor.UpperLeft,
                    Color = RimRewindSkin.Muted,
                    TextWrap = true,
                    MarginTop = 4f.Point(),
                },
                new Label
                {
                    Content = _screen.Draft.Select(BuildAvailabilityHint),
                    Font = GameFont.Tiny,
                    TextAlign = TextAnchor.UpperLeft,
                    Color = RimRewindSkin.Amber,
                    TextWrap = true,
                    MarginTop = _screen.Draft.Select(profile =>
                        string.IsNullOrEmpty(BuildAvailabilityHint(profile))
                            ? 0f.Point()
                            : 4f.Point()
                    ),
                },
            ];

        private Widget BuildTitle() =>
            new View
            {
                FlexDirection = FlexDirections.Row,
                AlignItems = Align.Center,
                Children = new WidgetList
                {
                    new Label
                    {
                        Content = "<b>" + Option.Value.Title + "</b>",
                        Color = _screen.Draft.Select(profile =>
                            Option.Value.Unavailable?.Invoke(profile) != null
                                ? RimRewindSkin.Muted
                                : RimRewindSkin.Text
                        ),
                        TextAlign = TextAnchor.MiddleLeft,
                        TextWrap = true,
                        MinHeight = 24f.Point(),
                        FlexShrink = 1f,
                        MinWidth = 0f.Point(),
                    },
                    new Label
                    {
                        Content = Group.Combine(
                            _screen.Draft,
                            _screen.Applied,
                            (draft, applied) => draft.Differs(applied, Option.Value.Id) ? " •" : ""
                        ),
                        Color = RimRewindSkin.Amber,
                        Width = 14f.Point(),
                        Tooltip = "Has an unapplied change",
                    },
                },
            };

        private string BuildAvailabilityHint(RimRewindProfile profile) =>
            Option.Value.Unavailable?.Invoke(profile)
            ?? (
                Option.Value.Id == RimRewindSetting.EffectIntensity
                && profile.Enabled(RimRewindSetting.ReducedMotion)
                    ? $"With Minimum Motion: {profile.EffectiveEffect:P0}"
                    : ""
            );
    }
}
