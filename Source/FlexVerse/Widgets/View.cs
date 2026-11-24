// parents: @.frostline/graph/m-view-widget.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class View : Widget
    {
        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<System.Action> OnClick { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<Color> BackgroundColor { get; set; }

        [Prop]
        public Behavior<Color> BorderColor { get; set; } = Color.white;

        [Prop]
        public Behavior<Texture> BackgroundImage { get; set; }

        [Prop]
        public Behavior<BackgroundSize> BackgroundSize { get; set; } =
            FlexVerse.BackgroundSize.Cover;

        [Prop]
        public Behavior<Vector2> BackgroundPosition { get; set; } = new Vector2(0.5f, 0.5f);

        [Prop]
        public Behavior<Color> BackgroundImageTintColor { get; set; } = Color.white;

        [Prop]
        public Behavior<float> BorderRadius { get; set; } = 0f;

        [Prop]
        public Behavior<float> BorderTopLeftRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderTopRightRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderBottomRightRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderBottomLeftRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<Shadow> BoxShadow { get; set; } = Shadow.None;

        public View()
        {
            FlexDirection = FlexDirections.Column;
            FlexShrink = 0;
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return (Children.Value != default) ? Children.Value : Enumerable.Empty<Widget>();
        }

        public override void OnGUI(Rect calculatedRect)
        {
            var radii = CornerRadii.Resolve(
                calculatedRect,
                BorderRadius.Value,
                BorderTopLeftRadius.Value,
                BorderTopRightRadius.Value,
                BorderBottomRightRadius.Value,
                BorderBottomLeftRadius.Value
            );
            ShadowDrawer.Draw(calculatedRect, radii, BoxShadow.Value);
            BackgroundDrawer.Draw(
                calculatedRect,
                BackgroundColor.Value,
                BackgroundImage.Value,
                BackgroundSize.Value,
                BackgroundPosition.Value,
                BackgroundImageTintColor.Value,
                radii
            );
            BorderDrawer.Draw(this, calculatedRect, BorderColor.Value, radii);

            if (!Disabled.Value && OnClick.Value != null)
            {
                if (Widgets.ButtonInvisible(calculatedRect))
                    OnClick.Value.Invoke();
            }

            if (!string.IsNullOrEmpty(Tooltip.Value))
            {
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
            }
        }
    }
}
