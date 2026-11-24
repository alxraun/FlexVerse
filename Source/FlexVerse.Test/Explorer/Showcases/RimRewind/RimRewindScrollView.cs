// parents: @.frostline/graph/m-explorer.xml

using System;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Stock ScrollView geometry with spring wheel motion and independently faded native scrollbars.</summary>
    public partial class RimRewindScrollView : ScrollView
    {
        [Prop]
        public Behavior<Action> OnUserScroll { get; set; }

        [Prop]
        public Behavior<float> WheelStep { get; set; } = 42f;

        [Prop]
        public Behavior<float> IdleOpacity { get; set; } = .18f;

        [Prop]
        public Behavior<float> FadeDelay { get; set; } = .75f;

        [State]
        private Behavior<float> BarOpacity { get; set; } =
            Spring.State(.18f, tension: 80f, friction: 16f);

        [State]
        private Behavior Fade { get; set; }

        [State]
        private Behavior<Vector2> Maximum { get; set; } = Vector2.zero;

        [State]
        private Behavior<bool> PointerOnBar { get; set; } = false;

        [State]
        private Behavior<(Vector2 Position, bool Direct, int Revision)> NativeInput { get; set; } =
            (Vector2.zero, false, 0);

        private bool _hasGeometry;
        private int _dragControl;
        private const float BarWidth = 16f;

        public RimRewindScrollView()
        {
            PaddingRight = 24f.Point();
            PaddingBottom = 18f.Point();
            MinHeight = 0f.Point();
        }

        public override void OnMount()
        {
            ScrollPosition = Motion(Vector2.zero);
            BarOpacity.Target = Mathf.Clamp01(IdleOpacity.Value);
            BarOpacity.Value = BarOpacity.Target;
            Watch.NativeInput.OnUpdate += _ => HandleNativeInput();
            Watch.Maximum.OnUpdate += _ => HandleBoundsChange();
            Watch.PointerOnBar.OnUpdate += _ =>
            {
                if (PointerOnBar.Value)
                    WakeScrollbar();
            };
            Watch.ScrollPosition.OnUpdate += _ => OnScroll.Value?.Invoke(ScrollPosition.Value);
        }

        public override void OnUnmount()
        {
            if (_dragControl != 0 && GUIUtility.hotControl == _dragControl)
                GUIUtility.hotControl = 0;
        }

        public void Reveal(Widget widget)
        {
            if (!_hasGeometry)
                return;
            var viewport = Engine.TransformService.ToScreen(this);
            var bounds = Engine.TransformService.ToScreen(widget);
            var delta = new Vector2(
                bounds.xMin < viewport.xMin
                    ? bounds.xMin - viewport.xMin
                    : Mathf.Max(0f, bounds.xMax - viewport.xMax),
                bounds.yMin < viewport.yMin
                    ? bounds.yMin - viewport.yMin
                    : Mathf.Max(0f, bounds.yMax - viewport.yMax)
            );
            if (delta.sqrMagnitude < .01f)
                return;
            ScrollPosition.Target = Clamp(ScrollPosition.Value + delta);
            WakeScrollbar();
        }

        public void ScrollTo(Vector2 position)
        {
            var target = Clamp(position);
            if ((target - ScrollPosition.Target).sqrMagnitude < .01f)
                return;
            ScrollPosition.Target = target;
            WakeScrollbar();
        }

        public void StopScrolling() => ScrollPosition = Motion(ScrollPosition.Value);

        private static Behavior<Vector2> Motion(Vector2 value) =>
            Spring.State(value, tension: 80f, friction: 16f, restThreshold: .1f);

        private Vector2 Clamp(Vector2 value) =>
            new(
                ScrollHorizontal.Value ? Mathf.Clamp(value.x, 0f, Maximum.Value.x) : 0f,
                ScrollVertical.Value ? Mathf.Clamp(value.y, 0f, Maximum.Value.y) : 0f
            );

        private bool Dragging => _dragControl != 0 && GUIUtility.hotControl == _dragControl;

        private void WakeScrollbar()
        {
            BarOpacity.Target = 1f;
            Fade = Timeline.Create(
                Timeline.Sequence(
                    Timeline.WaitUntil(() =>
                        !ScrollPosition.IsUpdating && !PointerOnBar.Value && !Dragging
                    ),
                    Timeline.Wait(Mathf.Max(0f, FadeDelay.Value)),
                    Timeline.Call(() => BarOpacity.Target = Mathf.Clamp01(IdleOpacity.Value))
                )
            );
        }

        private void HandleNativeInput()
        {
            OnUserScroll.Value?.Invoke();
            var input = NativeInput.Value;
            if (input.Direct)
                ScrollPosition = Motion(Clamp(input.Position));
            else
                ScrollPosition.Target = Clamp(input.Position);
            WakeScrollbar();
        }

        private void HandleBoundsChange()
        {
            var position = Clamp(ScrollPosition.Value);
            var target = Clamp(ScrollPosition.Target);
            if (position != ScrollPosition.Value || target != ScrollPosition.Target)
                ScrollPosition = Spring.FromTo(
                    position,
                    target,
                    Vector2.zero,
                    80f,
                    16f,
                    restThreshold: .1f
                );
        }

        public override void OnScrollWheel(FlexEvent e)
        {
            if (Disabled.Value || !_hasGeometry)
                return;
            OnUserScroll.Value?.Invoke();
            var delta = e.ScrollDelta * Mathf.Max(1f, WheelStep.Value);
            if (
                ScrollHorizontal.Value
                && (!ScrollVertical.Value || (e.Modifiers & EventModifiers.Shift) != 0)
            )
                delta = new Vector2(delta.x + delta.y, 0f);
            var target = Clamp(ScrollPosition.Target + delta);
            if ((target - ScrollPosition.Target).sqrMagnitude < .001f)
            {
                e.StopPropagation();
                return;
            }
            ScrollPosition.Target = target;
            WakeScrollbar();
            e.StopPropagation();
        }

        public override ChildrenGuiSpace BeginChildrenGUI(Rect calculatedRect, Rect viewport)
        {
            if (
                !_hasGeometry
                || Engine.DidLayoutCalculationHappenThisCycle
                || viewport != _cachedScrollViewViewport
            )
            {
                _cachedScrollViewViewport = viewport;
                _cachedContentViewRect = CalculateContentRect(
                    viewport,
                    Engine.GetLayoutResults(),
                    calculatedRect
                );
                _hasGeometry = true;
            }
            Maximum.Target = new Vector2(
                Mathf.Max(0f, _cachedContentViewRect.width - viewport.width),
                Mathf.Max(0f, _cachedContentViewRect.height - viewport.height)
            );
            var position = Clamp(ScrollPosition.Value);

            var vertical = new Rect(
                viewport.xMax - BarWidth,
                viewport.y,
                BarWidth,
                viewport.height - (ScrollHorizontal.Value && Maximum.Value.x > 0f ? BarWidth : 0f)
            );
            var horizontal = new Rect(
                viewport.x,
                viewport.yMax - BarWidth,
                viewport.width - (ScrollVertical.Value && Maximum.Value.y > 0f ? BarWidth : 0f),
                BarWidth
            );
            var pointerOnBar =
                !Disabled.Value
                && (
                    (
                        ScrollVertical.Value
                        && Maximum.Value.y > 0f
                        && vertical.Contains(Event.current.mousePosition)
                    )
                    || (
                        ScrollHorizontal.Value
                        && Maximum.Value.x > 0f
                        && horizontal.Contains(Event.current.mousePosition)
                    )
                );
            PointerOnBar.Target = pointerOnBar;

            var direct = Event.current.type == EventType.MouseDrag;
            var color = GUI.color;
            var enabled = GUI.enabled;
            try
            {
                GUI.enabled = enabled && !Disabled.Value;
                GUI.color = color * new Color(1f, 1f, 1f, Mathf.Clamp01(BarOpacity.Value));
                var hotControl = GUIUtility.hotControl;
                var next = position;
                if (ScrollVertical.Value && Maximum.Value.y > 0f)
                    next.y = GUI.VerticalScrollbar(
                        vertical,
                        position.y,
                        viewport.height,
                        0f,
                        _cachedContentViewRect.height
                    );
                if (ScrollHorizontal.Value && Maximum.Value.x > 0f)
                    next.x = GUI.HorizontalScrollbar(
                        horizontal,
                        position.x,
                        viewport.width,
                        0f,
                        _cachedContentViewRect.width
                    );
                if (GUIUtility.hotControl != 0 && GUIUtility.hotControl != hotControl)
                    _dragControl = GUIUtility.hotControl;
                if (next != position && !Disabled.Value)
                {
                    var proposed = direct ? next : ScrollPosition.Target + next - position;
                    NativeInput.Target = (Clamp(proposed), direct, NativeInput.Value.Revision + 1);
                }
            }
            finally
            {
                GUI.color = color;
                GUI.enabled = enabled;
            }

            Widgets.BeginScrollView(viewport, ref position, _cachedContentViewRect, false);
            try
            {
                return new ChildrenGuiSpace(
                    viewport.position,
                    appliesChildrenTransform: true,
                    clipsChildren: true
                );
            }
            catch
            {
                Widgets.EndScrollView();
                throw;
            }
        }
    }
}
