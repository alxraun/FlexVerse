// parents: @.frostline/graph/m-text-field-widget.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class TextField : Widget
    {
        [Prop]
        public Behavior<string> Value { get; set; } = "";

        [Prop]
        public Behavior<int> MaxLength { get; set; } = 9999;

        [Prop]
        public Behavior<Func<string, bool>> Validator { get; set; }

        [Prop]
        public Behavior<Action<string>> OnValueChange { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<string> Placeholder { get; set; }

        [Prop]
        public Behavior<GameFont> Font { get; set; } = GameFont.Small;

        [Prop]
        public Behavior<TextAnchor> TextAlign { get; set; } = TextAnchor.MiddleLeft;

        [Prop]
        public Behavior<Color> Color { get; set; } = UnityEngine.Color.white;

        [Prop]
        public Behavior<bool> DrawTextFieldBackground { get; set; } = true;

        [Prop]
        public Behavior<Action> OnSubmit { get; set; }

        [Prop]
        public Behavior<Action> OnCancel { get; set; }

        public TextField()
        {
            FlexShrink = 0f;
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (Disabled.Value)
                return;

            Engine.FocusManager.RequestFocus(this);
            e.StopPropagation();
        }

        public override void OnKeyDown(FlexEvent e)
        {
            if (Disabled.Value)
            {
                e.StopPropagation();
                return;
            }

            switch (e.KeyCode)
            {
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    OnSubmit.Value?.Invoke();
                    e.StopPropagation();
                    break;
                case KeyCode.Escape:
                    OnCancel.Value?.Invoke();
                    e.StopPropagation();
                    break;
            }
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                GameFont fontBefore = Text.Font;
                try
                {
                    Text.Font = Font.Value;

                    const float intrinsicWidth = 100f;
                    const float intrinsicHeight = 30f;
                    float measuredWidth = widthMode switch
                    {
                        MeasureMode.Exactly => width,
                        MeasureMode.AtMost => Mathf.Min(intrinsicWidth, width),
                        _ => intrinsicWidth,
                    };
                    float measuredHeight = heightMode switch
                    {
                        MeasureMode.Exactly => height,
                        MeasureMode.AtMost => Mathf.Min(intrinsicHeight, height),
                        _ => intrinsicHeight,
                    };
                    return new Size(Mathf.Max(0f, measuredWidth), Mathf.Max(0f, measuredHeight));
                }
                finally
                {
                    Text.Font = fontBefore;
                }
            };

        private static void RemoveNativeBackground(GUIStyle style)
        {
            style.normal.background = null;
            style.hover.background = null;
            style.focused.background = null;
            style.active.background = null;
            style.onNormal.background = null;
            style.onHover.background = null;
            style.onFocused.background = null;
            style.onActive.background = null;
        }

        internal void ProposeValue(string proposed)
        {
            if (
                !Disabled.Value
                && proposed != (Value.Value ?? "")
                && (Validator.Value?.Invoke(proposed) ?? true)
            )
                OnValueChange.Value?.Invoke(proposed);
        }

        public override void OnGUI(Rect calculatedRect)
        {
            if (!string.IsNullOrEmpty(Tooltip.Value))
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);

            var oldEnabled = GUI.enabled;
            var oldColor = GUI.color;
            var oldFont = Text.Font;
            var oldAnchor = Text.Anchor;
            GUIStyle nativeStyle = null;
            TextAnchor oldStyleAlignment = default;

            try
            {
                GUI.enabled = oldEnabled && !Disabled.Value;
                Text.Font = Font.Value;
                Text.Anchor = TextAlign.Value;

                nativeStyle = Text.CurTextFieldStyle;
                oldStyleAlignment = nativeStyle.alignment;

                if (!DrawTextFieldBackground.Value)
                {
                    nativeStyle = new GUIStyle(nativeStyle);
                    RemoveNativeBackground(nativeStyle);
                }

                nativeStyle.alignment = TextAlign.Value;
                string controlName = Engine.FocusManager.Register(this);
                if (Disabled.Value && Engine.FocusManager.HasFocus(this))
                    GUI.FocusControl(null);
                GUI.SetNextControlName(controlName);

                string currentValue = Value.Value ?? "";
                bool currentValid = Validator.Value?.Invoke(currentValue) ?? true;
                GUI.color =
                    oldColor
                    * Color.Value
                    * (currentValid ? UnityEngine.Color.white : UnityEngine.Color.red);

                string proposed = GUI.TextField(
                    calculatedRect,
                    currentValue,
                    Math.Max(0, MaxLength.Value),
                    nativeStyle
                );
                ProposeValue(proposed);

                if (
                    string.IsNullOrEmpty(currentValue)
                    && !Engine.FocusManager.HasFocus(this)
                    && !string.IsNullOrEmpty(Placeholder.Value)
                )
                {
                    var hintColor = GUI.color;
                    GUI.color = oldColor * Color.Value * new Color(0.7f, 0.7f, 0.7f, 0.7f);
                    var placeholderStyle = new GUIStyle(nativeStyle);
                    RemoveNativeBackground(placeholderStyle);
                    GUI.Label(calculatedRect, new GUIContent(Placeholder.Value), placeholderStyle);
                    GUI.color = hintColor;
                }
            }
            finally
            {
                if (nativeStyle != null && ReferenceEquals(nativeStyle, Text.CurTextFieldStyle))
                    nativeStyle.alignment = oldStyleAlignment;
                Text.Font = oldFont;
                Text.Anchor = oldAnchor;
                GUI.enabled = oldEnabled;
                GUI.color = oldColor;
            }
        }
    }
}
