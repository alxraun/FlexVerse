// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoCard : View
    {
        [Prop]
        public Behavior<BentoSkin> Skin { get; set; }

        [Prop]
        public Behavior<string> Caption { get; set; }

        [Prop]
        public Behavior<string> ValueText { get; set; } = "";

        [Prop]
        public Behavior<Widget> Content { get; set; }

        [Prop]
        public Behavior<float> Reveal { get; set; }

        [Prop]
        public Behavior<float> Accent { get; set; }

        [Prop]
        public Behavior<bool> Hero { get; set; } = false;

        public BentoCard()
        {
            FlexDirection = FlexDirections.Column;
            FlexBasis = 0f.Point();
            FlexShrink = 1f;
            MinWidth = 0f.Point();
            MinHeight = 0f.Point();
            MarginAll = 3f.Point();
            PaddingAll = 9f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = Caption,
                    Font = GameFont.Tiny,
                    Height = 18f.Point(),
                    TextAlign = TextAnchor.MiddleLeft,
                    TextWrap = false,
                    TextOverflow = TextOverflowMode.Ellipsis,
                    Color = Reveal.Select(alpha => new Color(0.66f, 0.7f, 0.75f, alpha)),
                },
                new Label
                {
                    Content = ValueText,
                    Font = Hero.Value ? GameFont.Medium : GameFont.Small,
                    Height = (
                        string.IsNullOrEmpty(ValueText.Value) ? 0f
                        : Hero.Value ? 29f
                        : 23f
                    ).Point(),
                    TextAlign = TextAnchor.MiddleLeft,
                    TextWrap = false,
                    TextOverflow = TextOverflowMode.Ellipsis,
                    Color = Reveal.Select(alpha => new Color(0.94f, 0.95f, 0.96f, alpha)),
                },
                new View
                {
                    FlexGrow = 1f,
                    FlexShrink = 1f,
                    MinHeight = 0f.Point(),
                    MinWidth = 0f.Point(),
                    Children = new WidgetList { Content.Value },
                },
            ];

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var previous = GUI.color;
            try
            {
                GUI.color = new Color(1f, 1f, 1f, Mathf.Clamp01(Reveal.Value));
                (Hero.Value ? Skin.Value.Hero : Skin.Value.Normal).Draw(
                    rect,
                    false,
                    false,
                    false,
                    false
                );
                GUI.color = new Color(1f, 1f, 1f, Reveal.Value * Accent.Value * 0.22f);
                Skin.Value.Accent.Draw(rect, false, false, false, false);
            }
            finally
            {
                GUI.color = previous;
            }
        }
    }
}
