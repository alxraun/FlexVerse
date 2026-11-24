// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSidebar : View
    {
        public RimRewindSidebar()
        {
            Width = 224f.Point();
            MarginRight = 24f.Point();
            PaddingAll = 8f.Point();
            BackgroundColor = RimRewindSkin.Sidebar;
            BorderRadius = RimRewindSkin.CornerRadius;
            BorderAll = 0f.Point();
            BorderColor = RimRewindSkin.Alpha(RimRewindSkin.Line, .4f);
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                RimRewindSkin.BuildCaption("Configuration"),
                new RimRewindScrollView
                {
                    PaddingRight = 14f.Point(),
                    PaddingBottom = 0f.Point(),
                    Children = new WidgetList(
                        Enumerable
                            .Range(0, RimRewindOptions.Categories.Length)
                            .Select(index => new RimRewindNavigationItem
                            {
                                Key = index,
                                Index = index,
                            })
                    ),
                },
                new RimRewindArchiveSummary(),
            ];
    }
}
