// parents: @.frostline/graph/m-area-slider-widget.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class AreaSlider : Widget
    {
        [Prop]
        public Behavior<Vector2> Value { get; set; } = Vector2.zero;

        [Prop]
        public Behavior<FloatRange> XRange { get; set; } = new FloatRange(0f, 1f);

        [Prop]
        public Behavior<FloatRange> YRange { get; set; } = new FloatRange(0f, 1f);

        [Prop]
        public Behavior<Action<Vector2>> OnValueChange { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<string> Label { get; set; }

        [Prop]
        public Behavior<Color> LabelColor { get; set; } = Color.white;

        private bool _isDragging;

        public AreaSlider()
        {
            Width = 100.Point();
            Height = 100.Point();
            FlexGrow = 0;
            FlexShrink = 1;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, availableWidth, widthMode, availableHeight, heightMode) =>
            {
                float desiredSize = 100f;
                float measuredWidth = desiredSize;
                if (widthMode == MeasureMode.Exactly)
                    measuredWidth = availableWidth;
                else if (widthMode == MeasureMode.AtMost)
                    measuredWidth = Mathf.Min(desiredSize, availableWidth);
                measuredWidth = Mathf.Max(0f, measuredWidth);

                float measuredHeight = desiredSize;
                if (heightMode == MeasureMode.Exactly)
                    measuredHeight = availableHeight;
                else if (heightMode == MeasureMode.AtMost)
                    measuredHeight = Mathf.Min(desiredSize, availableHeight);
                measuredHeight = Mathf.Max(0f, measuredHeight);

                return new Size(measuredWidth, measuredHeight);
            };

        public override void OnGUI(Rect calculatedRect)
        {
            var oldEnabled = GUI.enabled;
            var oldColor = GUI.color;
            try
            {
                if (!string.IsNullOrEmpty(Tooltip.Value))
                {
                    TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
                }

                if (Disabled.Value)
                {
                    _isDragging = false;
                    GUI.enabled = false;
                    GUI.color = oldColor * Color.gray;
                    GenUI.AbsorbClicksInRect(calculatedRect);
                }

                Widgets.DrawBoxSolid(calculatedRect, new Color(0.2f, 0.2f, 0.2f));

                float handleX =
                    Mathf.InverseLerp(XRange.Value.min, XRange.Value.max, Value.Value.x)
                    * calculatedRect.width;

                float handleY =
                    (1f - Mathf.InverseLerp(YRange.Value.min, YRange.Value.max, Value.Value.y))
                    * calculatedRect.height;

                Vector2 handlePixelPos = new(
                    calculatedRect.x + handleX,
                    calculatedRect.y + handleY
                );

                float handleSize = 12f;
                Rect handleRect = new(
                    handlePixelPos.x - handleSize / 2f,
                    handlePixelPos.y - handleSize / 2f,
                    handleSize,
                    handleSize
                );

                if (!string.IsNullOrEmpty(Label.Value))
                {
                    var originalGuiColor = GUI.color;
                    var originalFont = Text.Font;
                    var originalAnchor = Text.Anchor;
                    var originalWordWrap = Text.WordWrap;

                    try
                    {
                        GUI.color = originalGuiColor * LabelColor.Value;
                        Text.Font = GameFont.Small;
                        Text.Anchor = TextAnchor.MiddleCenter;
                        Text.WordWrap = false;

                        Widgets.Label(calculatedRect, Label.Value);
                    }
                    finally
                    {
                        GUI.color = originalGuiColor;
                        Text.Font = originalFont;
                        Text.Anchor = originalAnchor;
                        Text.WordWrap = originalWordWrap;
                    }
                }

                GUI.DrawTexture(
                    handleRect,
                    ContentFinder<Texture2D>.Get("UI/Buttons/SliderHandle", true)
                );
            }
            finally
            {
                GUI.enabled = oldEnabled;
                GUI.color = oldColor;
            }
        }

        private bool TryGetLocalPoint(FlexEvent e, out Vector2 point, out Vector2 size)
        {
            point = Vector2.zero;
            size = Vector2.zero;
            if (
                Engine?.TransformService == null
                || !Engine.GetLayoutResults().TryGetValue(this, out var layout)
            )
                return false;
            size = layout.size;
            return Engine.TransformService.TryToLocal(this, e.MousePosition, out point);
        }

        internal void ProposeValue(Vector2 proposed)
        {
            var current = Value.Value;
            if (
                !Disabled.Value
                && (
                    !Mathf.Approximately(proposed.x, current.x)
                    || !Mathf.Approximately(proposed.y, current.y)
                )
            )
                OnValueChange.Value?.Invoke(proposed);
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (
                Disabled.Value
                || e.Button != 0
                || !TryGetLocalPoint(e, out var point, out var size)
            )
                return;

            if (new Rect(Vector2.zero, size).Contains(point))
            {
                _isDragging = true;
                e.StopPropagation();
            }
        }

        public override void OnMouseDrag(FlexEvent e)
        {
            if (
                !_isDragging
                || Disabled.Value
                || !TryGetLocalPoint(e, out var point, out var size)
                || size.x <= 0f
                || size.y <= 0f
            )
                return;

            float u = Mathf.Clamp01(point.x / size.x);
            float v = Mathf.Clamp01(point.y / size.y);
            var proposed = new Vector2(
                Mathf.Lerp(XRange.Value.min, XRange.Value.max, u),
                Mathf.Lerp(YRange.Value.max, YRange.Value.min, v)
            );

            ProposeValue(proposed);
            e.StopPropagation();
        }

        public override void OnMouseUp(FlexEvent e)
        {
            if (!_isDragging)
                return;
            _isDragging = false;
            e.StopPropagation();
        }

        public override void OnMouseLeave(FlexEvent e)
        {
            _isDragging = false;
        }

        public override void OnUnmount()
        {
            _isDragging = false;
        }
    }
}
