// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingsContent : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<RimRewindOption[]> Options { get; set; }

        public override void OnMount() => Options = Session.Value.VisibleOptions;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                .. Options
                    .Value.GroupBy(option => option.Category)
                    .OrderBy(group => group.Key)
                    .Select(group => new RimRewindCategorySection
                    {
                        Key = group.Key,
                        Index = group.Key,
                        Options = group.ToArray(),
                    }),
                Options.Value.Length == 0 ? new RimRewindEmptyState() : null,
            ];
    }
}
