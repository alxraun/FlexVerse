// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingControl : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                Option.Value.Control switch
                {
                    RimRewindControl.Choice => new RimRewindChoice
                    {
                        Value = _screen.Draft.Select(profile => (int)profile[Option.Value.Id]),
                        Options = Option.Value.Choices,
                        Disabled = _screen.Draft.Select(profile =>
                            Option.Value.Unavailable?.Invoke(profile) != null
                        ),
                        OnValueChange = new Action<int>(HandleChoice),
                    },
                    RimRewindControl.Toggle => new RimRewindToggleSetting { Option = Option },
                    _ => new RimRewindNumberControl { Option = Option },
                },
            ];

        private void HandleChoice(int value) => _screen.HandleOptionEdit(Option.Value, value);
    }
}
