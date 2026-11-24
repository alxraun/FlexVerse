// parents: @.frostline/graph/m-transform-service.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Answers read-only coordinate conversions over published geometry. (M-TRANSFORM-SERVICE)</summary>
    public sealed class TransformService
    {
        private readonly WidgetGeometrySnapshot _snapshot;

        /// <summary>Binds coordinate queries to a geometry snapshot.</summary>
        public TransformService(WidgetGeometrySnapshot snapshot) => _snapshot = snapshot;

        /// <summary>Returns the world matrix of a widget. (REQ-001)</summary>
        public Matrix4x4 GetWorldMatrix(Widget widget) =>
            _snapshot.TryGet(widget, out var geometry) ? geometry.World : Matrix4x4.identity;

        internal Matrix4x4 GetSelfMatrix(Widget widget) =>
            _snapshot.TryGet(widget, out var geometry) ? geometry.Self : Matrix4x4.identity;

        internal Matrix4x4 GetChildrenMatrix(Widget widget) =>
            _snapshot.TryGet(widget, out var geometry) ? geometry.Children : Matrix4x4.identity;

        /// <summary>Projects a local point into screen coordinates.</summary>
        public Vector2 ToScreen(Widget widget, Vector2 localPoint)
        {
            if (!_snapshot.TryGet(widget, out var geometry))
                return Vector2.zero;

            return Project(geometry.World, localPoint);
        }

        /// <summary>Returns the axis-aligned screen bounds of the transformed rectangle.</summary>
        public Rect ToScreen(Widget widget)
        {
            if (!_snapshot.TryGet(widget, out var geometry))
                return Rect.zero;

            return ProjectBounds(geometry.World, geometry.LayoutRect.size);
        }

        /// <summary>Maps a screen point into widget-local coordinates. (REQ-002)</summary>
        public bool TryToLocal(Widget widget, Vector2 screenPoint, out Vector2 localPoint)
        {
            if (!_snapshot.TryGet(widget, out var geometry) || !geometry.Invertible)
            {
                localPoint = Vector2.zero;
                return false;
            }

            localPoint = Project(geometry.Inverse, screenPoint);
            return true;
        }

        /// <summary>Maps a screen point into widget-local coordinates.</summary>
        public Vector2 ToLocal(Widget widget, Vector2 screenPoint) =>
            TryToLocal(widget, screenPoint, out var point) ? point : Vector2.zero;

        private static Rect ProjectBounds(Matrix4x4 matrix, Vector2 size)
        {
            var a = Project(matrix, Vector2.zero);
            var b = Project(matrix, new Vector2(size.x, 0f));
            var c = Project(matrix, new Vector2(0f, size.y));
            var d = Project(matrix, size);
            var xmin = Mathf.Min(Mathf.Min(a.x, b.x), Mathf.Min(c.x, d.x));
            var ymin = Mathf.Min(Mathf.Min(a.y, b.y), Mathf.Min(c.y, d.y));
            var xmax = Mathf.Max(Mathf.Max(a.x, b.x), Mathf.Max(c.x, d.x));
            var ymax = Mathf.Max(Mathf.Max(a.y, b.y), Mathf.Max(c.y, d.y));
            return Rect.MinMaxRect(xmin, ymin, xmax, ymax);
        }

        private static Vector2 Project(Matrix4x4 matrix, Vector2 point)
        {
            var result = matrix.MultiplyPoint3x4(new Vector3(point.x, point.y, 0f));
            return new Vector2(result.x, result.y);
        }
    }
}
