// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Unity control capture and focus; model changes belong to the slider widget.</summary>
    internal sealed class RimRewindSliderInput
    {
        private int _capture;
        private int _control;
        private int _revision;
        private float _grabOffset;
        public bool Focused => _control != 0 && GUIUtility.keyboardControl == _control;

        public void RequestFocus()
        {
            GUI.FocusControl(null);
            GUIUtility.keyboardControl = _control;
        }

        public void Release()
        {
            if (_capture != 0 && GUIUtility.hotControl == _capture)
                GUIUtility.hotControl = 0;
            _capture = 0;
        }

        public bool TryRead(
            Rect rect,
            bool disabled,
            float shown,
            out RimRewindSliderGesture gesture
        )
        {
            var e = Event.current;
            _control = GUIUtility.GetControlID(FocusType.Keyboard, rect);
            var action = RimRewindSliderGesture.Kind.None;
            if (_capture != 0 && (disabled || GUIUtility.hotControl != _capture))
            {
                Release();
                action = RimRewindSliderGesture.Kind.Release;
            }
            else if (
                !disabled
                && e.type == EventType.MouseDown
                && e.button == 0
                && rect.Contains(e.mousePosition)
            )
            {
                GUI.FocusControl(null);
                GUIUtility.keyboardControl = _control;
                GUIUtility.hotControl = _control;
                _capture = _control;
                var pointer = (e.mousePosition.x - rect.x - 9f) / Mathf.Max(1f, rect.width - 18f);
                var onHandle = Mathf.Abs(pointer - shown) * Mathf.Max(1f, rect.width - 18f) <= 8f;
                _grabOffset = onHandle ? pointer - shown : 0f;
                action = onHandle
                    ? RimRewindSliderGesture.Kind.Grab
                    : RimRewindSliderGesture.Kind.Press;
            }
            else if (_capture == _control && GUIUtility.hotControl == _control)
            {
                if (e.type == EventType.MouseDrag)
                    action = RimRewindSliderGesture.Kind.Drag;
                if (e.rawType == EventType.MouseUp)
                {
                    Release();
                    action = RimRewindSliderGesture.Kind.Release;
                }
            }
            if (action == RimRewindSliderGesture.Kind.None)
            {
                gesture = default;
                return false;
            }
            var position = Mathf.Clamp01(
                (e.mousePosition.x - rect.x - 9f) / Mathf.Max(1f, rect.width - 18f) - _grabOffset
            );
            gesture = new RimRewindSliderGesture(action, position, ++_revision);
            if (
                e.type != EventType.Used
                && e.type != EventType.Repaint
                && e.type != EventType.Layout
            )
                e.Use();
            return true;
        }
    }
}
