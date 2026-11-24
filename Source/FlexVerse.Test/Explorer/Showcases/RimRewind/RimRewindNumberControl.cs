// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindNumberControl : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindNumberReadout { Option = Option },
                new RimRewindSlider
                {
                    Value = _screen.Draft.Select(profile => profile[Option.Value.Id]),
                    MinValue = _screen.Draft.Select(profile => Option.Value.Bounds(profile).Min),
                    MaxValue = _screen.Draft.Select(profile => Option.Value.Bounds(profile).Max),
                    Height = 40f.Point(),
                    Step = Option.Value.Step,
                    Disabled = _screen.Draft.Select(profile =>
                        Option.Value.Unavailable?.Invoke(profile) != null
                    ),
                    Tooltip = _screen.Draft.Select(BuildRangeHint),
                    OnValueChange = new Action<float>(HandleValue),
                },
            ];

        private string BuildRangeHint(RimRewindProfile profile)
        {
            var bounds = Option.Value.Bounds(profile);
            return Option.Value.Format(bounds.Min) + " — " + Option.Value.Format(bounds.Max);
        }

        private void HandleValue(float value) => _screen.HandleOptionEdit(Option.Value, value);
    }
}
