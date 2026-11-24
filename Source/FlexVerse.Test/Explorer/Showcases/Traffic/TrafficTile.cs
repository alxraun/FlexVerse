// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class TrafficTile : View
    {
        [Prop]
        public Behavior<TrafficSkin> Skin { get; set; }

        [Prop]
        public Behavior<string> Caption { get; set; }

        [Prop]
        public Behavior<string> Value { get; set; }

        [Prop]
        public Behavior<bool> Hero { get; set; } = false;

        [Prop]
        public Behavior<float> Activity { get; set; } = 0f;

        public TrafficTile()
        {
            FlexDirection = FlexDirections.Column;
            PaddingAll = (TrafficLayouts.Gap * 0.5f + 10f).Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = Caption,
                    Font = GameFont.Tiny,
                    Color = Hero.Value ? TrafficSkin.HeroCaption : TrafficSkin.Caption,
                    TextAlign = TextAnchor.MiddleLeft,
                    TextWrap = false,
                    TextOverflow = TextOverflowMode.Ellipsis,
                    Height = 18f.Point(),
                },
                new View { FlexGrow = 1f, MinHeight = 0f.Point() },
                new Label
                {
                    Content = Value,
                    Font = GameFont.Medium,
                    Color = TrafficSkin.Value,
                    TextAlign = TextAnchor.MiddleLeft,
                    TextWrap = false,
                    TextOverflow = TextOverflowMode.Ellipsis,
                    Height = 26f.Point(),
                },
            ];

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var previous = GUI.color;
            try
            {
                GUI.color = Color.white;
                var style = Hero.Value ? Skin.Value.Hero : Skin.Value.Normal;
                var surface = rect.ContractedBy(TrafficLayouts.Gap * 0.5f);
                style.Draw(surface, false, false, false, false);
                GUI.color = new Color(
                    1f,
                    1f,
                    1f,
                    Mathf.Clamp01(Activity.Value) * (Hero.Value ? 0.3f : 0.75f)
                );
                Skin.Value.Transit.Draw(surface, false, false, false, false);
            }
            finally
            {
                GUI.color = previous;
            }
        }
    }
}
