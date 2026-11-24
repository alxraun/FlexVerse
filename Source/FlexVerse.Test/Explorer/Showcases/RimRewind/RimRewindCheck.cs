// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindCheck : Widget
    {
        [Prop]
        public Behavior<bool> Checked { get; set; } = false;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<Action<bool>> OnValueChange { get; set; }

        [State]
        private Behavior<float> Hover { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<(bool Value, int Revision)> Input { get; set; } = (false, 0);

        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<bool> Focused { get; set; } = false;
        private RimRewind _screen;
        private string _controlName;
        public bool CanReceiveFocus => !Disabled.Value;

        public RimRewindCheck()
        {
            Width = 52f.Point();
            Height = 40f.Point();
        }

        public override void OnMount()
        {
            _screen = Session.Value;
            _controlName = "RimRewind:" + Guid.NewGuid().ToString("N");
            Focused = _screen.FocusOwner.Select(owner => ReferenceEquals(owner, this));
            Watch.Input.OnUpdate += _ => OnValueChange.Value?.Invoke(Input.Value.Value);
        }

        public override void OnUnmount()
        {
            _screen.HandleBlur(this);
            if (GUI.GetNameOfFocusedControl() == _controlName)
                GUI.FocusControl(null);
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (e.Button == 0 && CanReceiveFocus)
                RequestNativeFocus();
        }

        public void RequestNativeFocus()
        {
            if (CanReceiveFocus)
            {
                _screen.HandleFocus(this);
                GUI.FocusControl(_controlName);
            }
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindCheckFace
                {
                    Mark = Spring.To(Checked.Value ? 1f : 0f, 80f, 16f),
                    Checked = Checked,
                    Hover = Hover,
                    Focused = Focused,
                    Disabled = Disabled,
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                },
            ];

        public override void OnMouseEnter(FlexEvent e) => Hover.Target = Disabled.Value ? 0f : 1f;

        public override void OnMouseLeave(FlexEvent e) => Hover.Target = 0f;

        public override void OnGUI(Rect rect)
        {
            var enabled = GUI.enabled;
            try
            {
                GUI.enabled = enabled && !Disabled.Value;
                GUI.SetNextControlName(_controlName);
                var proposed = GUI.Toggle(rect, Checked.Value, GUIContent.none, GUIStyle.none);
                if (!Disabled.Value && proposed != Checked.Value)
                    Input.Target = (proposed, Input.Value.Revision + 1);
            }
            finally
            {
                GUI.enabled = enabled;
            }
        }
    }
}
