// parents: @.frostline/graph/m-label-widget.xml

using System.Globalization;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public enum TextTransformMode
    {
        None,
        Uppercase,
        Lowercase,
        Capitalize,
    }

    public enum TextOverflowMode
    {
        Clip,
        Ellipsis,
    }

    public partial class Label : Widget
    {
        [Prop]
        public Behavior<string> Content { get; set; }

        [Prop(MarkDirtyOnSet = true, MarkDirtyOnUpdate = true)]
        public Behavior<TextTransformMode> TextTransform { get; set; } = TextTransformMode.None;

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> AutoTooltip { get; set; } = false;

        [Prop]
        public Behavior<bool> TextWrap { get; set; } = true;

        [Prop]
        public Behavior<TextOverflowMode> TextOverflow { get; set; } = TextOverflowMode.Clip;

        [Prop]
        public Behavior<TextAnchor> TextAlign { get; set; } = TextAnchor.MiddleCenter;

        [Prop]
        public Behavior<GameFont> Font { get; set; } = GameFont.Small;

        [Prop]
        public Behavior<Color> Color { get; set; } = UnityEngine.Color.white;

        public Label()
        {
            FlexShrink = 0f;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                string transformedContent = ApplyTextTransform(Content.Value);

                if (string.IsNullOrEmpty(transformedContent))
                    return new Size(
                        ConstrainMeasurement(0f, width, widthMode),
                        ConstrainMeasurement(0f, height, heightMode)
                    );

                GameFont fontBefore = Text.Font;
                bool wordWrapBefore = Text.WordWrap;
                try
                {
                    Text.Font = Font.Value;
                    Text.WordWrap = false;
                    Vector2 naturalSize = Text.CalcSize(transformedContent);
                    float measuredWidth = ConstrainMeasurement(naturalSize.x, width, widthMode);
                    float measuredHeight = naturalSize.y;

                    if (TextWrap.Value && widthMode != MeasureMode.Undefined)
                    {
                        Text.WordWrap = true;
                        measuredHeight =
                            measuredWidth > 0f
                                ? Text.CalcHeight(transformedContent, measuredWidth)
                                : 0f;
                    }

                    return new Size(
                        measuredWidth,
                        ConstrainMeasurement(measuredHeight, height, heightMode)
                    );
                }
                finally
                {
                    Text.Font = fontBefore;
                    Text.WordWrap = wordWrapBefore;
                }
            };

        private static float ConstrainMeasurement(float natural, float available, MeasureMode mode)
        {
            return mode switch
            {
                MeasureMode.Exactly => Mathf.Max(0f, available),
                MeasureMode.AtMost => Mathf.Min(natural, Mathf.Max(0f, available)),
                _ => natural,
            };
        }

        private Rect GetTextRect(Rect rect)
        {
            Node node = Engine.GetNode(this);
            float left = node.LayoutGetPadding(Edge.Left) + node.LayoutGetBorder(Edge.Left);
            float top = node.LayoutGetPadding(Edge.Top) + node.LayoutGetBorder(Edge.Top);
            float right = node.LayoutGetPadding(Edge.Right) + node.LayoutGetBorder(Edge.Right);
            float bottom = node.LayoutGetPadding(Edge.Bottom) + node.LayoutGetBorder(Edge.Bottom);
            return new Rect(
                rect.x + left,
                rect.y + top,
                Mathf.Max(0f, rect.width - left - right),
                Mathf.Max(0f, rect.height - top - bottom)
            );
        }

        public override void OnGUI(Rect calculatedRect)
        {
            string transformedContent = ApplyTextTransform(Content.Value);

            var originalGuiColor = GUI.color;
            var originalFont = Text.Font;
            var originalAnchor = Text.Anchor;
            var originalWordWrap = Text.WordWrap;

            try
            {
                GUI.color = originalGuiColor * Color.Value;
                Text.Font = Font.Value;
                Text.Anchor = TextAlign.Value;
                Text.WordWrap = TextWrap.Value;

                Rect textRect = GetTextRect(calculatedRect);
                string visibleContent = transformedContent;
                bool useEllipsis =
                    !TextWrap.Value && TextOverflow.Value == TextOverflowMode.Ellipsis;
                if (useEllipsis && !string.IsNullOrEmpty(transformedContent))
                {
                    visibleContent = Text.ClampTextWithEllipsis(textRect, transformedContent);
                }

                bool isElided = visibleContent != transformedContent;

                if (
                    !string.IsNullOrEmpty(visibleContent)
                    && textRect.width > 0f
                    && textRect.height > 0f
                )
                {
                    GUI.BeginGroup(textRect);
                    try
                    {
                        Widgets.Label(
                            new Rect(0f, 0f, textRect.width, textRect.height),
                            visibleContent
                        );
                    }
                    finally
                    {
                        GUI.EndGroup();
                    }
                }

                string tooltip = Tooltip.Value;
                if (string.IsNullOrEmpty(tooltip) && AutoTooltip.Value && isElided)
                    tooltip = transformedContent;
                if (!string.IsNullOrEmpty(tooltip))
                    TooltipHandler.TipRegion(calculatedRect, tooltip);
            }
            finally
            {
                GUI.color = originalGuiColor;
                Text.Font = originalFont;
                Text.Anchor = originalAnchor;
                Text.WordWrap = originalWordWrap;
            }
        }

        private string ApplyTextTransform(string content)
        {
            if (string.IsNullOrEmpty(content) || TextTransform.Value == TextTransformMode.None)
            {
                return content;
            }

            return TextTransform.Value switch
            {
                TextTransformMode.Uppercase => content.ToUpper(),
                TextTransformMode.Lowercase => content.ToLower(),
                TextTransformMode.Capitalize => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(
                    content.ToLower()
                ),
                _ => content,
            };
        }
    }
}
