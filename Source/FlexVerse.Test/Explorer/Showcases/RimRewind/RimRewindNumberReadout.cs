// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindNumberReadout : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public RimRewindNumberReadout()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            Height = 40f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildStepButton(false),
                new Label
                {
                    Content = _screen.Draft.Select(profile =>
                        Option.Value.Format(profile[Option.Value.Id])
                    ),
                    FlexGrow = 1f,
                    Height = 40f.Point(),
                    TextAlign = TextAnchor.MiddleCenter,
                    Color = _screen.Draft.Select(profile =>
                        Option.Value.Unavailable?.Invoke(profile) != null
                            ? RimRewindSkin.Muted
                            : RimRewindSkin.Accent
                    ),
                },
                BuildStepButton(true),
            ];

        private Widget BuildStepButton(bool increase) =>
            new RimRewindButton
            {
                Label = increase ? "+" : "−",
                Compact = true,
                Quiet = true,
                Width = 40f.Point(),
                Height = 40f.Point(),
                Disabled = _screen.Draft.Select(profile => IsAtLimit(profile, increase)),
                Tooltip = increase ? "Increase value" : "Decrease value",
                OnActivate = increase ? new Action(HandleIncrease) : new Action(HandleDecrease),
            };

        private bool IsAtLimit(RimRewindProfile profile, bool increase)
        {
            if (Option.Value.Unavailable?.Invoke(profile) != null)
                return true;
            var bounds = Option.Value.Bounds(profile);
            return increase
                ? profile[Option.Value.Id] >= bounds.Max
                : profile[Option.Value.Id] <= bounds.Min;
        }

        private void HandleIncrease() =>
            _screen.HandleOptionEdit(
                Option.Value,
                _screen.Draft.Value[Option.Value.Id] + Option.Value.Step
            );

        private void HandleDecrease() =>
            _screen.HandleOptionEdit(
                Option.Value,
                _screen.Draft.Value[Option.Value.Id] - Option.Value.Step
            );
    }
}
