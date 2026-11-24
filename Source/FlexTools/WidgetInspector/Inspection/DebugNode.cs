// parents: @.frostline/graph/m-dbg-inspect.xml

using System;
using System.Collections.Generic;
using FlexTools.WidgetInspector.Models;
using FlexVerse;

namespace FlexTools.WidgetInspector.Inspection
{
    /// <summary>Immutable snapshot node describing one inspected widget.</summary>
    public sealed class DebugNode
    {
        private readonly WeakReference<Widget> _target;

        /// <summary>Creates a snapshot node for the given widget.</summary>
        public DebugNode(
            Widget widget,
            int instanceId,
            IReadOnlyList<DebugNode> children,
            IReadOnlyList<DebugPropertyNode> properties
        )
        {
            _target = new WeakReference<Widget>(widget);
            InstanceID = instanceId;
            TypeName = widget.GetType().Name;
            KeyDisplay = widget.Key?.ToString() ?? "";
            Children = children;
            Properties = properties;
        }

        /// <summary>Gets the stable instance identifier.</summary>
        public int InstanceID { get; }

        /// <summary>Gets the widget type name.</summary>
        public string TypeName { get; }

        /// <summary>Gets the display key or an empty string.</summary>
        public string KeyDisplay { get; }

        /// <summary>Gets the child snapshot nodes.</summary>
        public IReadOnlyList<DebugNode> Children { get; }

        /// <summary>Gets the property snapshot nodes.</summary>
        public IReadOnlyList<DebugPropertyNode> Properties { get; }

        /// <summary>Returns the live widget target, or null when collected.</summary>
        public Widget GetTarget() => _target.TryGetTarget(out var target) ? target : null;
    }
}
