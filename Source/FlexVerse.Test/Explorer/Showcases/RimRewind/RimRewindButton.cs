// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindButton : Widget
    {
        [Prop]
        public Behavior<string> Label { get; set; } = "";

        [Prop]
        public Behavior<string> Detail { get; set; } = "";

        [Prop]
        public Behavior<string> Tooltip { get; set; } = "";

        [Prop]
        public Behavior<Action> OnActivate { get; set; }

        [Prop]
        public Behavior<bool> Selected { get; set; } = false;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<bool> Primary { get; set; } = false;

        [Prop]
        public Behavior<bool> Quiet { get; set; } = false;

        [Prop]
        public Behavior<bool> Compact { get; set; } = false;

        [Prop]
        public Behavior<TextAnchor> TextAlign { get; set; } = TextAnchor.MiddleCenter;

        [Prop]
        public Behavior<bool> Outlined { get; set; } = false;

        /// <summary>Has no self-painted highlight; an external sliding highlight owns indication.</summary>
        [Prop]
        public Behavior<bool> Flat { get; set; } = false;

        [Prop]
        public Behavior<float> Radius { get; set; } = RimRewindSkin.CornerRadius;

        [State]
        private Behavior<float> Hover { get; set; } =
            Spring.State(0f, RimRewindSkin.HoverTension, RimRewindSkin.HoverFriction);

        [State]
        private Behavior<float> Pressure { get; set; } =
            Spring.State(0f, RimRewindSkin.PressTension, RimRewindSkin.PressFriction);

        [State]
        private Behavior<bool> Focused { get; set; } = false;

        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        private RimRewind _screen;
        public bool CanReceiveFocus => !Disabled.Value;
        private int _nativeControl;

        public RimRewindButton()
        {
            Height = 40f.Point();
            MinWidth = 0f.Point();
            FlexDirection = FlexDirections.Column;
            FlexShrink = 0f;
        }

        public override void OnMount()
        {
            _screen = Session.Value;
            Focused = _screen.FocusOwner.Select(owner => ReferenceEquals(owner, this));
        }

        public override void OnUnmount()
        {
            _screen.HandleBlur(this);
            if (_nativeControl != 0 && GUIUtility.keyboardControl == _nativeControl)
                GUIUtility.keyboardControl = 0;
        }

        public override void OnGUI(Rect rect) =>
            _nativeControl = GUIUtility.GetControlID(FocusType.Passive, rect);

        public void RequestNativeFocus()
        {
            if (!CanReceiveFocus)
                return;
            _screen.HandleFocus(this);
            GUI.FocusControl(null);
            GUIUtility.keyboardControl = _nativeControl;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButtonFace
                {
                    Label = Label,
                    Outlined = Outlined,
                    Radius = Radius,
                    Detail = Detail,
                    Tooltip = Tooltip,
                    Hover = Hover,
                    Focused = Focused,
                    Pressure = Pressure,
                    Selection = Spring.To(Selected.Value ? 1f : 0f, 80f, 16f),
                    Quiet = Quiet,
                    Primary = Primary,
                    Compact = Compact,
                    Flat = Flat,
                    TextAlign = TextAlign,
                    Availability = Disabled.Select(disabled => disabled ? .38f : 1f),
                    FlexGrow = 1f,
                    MinWidth = 0f.Point(),
                    Translate = Pressure.Select(pressure => new Vector2(
                        0f,
                        pressure * RimRewindSkin.PressOffset
                    )),
                },
            ];

        public override void OnMouseEnter(FlexEvent e) => Hover.Target = Disabled.Value ? 0f : 1f;

        public override void OnMouseLeave(FlexEvent e)
        {
            Hover.Target = 0f;
            Pressure.Target = 0f;
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (e.Button != 0)
                return;
            if (!Disabled.Value)
            {
                RequestNativeFocus();
                Pressure.Target = 1f;
            }
            e.StopPropagation();
        }

        public override void OnMouseUp(FlexEvent e)
        {
            if (e.Button != 0)
                return;
            var activate = Pressure.Target > .5f && !Disabled.Value;
            Pressure.Target = 0f;
            e.StopPropagation();
            if (activate)
                OnActivate.Value?.Invoke();
        }
    }
}
