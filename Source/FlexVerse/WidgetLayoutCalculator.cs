// parents: @.frostline/graph/m-widget-engine.xml

using System;
using System.Collections.Generic;
using FlexVerse.Flexbox;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    internal sealed class WidgetLayoutCalculator
    {
        private readonly WidgetTree _widgetTree;
        private readonly Dictionary<Widget, Rect> _layoutResults = [];
        private readonly Dictionary<Widget, Rect> _paddingBoxes = [];
        public IReadOnlyDictionary<Widget, Rect> PaddingBoxes => _paddingBoxes;
        public IReadOnlyDictionary<Widget, Rect> Results => _layoutResults;

        public WidgetLayoutCalculator(WidgetTree widgetTree)
        {
            _widgetTree = widgetTree;
        }

        public void Clear()
        {
            _layoutResults.Clear();
            _paddingBoxes.Clear();
        }

        public void CalculateLayout(Rect availableRect)
        {
            var rootNode = _widgetTree.GetRootNode();
            if (rootNode == null)
            {
                Log.Error("M-LAYOUT-TRANSFORMER", "layout.null_root");
                Clear();
                return;
            }

            try
            {
                Flex.CalculateLayout(
                    rootNode,
                    availableRect.width,
                    availableRect.height,
                    Directions.LTR
                );
                Log.Debug(
                    "M-LAYOUT-TRANSFORMER",
                    "layout.calculate.begin",
                    ("root", rootNode),
                    ("rect", availableRect)
                );

                Clear();
                StoreLayoutResultsRecursive(rootNode, availableRect.x, availableRect.y);
            }
            catch (Exception e)
            {
                Log.Error("M-LAYOUT-TRANSFORMER", "layout.exception", ("error", e));
                Clear();
            }
        }

        private void StoreLayoutResultsRecursive(
            Node node,
            float parentAbsoluteX,
            float parentAbsoluteY
        )
        {
            if (node.Context is Widget widget)
            {
                var rect = new Rect(
                    parentAbsoluteX + node.LayoutGetLeft(),
                    parentAbsoluteY + node.LayoutGetTop(),
                    node.LayoutGetWidth(),
                    node.LayoutGetHeight()
                );
                Log.Debug(
                    "M-LAYOUT-TRANSFORMER",
                    "layout.result.stored",
                    ("widget", widget),
                    ("rect", rect)
                );
                _layoutResults[widget] = rect;
                _paddingBoxes[widget] = PaddingBox(
                    rect.size,
                    node.LayoutGetBorder(Edge.Left),
                    node.LayoutGetBorder(Edge.Top),
                    node.LayoutGetBorder(Edge.Right),
                    node.LayoutGetBorder(Edge.Bottom)
                );
            }

            for (int i = 0; i < node.ChildrenCount; i++)
            {
                var childNode = node.GetChild(i);
                StoreLayoutResultsRecursive(
                    childNode,
                    parentAbsoluteX + node.LayoutGetLeft(),
                    parentAbsoluteY + node.LayoutGetTop()
                );
            }
        }

        private static Rect PaddingBox(
            Vector2 size,
            float left,
            float top,
            float right,
            float bottom
        )
        {
            left = NonNegative(left);
            top = NonNegative(top);
            right = NonNegative(right);
            bottom = NonNegative(bottom);

            return new Rect(
                left,
                top,
                Mathf.Max(0f, size.x - left - right),
                Mathf.Max(0f, size.y - top - bottom)
            );
        }

        private static float NonNegative(float value) =>
            float.IsNaN(value) || float.IsInfinity(value) || value < 0f ? 0f : value;
    }
}
