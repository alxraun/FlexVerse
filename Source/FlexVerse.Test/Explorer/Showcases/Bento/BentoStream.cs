// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoStream : View
    {
        [Prop]
        public Behavior<BentoPanelMotion> Motion { get; set; }

        [Prop]
        public Behavior<float> Reveal { get; set; }
        private static readonly string[] Names = ["Gateway", "Worker", "Storage", "Queue"];

        public BentoStream()
        {
            FlexDirection = FlexDirections.Column;
            FlexGrow = 1f;
            MinHeight = 0f.Point();
            MinWidth = 0f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            Enumerable.Range(0, 4).Select(BuildRow).ToArray();

        private Widget BuildRow(int row) =>
            new View
            {
                Key = row,
                FlexDirection = FlexDirections.Row,
                FlexBasis = 0f.Point(),
                MinHeight = 0f.Point(),
                MarginVertical = 2f.Point(),
                AlignItems = Align.Center,
                FlexGrow = Motion.Value.Bias.Select(bias =>
                    0.7f + (row % 2 == 0 ? bias : 1f - bias) * 0.9f
                ),
                Children = new WidgetList
                {
                    new BentoRect
                    {
                        Width = 4f.Point(),
                        Height = 65f.Percent(),
                        Ink = Motion.Value.Ink,
                        MarginRight = 7f.Point(),
                    },
                    new Label
                    {
                        Content = Names[row],
                        Font = GameFont.Tiny,
                        Height = 100f.Percent(),
                        TextAlign = TextAnchor.MiddleLeft,
                        TextWrap = false,
                        TextOverflow = TextOverflowMode.Ellipsis,
                        FlexGrow = 1f,
                        MinWidth = 0f.Point(),
                        Color = Reveal.Select(alpha => new Color(0.65f, 0.69f, 0.74f, alpha)),
                    },
                    new View
                    {
                        Width = 42f.Percent(),
                        Height = 60f.Percent(),
                        FlexDirection = FlexDirections.Row,
                        Children = new WidgetList(
                            Enumerable
                                .Range(0, 4)
                                .Select(index => new BentoRect
                                {
                                    Key = index,
                                    Height = 100f.Percent(),
                                    MarginHorizontal = 1f.Point(),
                                    FlexBasis = 0f.Point(),
                                    Ink = index <= row ? Motion.Value.Ink : Motion.Value.MutedInk,
                                    FlexGrow = Motion.Value.Fill.Select(fill =>
                                        0.4f + (index + 1) * fill * 0.3f
                                    ),
                                })
                        ),
                    },
                },
            };
    }
}
