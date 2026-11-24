// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoSection : View
    {
        [Prop]
        public Behavior<BentoSectionMotion> Motion { get; set; }

        [Prop]
        public Behavior<BentoSkin> Skin { get; set; }

        [Prop]
        public Behavior<int> Identity { get; set; }
        private static readonly string[] Titles = ["Revenue", "Audience", "Infrastructure"];
        private static readonly string[] Values = ["$48.2k", "9.1k", "99.98%"];

        public BentoSection()
        {
            FlexDirection = FlexDirections.Column;
            FlexBasis = 0f.Point();
            MinWidth = 0f.Point();
            MinHeight = 0f.Point();
            MarginHorizontal = 3f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            var model = Motion.Value;
            return
            [
                BuildCard(
                    0,
                    Titles[Identity.Value],
                    Values[Identity.Value],
                    model.TopWeight,
                    new BentoChart { Motion = model.Panels[0], Identity = Identity },
                    true
                ),
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    FlexGrow = model.MiddleWeight,
                    FlexBasis = 0f.Point(),
                    MinHeight = 0f.Point(),
                    MinWidth = 0f.Point(),
                    Children = new WidgetList
                    {
                        BuildCard(
                            1,
                            "Activity",
                            "1.2M",
                            model.Panels[1].Split,
                            new BentoChart
                            {
                                Motion = model.Panels[1],
                                Identity = Identity,
                                Bars = 7,
                            }
                        ),
                        BuildCard(
                            2,
                            "Distribution",
                            "",
                            model.Panels[2].Split,
                            new BentoGrid { Motion = model.Panels[2] }
                        ),
                    },
                },
                BuildCard(
                    3,
                    "Live services",
                    "",
                    model.BottomWeight,
                    new BentoStream { Motion = model.Panels[3], Reveal = model.Reveal }
                ),
            ];
        }

        private Widget BuildCard(
            int index,
            string title,
            string value,
            Behavior<float> weight,
            Widget content,
            bool hero = false
        ) =>
            new BentoCard
            {
                Key = index,
                Skin = Skin,
                Caption = title,
                ValueText = value,
                Reveal = Motion.Value.Reveal,
                Accent = Motion.Value.Panels[index].Fill,
                PaddingAll = Motion.Value.Insets,
                Hero = hero,
                FlexGrow = weight,
                Content = content,
            };
    }
}
