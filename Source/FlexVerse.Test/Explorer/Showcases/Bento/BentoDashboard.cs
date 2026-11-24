// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoDashboard : View
    {
        [Prop]
        public Behavior<BentoPresentation> Scene { get; set; }

        [Prop]
        public Behavior<BentoSkin> Skin { get; set; }
        private static readonly string[] SummaryNames =
        [
            "Requests",
            "Retention",
            "Response",
            "Capacity",
        ];
        private static readonly string[] SummaryValues = ["1.2M", "92%", "182 ms", "84 GB"];

        public BentoDashboard()
        {
            FlexDirection = FlexDirections.Column;
            MinWidth = 0f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildHeader(),
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    FlexGrow = 1f,
                    MinHeight = 0f.Point(),
                    Children = new WidgetList(
                        Scene.Value.Sections.Select(
                            (section, index) =>
                                new BentoSection
                                {
                                    Key = index,
                                    Identity = index,
                                    Motion = section,
                                    Skin = Skin,
                                    FlexGrow = section.Weight,
                                }
                        )
                    ),
                },
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    Height = Scene.Value.FooterHeight,
                    FlexShrink = 0f,
                    MarginTop = 5f.Point(),
                    Children = new WidgetList(Enumerable.Range(0, 4).Select(BuildSummary)),
                },
            ];

        private Widget BuildHeader() =>
            new View
            {
                Height = 38f.Point(),
                FlexShrink = 0f,
                FlexDirection = FlexDirections.Row,
                AlignItems = Align.Center,
                PaddingHorizontal = 6f.Point(),
                Children = new WidgetList
                {
                    new Label
                    {
                        Content = "BENTO",
                        Font = GameFont.Medium,
                        TextAlign = TextAnchor.MiddleLeft,
                        FlexGrow = 1f,
                        Height = 30f.Point(),
                        Color = BentoSkin.Value,
                    },
                    new Label
                    {
                        Content = Scene.Value.Phase,
                        Font = GameFont.Tiny,
                        Width = 150f.Point(),
                        Height = 22f.Point(),
                        TextWrap = false,
                        Color = BentoSkin.Caption,
                    },
                    new View
                    {
                        FlexDirection = FlexDirections.Row,
                        AlignItems = Align.Center,
                        Children = new WidgetList(
                            Enumerable
                                .Range(0, 6)
                                .Select(index => new BentoRect
                                {
                                    Key = index,
                                    Width = 14f.Point(),
                                    Height = 3f.Point(),
                                    MarginLeft = 4f.Point(),
                                    Ink = Scene.Value.PhaseIndex.Select(phase =>
                                        phase == index
                                            ? new Color(0.72f, 0.83f, 0.88f)
                                            : new Color(0.2f, 0.23f, 0.27f)
                                    ),
                                })
                        ),
                    },
                },
            };

        private Widget BuildSummary(int index)
        {
            var section = Scene.Value.Sections[index % 3];
            var panel = section.Panels[index % 4];
            return new BentoCard
            {
                Key = index,
                Skin = Skin,
                FlexGrow = panel.Split,
                Caption = SummaryNames[index],
                ValueText = SummaryValues[index],
                Reveal = section.Reveal,
                Accent = panel.Fill,
                Content = new View
                {
                    FlexDirection = FlexDirections.Row,
                    FlexGrow = 1f,
                    MinHeight = 0f.Point(),
                    AlignItems = Align.Center,
                    Children = new WidgetList(
                        Enumerable
                            .Range(0, 5)
                            .Select(segment => new BentoRect
                            {
                                Key = segment,
                                Height = 5f.Point(),
                                FlexBasis = 0f.Point(),
                                MarginRight = 3f.Point(),
                                Ink = segment % 2 == 0 ? panel.Ink : panel.MutedInk,
                                FlexGrow = panel.Bias.Select(bias =>
                                    0.5f + (segment + 1) * bias * 0.3f
                                ),
                            })
                    ),
                },
            };
        }
    }
}
