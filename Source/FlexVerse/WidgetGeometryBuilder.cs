// parents: @.frostline/graph/m-widget-geometry.xml
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse
{
    internal sealed class WidgetGeometryBuilder(
        WidgetTree tree,
        IReadOnlyDictionary<Widget, Rect> layoutResults,
        IReadOnlyDictionary<Widget, Rect> paddingBoxes,
        WidgetGeometrySnapshot snapshot
    )
    {
        public void Refresh()
        {
            var entries = new Dictionary<Widget, WidgetGeometry>();
            var root = tree.GetRootInstance();
            if (root != null)
                Capture(root, null, Matrix4x4.identity, Vector2.zero, entries);
            snapshot.Publish(root, entries);
        }

        private void Capture(
            Widget widget,
            Widget parent,
            Matrix4x4 parentChildrenWorld,
            Vector2 parentPosition,
            Dictionary<Widget, WidgetGeometry> entries
        )
        {
            if (!layoutResults.TryGetValue(widget, out var rect))
                return;

            var geometry = CreateGeometry(
                rect,
                ComposeSelfTransform(
                    rect.size,
                    widget.TransformOrigin.Value,
                    widget.Translate.Value,
                    widget.Scale.Value,
                    widget.Rotate.Value
                ),
                widget.GetChildrenMatrix(),
                parentChildrenWorld,
                parentPosition,
                parent,
                System.Array.AsReadOnly(tree.GetChildrenInPaintOrder(widget).ToArray()),
                widget.Overflow.Value != Overflows.Visible,
                paddingBoxes.TryGetValue(widget, out var clip) ? clip : Rect.zero
            );
            entries.Add(widget, geometry);
            var childrenWorld = geometry.World * geometry.Children;
            foreach (var child in tree.GetChildInstances(widget))
            {
                if (child != null)
                    Capture(child, widget, childrenWorld, rect.position, entries);
            }
        }

        private static WidgetGeometry CreateGeometry(
            Rect layoutRect,
            Matrix4x4 self,
            Matrix4x4 children,
            Matrix4x4 parentChildrenWorld,
            Vector2 parentPosition,
            Widget parent,
            IReadOnlyList<Widget> paintChildren,
            bool clipsChildren,
            Rect localClip
        )
        {
            var localPosition = layoutRect.position - parentPosition;
            var world =
                parentChildrenWorld
                * Matrix4x4.Translate(new Vector3(localPosition.x, localPosition.y, 0f))
                * self;
            var invertible = TryInvert(world, out var inverse);

            return new WidgetGeometry
            {
                Parent = parent,
                PaintChildren = paintChildren,
                ClipsChildren = clipsChildren,
                LocalClip = localClip,
                LayoutRect = layoutRect,
                Self = self,
                Children = children,
                World = world,
                Inverse = inverse,
                Invertible = invertible,
            };
        }

        private static Matrix4x4 ComposeSelfTransform(
            Vector2 size,
            Vector2 origin,
            Vector2 translation,
            Vector2 scale,
            float rotation
        )
        {
            var pivot = new Vector3(size.x * origin.x, size.y * origin.y, 0f);
            return Matrix4x4.Translate(new Vector3(translation.x, translation.y, 0f))
                * Matrix4x4.Translate(pivot)
                * Matrix4x4.Rotate(Quaternion.Euler(0f, 0f, rotation))
                * Matrix4x4.Scale(new Vector3(scale.x, scale.y, 1f))
                * Matrix4x4.Translate(-pivot);
        }

        private static bool TryInvert(Matrix4x4 matrix, out Matrix4x4 inverse)
        {
            var determinant = matrix.determinant;
            var invertible =
                !float.IsNaN(determinant) && !float.IsInfinity(determinant) && determinant != 0f;
            inverse = invertible ? matrix.inverse : Matrix4x4.identity;
            return invertible;
        }
    }
}
