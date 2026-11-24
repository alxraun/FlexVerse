// parents: @.frostline/graph/m-button-widget.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class Button : Widget
    {
        [Prop]
        public Behavior<string> Label { get; set; }

        [Prop]
        public Behavior<Action> OnClick { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> DrawButtonBackground { get; set; } = true;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<GameFont> Font { get; set; } = GameFont.Small;

        [Prop]
        public Behavior<TextAnchor> TextAlign { get; set; } = TextAnchor.MiddleCenter;

        [Prop]
        public Behavior<Color> Color { get; set; } = UnityEngine.Color.white;

        public Button()
        {
            FlexShrink = 0f;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                if (string.IsNullOrEmpty(Label.Value))
                {
                    float defaultSize = 20f;
                    return new Size(defaultSize, defaultSize);
                }

                GameFont fontBefore = Text.Font;
                TextAnchor anchorBefore = Text.Anchor;
                bool wordWrapBefore = Text.WordWrap;

                try
                {
                    Text.Font = Font.Value;
                    Text.Anchor = TextAlign.Value;

                    Text.WordWrap = false;

                    float internalButtonHorizontalPadding = 8f;
                    float internalButtonVerticalPadding = 4f;

                    Vector2 textSize = Text.CalcSize(Label.Value);
                    float measuredWidth = textSize.x + internalButtonHorizontalPadding;
                    float measuredHeight = textSize.y + internalButtonVerticalPadding;

                    float finalWidth = measuredWidth;
                    if (widthMode == MeasureMode.Exactly)
                        finalWidth = width;
                    else if (widthMode == MeasureMode.AtMost)
                        finalWidth = Mathf.Min(measuredWidth, width);

                    float finalHeight = measuredHeight;
                    if (heightMode == MeasureMode.Exactly)
                        finalHeight = height;
                    else if (heightMode == MeasureMode.AtMost)
                        finalHeight = Mathf.Min(measuredHeight, height);

                    return new Size(finalWidth, finalHeight);
                }
                finally
                {
                    Text.Font = fontBefore;
                    Text.Anchor = anchorBefore;
                    Text.WordWrap = wordWrapBefore;
                }
            };

        public override void OnMouseUp(FlexEvent e)
        {
            if (!Disabled.Value)
                OnClick.Value?.Invoke();
            e.StopPropagation();
        }

        public override void OnGUI(Rect calculatedRect)
        {
            var originalGuiColor = GUI.color;
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;
            var originalWordWrap = Text.WordWrap;

            try
            {
                GUI.color = originalGuiColor * Color.Value;

                Text.Font = Font.Value;
                Text.Anchor = TextAlign.Value;

                Text.WordWrap = false;

                Widgets.ButtonText(
                    calculatedRect,
                    Label.Value ?? "",
                    DrawButtonBackground.Value,
                    true,
                    !Disabled.Value,
                    TextAlign.Value
                );

                if (!string.IsNullOrEmpty(Tooltip.Value))
                    TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
            }
            finally
            {
                GUI.color = originalGuiColor;
                Text.Font = originalFont;
                Text.Anchor = originalAnchor;
                Text.WordWrap = originalWordWrap;
            }
        }
    }
}
