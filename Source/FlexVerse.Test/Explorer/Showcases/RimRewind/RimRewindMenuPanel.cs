// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindMenuPanel : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindPopup> Popup { get; set; }

        [State]
        private Behavior<int> Focused { get; set; }

        [State]
        private Behavior<float> Arrival { get; set; } = Spring.State(.8f, 80f, 16f);

        public RimRewindMenuPanel()
        {
            PaddingAll = 8f.Point();
            BorderRadius = RimRewindSkin.CornerRadius;
            BorderAll = 0f.Point();
            BorderColor = RimRewindSkin.Alpha(RimRewindSkin.Accent, .7f);
            BackgroundColor = RimRewindSkin.Raised;
            BoxShadow = new Shadow(new Vector2(0f, 8f), 18f, 1f, new Color(0f, 0f, 0f, .32f));
        }

        public override void OnMount()
        {
            Focused = Session.Value.MenuFocus;
            Arrival.Target = 1f;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindMenuHighlight
                {
                    Position = Spring.To(
                        Focused.Value * 40f,
                        RimRewindSkin.HoverTension,
                        RimRewindSkin.HoverFriction
                    ),
                },
                .. Popup.Value.Options.Select((label, index) => BuildOption(label, index)),
            ];

        private Widget BuildOption(string label, int index) =>
            new RimRewindMenuOption
            {
                Key = index,
                Label = (index == Popup.Value.Selected ? "•  " : "    ") + label,
                Index = index,
                Height = 40f.Point(),
                Scale = Arrival.Select(value => Vector2.one * (.98f + .02f * Mathf.Clamp01(value))),
            };
    }
}
