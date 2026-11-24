// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSlider : Widget
    {
        [Prop]
        public Behavior<float> Value { get; set; } = 0f;

        [Prop]
        public Behavior<float> MinValue { get; set; } = 0f;

        [Prop]
        public Behavior<float> MaxValue { get; set; } = 1f;

        [Prop]
        public Behavior<float> Step { get; set; } = .01f;

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<string> Tooltip { get; set; } = "";

        [Prop]
        public Behavior<Action<float>> OnValueChange { get; set; }

        [State]
        private Behavior<float> Hover { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<float> Pressure { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<float> Fill { get; set; }

        [State]
        private Behavior<bool> Focused { get; set; } = false;

        [State]
        private Behavior<RimRewindSliderGesture> Input { get; set; } =
            Static.Value(default(RimRewindSliderGesture));
        private const float FastTension = 180f;
        private const float FastFriction = 26f;
        private RimRewindSliderInput _nativeInput;

        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;
        public bool CanReceiveFocus => !Disabled.Value;

        public RimRewindSlider()
        {
            Width = 100f.Percent();
            Height = 40f.Point();
            FlexShrink = 1f;
        }

        public override void OnMount()
        {
            _screen = Session.Value;
            Focused = _screen.FocusOwner.Select(owner => ReferenceEquals(owner, this));
            _nativeInput = new RimRewindSliderInput();
            Fill = Spring.State(UnitValue, FastTension, FastFriction);
            Watch.Value.OnUpdate += _ => Retarget();
            Watch.Value.OnActivate += Retarget;
            Watch.MinValue.OnUpdate += _ => Retarget();
            Watch.MinValue.OnActivate += Retarget;
            Watch.MaxValue.OnUpdate += _ => Retarget();
            Watch.MaxValue.OnActivate += Retarget;
            Watch.Input.OnUpdate += _ => HandleInput();
        }

        public override void OnUnmount()
        {
            _screen.HandleBlur(this);
            _nativeInput.Release();
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
                _nativeInput.RequestFocus();
            }
        }

        private float UnitValue =>
            Mathf.InverseLerp(
                MinValue.Value,
                Mathf.Max(MinValue.Value, MaxValue.Value),
                Value.Value
            );

        private void Retarget()
        {
            if (Fill != null)
                Fill.Target = UnitValue;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindSliderTrack
                {
                    Fill = Fill,
                    Focused = Focused,
                    Hover = Hover,
                    Pressure = Pressure,
                    Disabled = Disabled,
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                },
            ];

        public override void OnMouseEnter(FlexEvent e) => Hover.Target = Disabled.Value ? 0f : 1f;

        public override void OnMouseLeave(FlexEvent e) => Hover.Target = 0f;

        public override void OnGUI(Rect rect)
        {
            if (_nativeInput.TryRead(rect, Disabled.Value, Fill.Value, out var gesture))
                Input.Target = gesture;
            if (!string.IsNullOrEmpty(Tooltip.Value))
                TooltipHandler.TipRegion(rect, Tooltip.Value);
        }

        private void HandleInput()
        {
            var gesture = Input.Value;
            switch (gesture.Action)
            {
                case RimRewindSliderGesture.Kind.Press:
                    Pressure.Target = 1f;
                    Propose(Mathf.Lerp(MinValue.Value, MaxValue.Value, gesture.Position));
                    break;
                case RimRewindSliderGesture.Kind.Grab:
                case RimRewindSliderGesture.Kind.Drag:
                    Pressure.Target = 1f;
                    Propose(Mathf.Lerp(MinValue.Value, MaxValue.Value, gesture.Position));
                    Fill = Spring.State(UnitValue, FastTension, FastFriction);
                    break;
                case RimRewindSliderGesture.Kind.Release:
                    Pressure.Target = 0f;
                    break;
            }
        }

        private void Propose(float value)
        {
            if (Disabled.Value)
                return;
            var step = Mathf.Max(.00001f, Step.Value);
            var proposed = MinValue.Value + Mathf.Round((value - MinValue.Value) / step) * step;
            proposed = Mathf.Clamp(
                proposed,
                MinValue.Value,
                Mathf.Max(MinValue.Value, MaxValue.Value)
            );
            if (!Mathf.Approximately(proposed, Value.Value))
                OnValueChange.Value?.Invoke(proposed);
        }
    }
}
