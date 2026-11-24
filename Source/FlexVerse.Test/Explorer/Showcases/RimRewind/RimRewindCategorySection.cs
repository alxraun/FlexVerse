// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindCategorySection : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<int> Index { get; set; } = 0;

        [Prop]
        public Behavior<RimRewindOption[]> Options { get; set; } = Array.Empty<RimRewindOption>();

        [State]
        private Behavior<string> Query { get; set; }

        [State]
        private Behavior<bool> ChangedOnly { get; set; }

        public RimRewindCategorySection()
        {
            MarginBottom = 32f.Point();
        }

        public override void OnMount()
        {
            Query = Session.Value.Query;
            ChangedOnly = Session.Value.ChangedOnly;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindCategoryHeading
                {
                    Index = Index,
                    Count = Options.Select(options => options.Length),
                },
                BuildPreview(),
                .. Options
                    .Value.Select((option, index) => (Option: option, Index: index))
                    .GroupBy(item => item.Option.Group)
                    .Select(group => new RimRewindSettingsSection
                    {
                        Key = group.Key,
                        Title = group.Key,
                        Options = group.Select(item => item.Option).ToArray(),
                        FirstIndex = group.First().Index,
                    }),
            ];

        private Widget BuildPreview()
        {
            if (!string.IsNullOrWhiteSpace(Query.Value) || ChangedOnly.Value)
                return null;
            return Index.Value switch
            {
                0 or 2 or 6 => new RimRewindPreview { Category = Index },
                3 => new RimRewindInstabilityPreview(),
                _ => null,
            };
        }
    }
}
