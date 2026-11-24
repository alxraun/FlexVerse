// parents: @.frostline/graph/m-flex-events.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Wraps a native IMGUI event with bubbling metadata. (M-FLEX-EVENTS)</summary>
    public class FlexEvent(Event nativeEvent, Widget target)
    {
        /// <summary>Gets the underlying native event.</summary>
        public Event NativeEvent { get; } = nativeEvent;

        /// <summary>Gets the widget initially targeted by the event.</summary>
        public Widget Target { get; } = target;

        /// <summary>Gets or sets the widget currently handling the event.</summary>
        public Widget CurrentTarget { get; set; } = target;

        private bool _isPropagationStopped = false;

        /// <summary>Reports whether bubbling was halted. (REQ-004)</summary>
        public bool IsPropagationStopped => _isPropagationStopped;

        /// <summary>Gets the native event type.</summary>
        public EventType Type => NativeEvent.type;

        /// <summary>Gets the mouse position in screen coordinates.</summary>
        public Vector2 MousePosition => NativeEvent.mousePosition;

        /// <summary>Gets the key code of keyboard events.</summary>
        public KeyCode KeyCode => NativeEvent.keyCode;

        /// <summary>Gets the typed character of keyboard events.</summary>
        public char Character => NativeEvent.character;

        /// <summary>Gets the active keyboard modifiers.</summary>
        public EventModifiers Modifiers => NativeEvent.modifiers;

        /// <summary>Gets the scroll or mouse delta.</summary>
        public Vector2 ScrollDelta => NativeEvent.delta;

        /// <summary>Gets the mouse button index.</summary>
        public int Button => NativeEvent.button;

        /// <summary>Halts further bubbling of the event. (REQ-004)</summary>
        public void StopPropagation()
        {
            _isPropagationStopped = true;
        }
    }

    internal class FlexEventDispatcher(
        WidgetTree widgetTree,
        WidgetHitTester hitTester,
        FocusManager focusManager
    )
    {
        private readonly WidgetTree _widgetTree = widgetTree;
        private readonly WidgetHitTester _hitTester = hitTester;
        private readonly FocusManager _focusManager = focusManager;

        private List<Widget> _lastHoverPath = [];
        private Vector2 _lastMousePosition;

        public void ProcessEvents(Event nativeEvent)
        {
            if (nativeEvent == null)
                return;

            var currentHoveredWidget = _hitTester.HitTest(nativeEvent.mousePosition);

            ProcessHoverEvents(nativeEvent, currentHoveredWidget);
            ProcessMouseMoveEvents(nativeEvent.mousePosition, currentHoveredWidget);
            DispatchNativeMouseEvents(nativeEvent, currentHoveredWidget);
            DispatchNativeKeyboardEvents(nativeEvent);
        }

        private void DispatchNativeKeyboardEvents(Event nativeEvent)
        {
            if (!nativeEvent.isKey)
            {
                return;
            }

            Widget target = _focusManager.GetFocusedWidget();
            if (target == null)
            {
                Log.Debug("M-FLEX-EVENTS", "key.no.focus", ("type", nativeEvent.type));
                return;
            }

            switch (nativeEvent.type)
            {
                case EventType.KeyDown:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnKeyDown(flexEvent)
                    );
                    break;
                case EventType.KeyUp:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnKeyUp(flexEvent)
                    );
                    break;
            }
        }

        private void DispatchNativeMouseEvents(Event nativeEvent, Widget currentHoveredWidget)
        {
            Widget target = null;
            if (nativeEvent.isMouse || nativeEvent.type == EventType.ScrollWheel)
            {
                target = currentHoveredWidget;
            }

            switch (nativeEvent.type)
            {
                case EventType.MouseDown:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnMouseDown(flexEvent)
                    );
                    break;
                case EventType.MouseUp:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnMouseUp(flexEvent)
                    );
                    break;
                case EventType.MouseDrag:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnMouseDrag(flexEvent)
                    );
                    break;
                case EventType.ScrollWheel:
                    DispatchMouseEvent(
                        nativeEvent,
                        target,
                        (widget, flexEvent) => widget.OnScrollWheel(flexEvent)
                    );
                    break;
            }
        }

        private void ProcessMouseMoveEvents(
            Vector2 currentMousePosition,
            Widget currentHoveredWidget
        )
        {
            if (currentMousePosition != _lastMousePosition)
            {
                if (currentHoveredWidget != null)
                {
                    var moveEvent = new FlexEvent(
                        new Event
                        {
                            type = EventType.MouseMove,
                            mousePosition = currentMousePosition,
                        },
                        currentHoveredWidget
                    );
                    BubbleEvent(currentHoveredWidget, moveEvent, (c, e) => c.OnMouseMove(e));
                }
                _lastMousePosition = currentMousePosition;
            }
        }

        private void ProcessHoverEvents(Event nativeEvent, Widget currentHoveredWidget)
        {
            var currentHoverPath =
                currentHoveredWidget != null
                    ? _widgetTree.GetAncestorPath(currentHoveredWidget)
                    : [];

            var lastHoveredWidget = _lastHoverPath.LastOrDefault();
            if (currentHoveredWidget != lastHoveredWidget)
            {
                Log.Debug(
                    "M-FLEX-EVENTS",
                    "hover.changed",
                    ("current", currentHoveredWidget),
                    ("last", lastHoveredWidget)
                );
            }

            int divergenceIndex = 0;
            while (
                divergenceIndex < _lastHoverPath.Count
                && divergenceIndex < currentHoverPath.Count
                && _lastHoverPath[divergenceIndex] == currentHoverPath[divergenceIndex]
            )
            {
                divergenceIndex++;
            }

            if (_lastHoverPath.Count > 0)
            {
                var leaveEvent = new FlexEvent(nativeEvent, _lastHoverPath.Last());
                for (int i = _lastHoverPath.Count - 1; i >= divergenceIndex; i--)
                {
                    var widget = _lastHoverPath[i];

                    if (!_widgetTree.Contains(widget))
                        continue;
                    leaveEvent.CurrentTarget = widget;
                    widget.OnMouseLeave(leaveEvent);
                }
            }

            if (currentHoverPath.Count > 0)
            {
                var enterEvent = new FlexEvent(nativeEvent, currentHoverPath.Last());
                for (int i = divergenceIndex; i < currentHoverPath.Count; i++)
                {
                    var widget = currentHoverPath[i];
                    enterEvent.CurrentTarget = widget;
                    widget.OnMouseEnter(enterEvent);
                }
            }

            _lastHoverPath = currentHoverPath;
        }

        private void DispatchMouseEvent(
            Event nativeEvent,
            Widget target,
            Action<Widget, FlexEvent> dispatchAction
        )
        {
            if (target == null)
            {
                return;
            }

            Log.Debug(
                "M-FLEX-EVENTS",
                "event.dispatch.begin",
                ("type", nativeEvent.type),
                ("target", target)
            );

            var flexEvent = new FlexEvent(nativeEvent, target);
            BubbleEvent(target, flexEvent, dispatchAction);

            if (flexEvent.IsPropagationStopped)
            {
                nativeEvent.Use();
            }
        }

        private void BubbleEvent(
            Widget startWidget,
            FlexEvent flexEvent,
            Action<Widget, FlexEvent> dispatchAction
        )
        {
            if (startWidget == null)
            {
                return;
            }

            foreach (var widget in _widgetTree.TraverseAncestors(startWidget))
            {
                flexEvent.CurrentTarget = widget;
                Log.Debug(
                    "M-FLEX-EVENTS",
                    "event.bubble",
                    ("widget", widget),
                    ("eventType", flexEvent.Type)
                );
                dispatchAction(widget, flexEvent);

                if (flexEvent.IsPropagationStopped)
                {
                    Log.Debug("M-FLEX-EVENTS", "event.propagation.stopped", ("widget", widget));
                    break;
                }
            }
        }
    }
}
