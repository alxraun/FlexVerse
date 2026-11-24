// parents: @.frostline/graph/m-dbg-overlay.xml

using FlexTools.WidgetInspector.Models;
using FlexVerse;
using FlexVerse.Flexbox;
using UnityEngine;

namespace FlexTools.WidgetInspector.Overlays
{
    /// <summary>Draws margin, border, padding, and content bands for a widget.</summary>
    public class BoxModelOverlay
    {
        private static readonly Color MarginColor = new Color(0.68f, 0.51f, 0.33f, 0.4f);
        private static readonly Color BorderColor = new Color(0.89f, 0.76f, 0.51f, 0.4f);
        private static readonly Color PaddingColor = new Color(0.72f, 0.77f, 0.5f, 0.4f);
        private static readonly Color ContentColor = new Color(0.53f, 0.7f, 0.74f, 0.4f);

        /// <summary>Draws the box model bands for the selected widget.</summary>
        public void Draw(WidgetEngine engine, Widget widget)
        {
            if (!engine.Geometry.TryGet(widget, out var geometry))
                return;
            var node = engine.GetNode(widget);
            if (node == null)
                return;
            var border = new Rect(Vector2.zero, geometry.LayoutRect.size);
            var padding = Inset(border, node.LayoutGetBorder);
            var content = Inset(padding, node.LayoutGetPadding);
            var margin = new Rect(
                -node.LayoutGetMargin(Edge.Left),
                -node.LayoutGetMargin(Edge.Top),
                border.width + node.LayoutGetMargin(Edge.Left) + node.LayoutGetMargin(Edge.Right),
                border.height + node.LayoutGetMargin(Edge.Top) + node.LayoutGetMargin(Edge.Bottom)
            );
            var box = new DebugBoxModel
            {
                Margin = margin,
                Border = border,
                Padding = padding,
                Content = content,
            };
            OverlayScope.Draw(engine, widget, () => Draw(box));
        }

        private static Rect Inset(Rect rect, System.Func<Edge, float> edge) =>
            new Rect(
                rect.x + edge(Edge.Left),
                rect.y + edge(Edge.Top),
                Mathf.Max(0f, rect.width - edge(Edge.Left) - edge(Edge.Right)),
                Mathf.Max(0f, rect.height - edge(Edge.Top) - edge(Edge.Bottom))
            );

        private void Draw(DebugBoxModel box)
        {
            DrawRect(box.Margin, box.Border, MarginColor);
            DrawRect(box.Border, box.Padding, BorderColor);
            DrawRect(box.Padding, box.Content, PaddingColor);
            Verse.Widgets.DrawBoxSolid(box.Content, ContentColor);
        }

        private void DrawRect(Rect outer, Rect inner, Color color)
        {
            if (inner.y > outer.y)
                Verse.Widgets.DrawBoxSolid(
                    new Rect(outer.x, outer.y, outer.width, inner.y - outer.y),
                    color
                );

            if (outer.yMax > inner.yMax)
                Verse.Widgets.DrawBoxSolid(
                    new Rect(outer.x, inner.yMax, outer.width, outer.yMax - inner.yMax),
                    color
                );

            if (inner.x > outer.x)
                Verse.Widgets.DrawBoxSolid(
                    new Rect(outer.x, inner.y, inner.x - outer.x, inner.height),
                    color
                );

            if (outer.xMax > inner.xMax)
                Verse.Widgets.DrawBoxSolid(
                    new Rect(inner.xMax, inner.y, outer.xMax - inner.xMax, inner.height),
                    color
                );
        }
    }
}
