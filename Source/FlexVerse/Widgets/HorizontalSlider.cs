// parents: @.frostline/graph/m-horizontal-slider-widget.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class HorizontalSlider : Widget
    {
        [Prop]
        public Behavior<float> Value { get; set; }

        [Prop]
        public Behavior<float> MinValue { get; set; } = 0f;

        [Prop]
        public Behavior<float> MaxValue { get; set; } = 1f;

        [Prop]
        public Behavior<Action<float>> OnValueChange { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; }

        public HorizontalSlider()
        {
            FlexShrink = 1;
            Width = 100.Percent();
        }

        protected override MeasureFunc MeasureFunc =>
            (node, availableWidth, widthMode, availableHeight, heightMode) =>
            {
                float minIntrinsicWidth = 80f;
                float preferredWidth = minIntrinsicWidth;

                if (Width.Value.unit == Unit.Point && Width.Value.value >= 0)
                {
                    preferredWidth = Width.Value.value;
                }

                preferredWidth = Mathf.Max(preferredWidth, minIntrinsicWidth);

                float measuredWidth;
                if (widthMode == MeasureMode.Exactly)
                {
                    measuredWidth = availableWidth;
                }
                else if (widthMode == MeasureMode.AtMost)
                {
                    measuredWidth = Mathf.Min(preferredWidth, availableWidth);
                }
                else
                {
                    measuredWidth = preferredWidth;
                }

                float defaultIntrinsicHeight = 14f;
                float preferredHeight = defaultIntrinsicHeight;

                if (Height.Value.unit == Unit.Point && Height.Value.value >= 0)
                {
                    preferredHeight = Height.Value.value;
                }

                float measuredHeight;
                if (heightMode == MeasureMode.Exactly)
                {
                    measuredHeight = availableHeight;
                }
                else if (heightMode == MeasureMode.AtMost)
                {
                    measuredHeight = Mathf.Min(preferredHeight, availableHeight);
                }
                else
                {
                    measuredHeight = preferredHeight;
                }

                return new Size(measuredWidth, measuredHeight);
            };

        internal void ProposeValue(float proposed)
        {
            if (!Disabled.Value && !Mathf.Approximately(proposed, Value.Value))
                OnValueChange.Value?.Invoke(proposed);
        }

        public override void OnGUI(Rect calculatedRect)
        {
            if (!string.IsNullOrEmpty(Tooltip.Value))
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);

            var oldEnabled = GUI.enabled;
            var oldColor = GUI.color;
            try
            {
                if (Disabled.Value)
                {
                    GUI.enabled = false;
                    GUI.color = oldColor * UnityEngine.Color.gray;
                    GenUI.AbsorbClicksInRect(calculatedRect);
                }

                float oldValue = Value.Value;
                float proposed = Widgets.HorizontalSlider(
                    calculatedRect,
                    oldValue,
                    MinValue.Value,
                    MaxValue.Value
                );
                ProposeValue(proposed);
            }
            finally
            {
                GUI.enabled = oldEnabled;
                GUI.color = oldColor;
            }
        }
    }
}
