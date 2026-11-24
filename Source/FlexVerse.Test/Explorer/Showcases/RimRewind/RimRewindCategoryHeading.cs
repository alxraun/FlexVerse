// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindCategoryHeading : View
    {
        [Prop]
        public Behavior<int> Index { get; set; } = 0;

        [Prop]
        public Behavior<int> Count { get; set; } = 0;

        public RimRewindCategoryHeading()
        {
            MarginBottom = 12f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildTitle(),
                new Label
                {
                    Content = Index.Select(index => RimRewindOptions.Descriptions[index]),
                    Font = GameFont.Small,
                    Color = RimRewindSkin.Muted,
                    TextAlign = TextAnchor.UpperLeft,
                    TextWrap = true,
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
                        Content = Index.Select(index =>
                            $"{index + 1:00}  {RimRewindOptions.Categories[index]}"
                        ),
                        Font = GameFont.Medium,
                        Color = RimRewindSkin.Text,
                        TextAlign = TextAnchor.MiddleLeft,
                        Height = 36f.Point(),
                        FlexGrow = 1f,
                    },
                    RimRewindSkin.BuildCaption(Count.Select(count => $"Settings: {count}")),
                },
            };
    }
}
