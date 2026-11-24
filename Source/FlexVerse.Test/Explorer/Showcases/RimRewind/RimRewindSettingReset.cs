// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingReset : Widget
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }
        private RimRewind _screen;

        public RimRewindSettingReset()
        {
            Width = 76f.Point();
            Height = 40f.Point();
            MarginLeft = 8f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButton
                {
                    Label = "Reset",
                    Compact = true,
                    Quiet = true,
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                    Disabled = _screen.Draft.Select(profile =>
                        Math.Abs(profile[Option.Value.Id] - Option.Value.Default) < .0001f
                    ),
                    Tooltip = "Restore default value: " + Option.Value.Format(Option.Value.Default),
                    OnActivate = new Action(HandleReset),
                },
            ];

        private void HandleReset() => _screen.HandleEdit(Option.Value.Id, Option.Value.Default);
    }
}
