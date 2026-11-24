// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindArchiveSummary : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindArchiveSummary()
        {
            BorderTop = 1f.Point();
            BorderColor = RimRewindSkin.Line;
            PaddingTop = 10f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                RimRewindSkin.BuildCaption(
                    _screen.Workspace.Select(workspace =>
                        workspace.Scope == 0 ? "Rules for this colony" : "For new colonies"
                    )
                ),
                RimRewindSkin.BuildCaption(
                    _screen.Draft.Select(profile =>
                        $"{profile[RimRewindSetting.HistoryDepth]:0} s of past · {profile[RimRewindSetting.MemoryBudget]:0} MB"
                    ),
                    RimRewindSkin.Text
                ),
                RimRewindSkin.BuildCaption($"{RimRewindOptions.All.Length} settings · 8 sections"),
            ];
    }
}
