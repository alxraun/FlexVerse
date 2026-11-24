// parents: @.frostline/graph/m-dbg-inspect.xml

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using FlexTools.WidgetInspector.Accessors;
using FlexTools.WidgetInspector.Adapters;
using FlexTools.WidgetInspector.Isolation;
using FlexTools.WidgetInspector.Models;
using FlexTools.WidgetInspector.Tracking;
using FlexVerse;

namespace FlexTools.WidgetInspector.Inspection
{
    /// <summary>Captures widget trees and observes reactive property changes.</summary>
    public sealed class WidgetInspector : IDisposable
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();
        private readonly ConditionalWeakTable<Widget, Observation> _widgets = new();
        private readonly List<WeakReference<Observation>> _observations = new();
        private readonly IBehaviorAdapter _adapter = new DefaultAdapter();
        private readonly ChangeTracker _changes;
        private int _nextId;

        /// <summary>Creates an inspector backed by the given change tracker.</summary>
        public WidgetInspector(ChangeTracker changes) => _changes = changes;

        /// <summary>Subscribes to changes for every widget in the engine tree.</summary>
        public void Observe(WidgetEngine engine)
        {
            _observations.RemoveAll(reference => !reference.TryGetTarget(out _));
            if (!InspectorIsolation.IsIsolated(engine))
                Visit(engine, engine.GetRoot());
        }

        /// <summary>Returns an immutable snapshot of the engine tree.</summary>
        public DebugNode Inspect(WidgetEngine engine)
        {
            if (engine == null || InspectorIsolation.IsIsolated(engine))
                return null;
            return Capture(engine, engine.GetRoot());
        }

        /// <summary>Releases all observation subscriptions.</summary>
        public void Dispose()
        {
            foreach (var reference in _observations)
                if (reference.TryGetTarget(out var observation))
                    observation.Dispose();
            _observations.Clear();
        }

        private Observation GetObservation(Widget widget)
        {
            return _widgets.GetValue(
                widget,
                target =>
                {
                    var accessors = new List<RefAccessor>();
                    foreach (
                        var property in Properties.GetOrAdd(
                            target.GetType(),
                            type =>
                                type.GetProperties(
                                        BindingFlags.Public
                                            | BindingFlags.NonPublic
                                            | BindingFlags.Instance
                                    )
                                    .Where(p =>
                                        p.GetIndexParameters().Length == 0
                                        && Attribute
                                            .GetCustomAttributes(p, true)
                                            .Any(a => a is FVPAttribute)
                                    )
                                    .OrderBy(p => p.Name)
                                    .ToArray()
                        )
                    )
                    {
                        var attribute = Attribute
                            .GetCustomAttributes(property, true)
                            .OfType<FVPAttribute>()
                            .First();
                        var category =
                            attribute is StateAttribute ? "State"
                            : attribute is PropAttribute ? "Prop"
                            : attribute is InheritedContextAttribute ? "InheritedContext"
                            : "FVP";
                        accessors.Add(
                            new RefAccessor(AccessorFactory.GetRef(target, property), category)
                        );
                    }
                    var observation = new Observation(target, ++_nextId, accessors, _changes);
                    _observations.Add(new WeakReference<Observation>(observation));
                    return observation;
                }
            );
        }

        private void Visit(WidgetEngine engine, Widget widget)
        {
            if (widget == null)
                return;
            GetObservation(widget);
            foreach (var child in engine.GetChildren(widget))
                Visit(engine, child);
        }

        private DebugNode Capture(WidgetEngine engine, Widget widget)
        {
            if (widget == null)
                return null;
            var observation = GetObservation(widget);
            var children = engine
                .GetChildren(widget)
                .Select(child => Capture(engine, child))
                .Where(child => child != null)
                .ToArray();
            var properties = observation
                .Accessors.Select(accessor =>
                {
                    var behavior = accessor.Property.Get();
                    var slots =
                        behavior == null
                            ? new Dictionary<string, string>()
                            : _adapter
                                .GetSlots(behavior)
                                .ToDictionary(slot => slot.Name, ReadDisplay);
                    var value = slots.TryGetValue("Value", out var display) ? display : "null";
                    return new DebugPropertyNode(
                        accessor.Name,
                        accessor.Category,
                        value,
                        behavior == null
                            ? null
                            : new DebugBehaviorNode(
                                behavior.GetType().Name,
                                slots,
                                null,
                                RuntimeHelpers.GetHashCode(behavior)
                            )
                    );
                })
                .ToArray();
            return new DebugNode(widget, observation.Id, children, properties);
        }

        private static string ReadDisplay(IAccessor accessor)
        {
            try
            {
                return accessor.GetValue()?.ToString() ?? "null";
            }
            catch (Exception exception)
            {
                FlexVerse.Log.Warning(
                    "M-DBG-INSPECT",
                    "property.read.failed",
                    ("property", accessor.Name),
                    ("error", exception.Message)
                );
                return "<unavailable>";
            }
        }

        private sealed class Observation : IDisposable
        {
            private readonly Widget _widget;
            private readonly ChangeTracker _changes;
            private bool _disposed;

            /// <summary>Gets the stable observation identifier.</summary>
            public int Id { get; }

            /// <summary>Gets the property accessors under observation.</summary>
            public IReadOnlyList<RefAccessor> Accessors { get; }

            /// <summary>Subscribes to property and unmount events.</summary>
            public Observation(
                Widget widget,
                int id,
                IReadOnlyList<RefAccessor> accessors,
                ChangeTracker changes
            )
            {
                _widget = widget;
                _changes = changes;
                Id = id;
                Accessors = accessors;
                foreach (var accessor in Accessors)
                {
                    accessor.Property.Changed += Changed;
                    new WatchRef(accessor.Property).OnUpdate += Updated;
                }
                widget.Unmount += Dispose;
                Changed();
            }

            /// <summary>Removes all observation subscriptions.</summary>
            public void Dispose()
            {
                if (_disposed)
                    return;
                _disposed = true;
                foreach (var accessor in Accessors)
                {
                    accessor.Property.Changed -= Changed;
                    new WatchRef(accessor.Property).OnUpdate -= Updated;
                }
                _widget.Unmount -= Dispose;
            }

            private void Changed() => _changes.RegisterChange(_widget);

            private void Updated(float deltaTime) => Changed();
        }
    }
}
