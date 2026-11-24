// parents: @.frostline/graph/m-checkbox-widget.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class Checkbox : Widget
    {
        [Prop]
        public Behavior<bool> Checked { get; set; }

        [Prop]
        public Behavior<Action<bool>> OnValueChange { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        public Checkbox()
        {
            Width = Widgets.CheckboxSize.Point();
            Height = Widgets.CheckboxSize.Point();
            FlexShrink = 0;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, availableWidth, widthMode, availableHeight, heightMode) =>
            {
                float size = Widgets.CheckboxSize;

                float measuredWidth = size;
                if (widthMode == MeasureMode.Exactly)
                    measuredWidth = availableWidth;
                if (widthMode == MeasureMode.AtMost)
                    measuredWidth = Mathf.Min(size, availableWidth);

                float measuredHeight = size;
                if (heightMode == MeasureMode.Exactly)
                    measuredHeight = availableHeight;
                if (heightMode == MeasureMode.AtMost)
                    measuredHeight = Mathf.Min(size, availableHeight);

                return new Size(measuredWidth, measuredHeight);
            };

        internal void ProposeChecked(bool proposed)
        {
            if (!Disabled.Value && proposed != Checked.Value)
                OnValueChange.Value?.Invoke(proposed);
        }

        public override void OnGUI(Rect calculatedRect)
        {
            if (!string.IsNullOrEmpty(Tooltip.Value))
            {
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
            }

            bool currentState = Checked.Value;

            Widgets.Checkbox(
                calculatedRect.x,
                calculatedRect.y,
                ref currentState,
                disabled: Disabled.Value
            );

            ProposeChecked(currentState);
        }
    }
}
