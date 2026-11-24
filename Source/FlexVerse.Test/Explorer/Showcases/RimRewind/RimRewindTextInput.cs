// parents: @.frostline/graph/m-explorer.xml
using System;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindTextInput : Widget
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<string> Value { get; set; } = "";

        [Prop]
        public Behavior<string> Placeholder { get; set; } = "";

        [Prop]
        public Behavior<Action<string>> OnValueChange { get; set; }

        [State]
        private Behavior<string> Proposal { get; set; } = "";

        [State]
        private Behavior<int> Revision { get; set; } = 0;
        private RimRewind _screen;
        private string _controlName;
        private GUIStyle _style;
        private GUIStyle _hint;
        public bool CanReceiveFocus => true;

        public override void OnMount()
        {
            _screen = Session.Value;
            _controlName = "RimRewind:" + Guid.NewGuid().ToString("N");
            var font = Text.Font;
            try
            {
                Text.Font = GameFont.Small;
                _style = new GUIStyle(Text.CurTextFieldStyle)
                {
                    alignment = TextAnchor.MiddleLeft,
                    wordWrap = false,
                };
                RemoveBackground(_style);
                _style.normal.textColor = RimRewindPalette.OnSurface;
                _hint = new GUIStyle(_style);
                _hint.normal.textColor = RimRewindPalette.OnSurfaceVariant;
            }
            finally
            {
                Text.Font = font;
            }
            Watch.Revision.OnUpdate += _ => OnValueChange.Value?.Invoke(Proposal.Value);
        }

        private static void RemoveBackground(GUIStyle style)
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

        public override void OnUnmount()
        {
            _screen.HandleBlur(this);
            if (GUI.GetNameOfFocusedControl() == _controlName)
                GUI.FocusControl(null);
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (e.Button == 0)
                RequestNativeFocus();
        }

        public void RequestNativeFocus()
        {
            _screen.HandleFocus(this);
            GUI.FocusControl(_controlName);
        }

        public override void OnGUI(Rect rect)
        {
            var color = GUI.color;
            var font = Text.Font;
            var anchor = Text.Anchor;
            var wrap = Text.WordWrap;
            try
            {
                GUI.color = color;
                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleLeft;
                Text.WordWrap = false;
                GUI.SetNextControlName(_controlName);
                var proposed = GUI.TextField(rect, Value.Value ?? "", _style);
                if (proposed != Value.Value)
                {
                    Proposal.Target = proposed;
                    Revision.Target = Revision.Value + 1;
                }
                if (
                    string.IsNullOrEmpty(Value.Value)
                    && GUI.GetNameOfFocusedControl() != _controlName
                )
                    GUI.Label(rect, Placeholder.Value, _hint);
            }
            finally
            {
                GUI.color = color;
                Text.Font = font;
                Text.Anchor = anchor;
                Text.WordWrap = wrap;
            }
        }
    }
}
