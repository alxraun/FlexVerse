// parents: @.frostline/graph/m-widget-engine.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>
    /// Traverses each parent's children in stable, ascending ZIndex paint
    /// order (declaration order for ties). The renderer owns IMGUI matrix scopes; widgets provide child-context hooks.
    /// </summary>
    internal sealed class WidgetRenderer
    {
        private const float CullMargin = 1f;
        private static readonly Rect Unbounded = new(-10000000f, -10000000f, 20000000f, 20000000f);

        private readonly WidgetGeometrySnapshot _snapshot;
        private readonly Action _refreshGeometry;
        private readonly FocusManager _focusManager;

        private readonly HashSet<Widget> _focusChain = new();

        public WidgetRenderer(
            WidgetGeometrySnapshot snapshot,
            Action refreshGeometry,
            FocusManager focusManager
        )
        {
            _snapshot = snapshot;
            _refreshGeometry = refreshGeometry;
            _focusManager = focusManager;
        }

        public void RenderInstanceTree()
        {
            if (_snapshot.Root == null)
                return;

            BuildFocusChain();
            RenderInstanceRecursive(_snapshot.Root, Vector2.zero, Unbounded);
        }

        private void BuildFocusChain()
        {
            _focusChain.Clear();
            var focused = _focusManager?.GetFocusedWidget();
            while (focused != null && _focusChain.Add(focused))
            {
                focused = _snapshot.TryGet(focused, out var geometry) ? geometry.Parent : null;
            }
        }

        private void RenderInstanceRecursive(
            Widget instance,
            Vector2 nativeOrigin,
            Rect visibleRect
        )
        {
            if (instance == null || !_snapshot.TryGet(instance, out var geometry))
                return;

            var layoutRect = geometry.LayoutRect;
            var guiRect = new Rect(layoutRect.position - nativeOrigin, layoutRect.size);

            void RenderChildren(Vector2 childNativeOffset, Rect childVisibleRect)
            {
                if (!instance.GetChildrenMatrix().Equals(geometry.Children))
                {
                    _refreshGeometry();
                }

                var childOrigin = nativeOrigin + childNativeOffset;
                var children = _snapshot.TryGet(instance, out var current)
                    ? current.PaintChildren
                    : Array.Empty<Widget>();
                for (var i = 0; i < children.Count; i++)
                {
                    var child = children[i];
                    if (child == null)
                        continue;
                    if (
                        _focusChain.Contains(child)
                        || !_snapshot.TryGet(child, out var childGeometry)
                    )
                    {
                        RenderInstanceRecursive(child, childOrigin, childVisibleRect);
                        continue;
                    }

                    var childGuiRect = new Rect(
                        childGeometry.LayoutRect.position - childOrigin,
                        childGeometry.LayoutRect.size
                    );
                    if (!OverlapsVisible(childGuiRect, childVisibleRect))
                        continue;

                    RenderInstanceRecursive(child, childOrigin, childVisibleRect);
                }
            }

            if (layoutRect.width <= 0.001f || layoutRect.height <= 0.001f)
            {
                if (!geometry.ClipsChildren)
                    RenderChildren(Vector2.zero, visibleRect);
                return;
            }

            try
            {
                RenderWithChildren(instance, guiRect, geometry.Self, visibleRect, RenderChildren);
            }
            catch (Exception e)
            {
                Log.Error(
                    "M-WIDGET-ENGINE",
                    "render.exception",
                    ("widget", instance.GetType().Name),
                    ("error", e)
                );
            }
        }

        private static bool OverlapsVisible(Rect rect, Rect visibleRect)
        {
            return rect.xMin - CullMargin < visibleRect.xMax
                && rect.xMax + CullMargin > visibleRect.xMin
                && rect.yMin - CullMargin < visibleRect.yMax
                && rect.yMax + CullMargin > visibleRect.yMin;
        }

        private static Rect ClipRect(Rect rect, Rect visibleRect)
        {
            if (rect.width <= 0.001f || rect.height <= 0.001f)
                return visibleRect;

            var xMin = Mathf.Max(rect.xMin, visibleRect.xMin);
            var yMin = Mathf.Max(rect.yMin, visibleRect.yMin);
            var xMax = Mathf.Min(rect.xMax, visibleRect.xMax);
            var yMax = Mathf.Min(rect.yMax, visibleRect.yMax);
            if (xMax <= xMin || yMax <= yMin)
                return Rect.zero;

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        /// <summary>
        /// Maps a visible region from the widget's own draw context into its
        /// children's draw context. Children are drawn through the child-space
        /// matrix (scroll/pan applied natively or via GUI.matrix), so the
        /// region must be transformed by the inverse matrix. The corner AABB
        /// is a superset of the true pre-image, which keeps the test
        /// conservative for rotated or scaled transforms.
        /// </summary>
        private static Rect MapVisibleToChildSpace(Rect visibleRect, Matrix4x4 childrenMatrix)
        {
            if (childrenMatrix.Equals(Matrix4x4.identity))
                return visibleRect;

            var determinant = childrenMatrix.determinant;
            if (float.IsNaN(determinant) || float.IsInfinity(determinant) || determinant == 0f)
            {
                return Unbounded;
            }

            var inverse = childrenMatrix.inverse;
            Vector3 p0 = inverse.MultiplyPoint3x4(
                new Vector3(visibleRect.xMin, visibleRect.yMin, 0f)
            );
            Vector3 p1 = inverse.MultiplyPoint3x4(
                new Vector3(visibleRect.xMax, visibleRect.yMin, 0f)
            );
            Vector3 p2 = inverse.MultiplyPoint3x4(
                new Vector3(visibleRect.xMin, visibleRect.yMax, 0f)
            );
            Vector3 p3 = inverse.MultiplyPoint3x4(
                new Vector3(visibleRect.xMax, visibleRect.yMax, 0f)
            );
            return Rect.MinMaxRect(
                Mathf.Min(Mathf.Min(p0.x, p1.x), Mathf.Min(p2.x, p3.x)),
                Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y)),
                Mathf.Max(Mathf.Max(p0.x, p1.x), Mathf.Max(p2.x, p3.x)),
                Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y))
            );
        }

        private void RenderWithChildren(
            Widget instance,
            Rect guiRect,
            Matrix4x4 selfTransform,
            Rect visibleRect,
            Action<Vector2, Rect> renderChildren
        )
        {
            var originalMatrix = GUI.matrix;
            var geometry = _snapshot.TryGet(instance, out var current) ? current : null;
            var viewport =
                geometry == null
                    ? Rect.zero
                    : new Rect(
                        guiRect.position + geometry.LocalClip.position,
                        geometry.LocalClip.size
                    );
            var childVisibleRect =
                geometry != null && geometry.ClipsChildren
                    ? ClipRect(viewport, visibleRect)
                    : visibleRect;
            var beganChildren = false;
            var clipOpened = false;

            try
            {
                GUI.matrix = originalMatrix * RebaseTransform(selfTransform, guiRect.position);
                instance.OnGUI(guiRect);
                var space = instance.BeginChildrenGUI(guiRect, viewport);
                beganChildren = true;
                var origin = space.Origin;

                if (geometry != null && geometry.ClipsChildren && !space.ClipsChildren)
                {
                    var clip = new Rect(viewport.position - origin, viewport.size);
                    GUI.BeginGroup(clip);
                    clipOpened = true;
                    origin += clip.position;
                }

                if (!space.AppliesChildrenTransform)
                {
                    var localOrigin = guiRect.position - origin;

                    GUI.matrix *= RebaseTransform(instance.GetChildrenMatrix(), localOrigin);
                }

                var regionOrigin = new Rect(
                    childVisibleRect.position - origin,
                    childVisibleRect.size
                );
                renderChildren(
                    origin,
                    MapVisibleToChildSpace(regionOrigin, instance.GetChildrenMatrix())
                );
            }
            finally
            {
                try
                {
                    try
                    {
                        if (clipOpened)
                            GUI.EndGroup();
                    }
                    finally
                    {
                        if (beganChildren)
                            instance.EndChildrenGUI();
                    }
                }
                finally
                {
                    GUI.matrix = originalMatrix;
                }
            }
        }

        private static Matrix4x4 RebaseTransform(Matrix4x4 transform, Vector2 origin)
        {
            if (transform.Equals(Matrix4x4.identity))
                return transform;

            var originalMatrix = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                origin += GUIUtility.GUIToScreenPoint(Vector2.zero);
            }
            finally
            {
                GUI.matrix = originalMatrix;
            }
            var offset = new Vector3(origin.x, origin.y, 0f);
            return Matrix4x4.Translate(offset) * transform * Matrix4x4.Translate(-offset);
        }
    }
}
