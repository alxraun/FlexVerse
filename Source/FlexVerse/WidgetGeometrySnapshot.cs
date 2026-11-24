// parents: @.frostline/graph/m-widget-geometry.xml
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Published geometry shared by rendering and input. Replaced as one unit.</summary>
    public sealed class WidgetGeometrySnapshot
    {
        private IReadOnlyDictionary<Widget, WidgetGeometry> _entries =
            new Dictionary<Widget, WidgetGeometry>();

        /// <summary>Gets the root widget of the published geometry.</summary>
        public Widget Root { get; private set; }

        /// <summary>Returns the geometry entry for a widget when published.</summary>
        public bool TryGet(Widget widget, out WidgetGeometry geometry) =>
            _entries.TryGetValue(widget, out geometry);

        internal void Publish(Widget root, IReadOnlyDictionary<Widget, WidgetGeometry> entries)
        {
            _entries = entries;
            Root = root;
        }
    }

    /// <summary>Holds one widget's published layout rect, matrices and clip data. (REQ-002)</summary>
    public sealed class WidgetGeometry
    {
        /// <summary>Gets the parent widget instance.</summary>
        public Widget Parent { get; internal init; }

        /// <summary>Gets the children in stable paint order.</summary>
        public IReadOnlyList<Widget> PaintChildren { get; internal init; }

        /// <summary>Gets the calculated layout rectangle.</summary>
        public Rect LayoutRect { get; internal init; }

        /// <summary>Gets the self transform matrix.</summary>
        public Matrix4x4 Self { get; internal init; }

        /// <summary>Gets the child-space transform matrix.</summary>
        public Matrix4x4 Children { get; internal init; }

        /// <summary>Gets the world transform matrix.</summary>
        public Matrix4x4 World { get; internal init; }

        /// <summary>Gets the inverse world matrix.</summary>
        public Matrix4x4 Inverse { get; internal init; }

        /// <summary>Reports whether the world matrix is invertible.</summary>
        public bool Invertible { get; internal init; }

        /// <summary>Reports whether the widget clips its children.</summary>
        public bool ClipsChildren { get; internal init; }

        /// <summary>Gets the local child clip rectangle.</summary>
        public Rect LocalClip { get; internal init; }
    }
}
