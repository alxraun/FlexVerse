// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;
using Verse;

namespace FlexVerse
{
    internal static class BorderDrawer
    {
        internal static void Draw(Widget owner, Rect bounds, Color color, CornerRadii radii)
        {
            var edges = GetBorder(owner);
            if (!edges.Any || bounds.width <= 0f || bounds.height <= 0f || color.a <= 0f)
                return;

            if (radii.IsSquare)
            {
                float top = Mathf.Min(bounds.height, edges.T);
                float bottom = Mathf.Min(Mathf.Max(0f, bounds.height - top), edges.B);
                float left = Mathf.Min(bounds.width, edges.L);
                float right = Mathf.Min(Mathf.Max(0f, bounds.width - left), edges.R);
                float middleHeight = Mathf.Max(0f, bounds.height - top - bottom);

                if (top > 0f)
                    Widgets.DrawBoxSolid(new Rect(bounds.x, bounds.y, bounds.width, top), color);
                if (bottom > 0f)
                    Widgets.DrawBoxSolid(
                        new Rect(bounds.x, bounds.yMax - bottom, bounds.width, bottom),
                        color
                    );
                if (middleHeight > 0f && left > 0f)
                    Widgets.DrawBoxSolid(
                        new Rect(bounds.x, bounds.y + top, left, middleHeight),
                        color
                    );
                if (middleHeight > 0f && right > 0f)
                    Widgets.DrawBoxSolid(
                        new Rect(bounds.xMax - right, bounds.y + top, right, middleHeight),
                        color
                    );
            }
            else
            {
                ImageDrawer.DrawTint(
                    bounds,
                    AppearanceMasks.GetBorder(bounds, radii, edges),
                    color
                );
            }
        }

        internal static BorderEdges GetBorder(Widget widget)
        {
            var node = widget.Engine?.GetNode(widget);
            if (node == null)
                return default;
            return new BorderEdges(
                Positive(node.LayoutGetBorder(Edge.Left)),
                Positive(node.LayoutGetBorder(Edge.Top)),
                Positive(node.LayoutGetBorder(Edge.Right)),
                Positive(node.LayoutGetBorder(Edge.Bottom))
            );
        }

        private static float Positive(float value) =>
            float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? 0f : value;
    }
}
