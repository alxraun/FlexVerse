// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindWindow : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public override void OnMount() => _screen = Session.Value;

        public RimRewindWindow()
        {
            Width = 100f.Percent();
            Height = 100f.Percent();
            PaddingAll = 24f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindHeader
                {
                    Height = _screen.Compact.Select(compact => (compact ? 56f : 64f).Point()),
                    MarginBottom = _screen.Compact.Select(compact => (compact ? 8f : 16f).Point()),
                },
                new RimRewindBody(),
                new RimRewindFooter(),
            ];
    }
}
