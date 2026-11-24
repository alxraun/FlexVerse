// parents: @.frostline/graph/m-focus-manager.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>
    /// Adapter between the FlexVerse widget model and the string-based native
    /// IMGUI focus system: assigns each focusable widget instance a stable
    /// control name, forwards focus requests to GUI.FocusControl and resolves
    /// the focused widget for keyboard event dispatch (M-FOCUS-MANAGER).
    /// </summary>
    internal sealed class FocusManager(WidgetEngine engine)
    {
        private readonly WidgetEngine _engine = engine;
        private readonly Dictionary<Widget, string> _widgetToName = [];
        private readonly Dictionary<string, Widget> _nameToWidget = [];
        private readonly HashSet<Widget> _liveWidgets = [];
        private int _nextControlSequence;
        private bool _isFrameOpen;

        private string Prefix => "FV:" + _engine.Key + ":";

        public string Register(Widget widget)
        {
            if (!_widgetToName.TryGetValue(widget, out var name))
            {
                name = Prefix + _nextControlSequence++;
                _widgetToName[widget] = name;
                _nameToWidget[name] = widget;
                Log.Debug(
                    "M-FOCUS",
                    "focusable.registered",
                    ("widget", widget),
                    ("controlName", name)
                );
            }

            _liveWidgets.Add(widget);
            return name;
        }

        public void RequestFocus(Widget widget)
        {
            if (widget == null)
            {
                return;
            }

            if (!_widgetToName.TryGetValue(widget, out var name))
            {
                Log.Warning("M-FOCUS", "focus.request.unregistered", ("widget", widget));
                return;
            }

            GUI.FocusControl(name);
            Log.Info("M-FOCUS", "focus.requested", ("widget", widget), ("controlName", name));
        }

        public Widget GetFocusedWidget()
        {
            var focusedName = GUI.GetNameOfFocusedControl();
            if (
                string.IsNullOrEmpty(focusedName)
                || !focusedName.StartsWith(Prefix, StringComparison.Ordinal)
            )
            {
                return null;
            }

            return _nameToWidget.TryGetValue(focusedName, out var widget) ? widget : null;
        }

        public bool HasFocus(Widget widget)
        {
            return widget != null && ReferenceEquals(GetFocusedWidget(), widget);
        }

        public void Unregister(Widget widget)
        {
            if (widget == null || !_widgetToName.TryGetValue(widget, out var name))
            {
                return;
            }

            _widgetToName.Remove(widget);
            _nameToWidget.Remove(name);
            _liveWidgets.Remove(widget);

            if (GUI.GetNameOfFocusedControl() == name)
            {
                GUI.FocusControl(null);
            }

            Log.Debug(
                "M-FOCUS",
                "focusable.unregistered",
                ("widget", widget),
                ("controlName", name)
            );
        }

        internal void BeginFrame()
        {
            _liveWidgets.Clear();
            _isFrameOpen = true;
        }

        internal void EndFrame()
        {
            if (!_isFrameOpen)
            {
                return;
            }

            var staleWidgets = new List<Widget>();
            foreach (var widget in _widgetToName.Keys)
            {
                if (!_liveWidgets.Contains(widget))
                {
                    staleWidgets.Add(widget);
                }
            }

            foreach (var widget in staleWidgets)
            {
                Unregister(widget);
            }

            _isFrameOpen = false;
        }
    }
}
