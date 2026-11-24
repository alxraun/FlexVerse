// parents: @.frostline/graph/m-scroll-view-widget.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse
{
    public partial class ScrollView : Widget
    {
        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<bool> ScrollHorizontal { get; set; } = false;

        [Prop]
        public Behavior<bool> ScrollVertical { get; set; } = true;

        [Prop]
        public Behavior<Action<Vector2>> OnScroll { get; set; }

        [Prop]
        public Behavior<Color> BackgroundColor { get; set; }

        [Prop]
        public Behavior<Color> Color { get; set; }

        [State]
        protected Behavior<Vector2> ScrollPosition { get; set; } = Vector2.zero;

        protected Rect _cachedScrollViewViewport;
        protected Rect _cachedContentViewRect;
        private bool _cachesAreValid = false;

        public ScrollView()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            FlexGrow = 1;
            FlexShrink = 0;
            FlexBasis = 0.Point();
            Overflow = Overflows.Scroll;
        }

        protected override Matrix4x4 GetChildrenTransform()
        {
            var scroll = ScrollPosition.Value;
            return Matrix4x4.Translate(new Vector3(-scroll.x, -scroll.y, 0f));
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return (Children.Value != default) ? Children.Value : Enumerable.Empty<Widget>();
        }

        public override void OnGUI(Rect calculatedRect)
        {
            DrawBackgroundAndBorder(calculatedRect);

            if (!string.IsNullOrEmpty(Tooltip.Value))
            {
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
            }
        }

        public override ChildrenGuiSpace BeginChildrenGUI(Rect calculatedRect, Rect viewport)
        {
            if (
                Engine.DidLayoutCalculationHappenThisCycle
                || !_cachesAreValid
                || viewport != _cachedScrollViewViewport
            )
            {
                _cachedScrollViewViewport = viewport;
                _cachedContentViewRect = CalculateContentRect(
                    viewport,
                    Engine.GetLayoutResults(),
                    calculatedRect
                );
                _cachesAreValid = true;
            }

            Rect currentViewport = _cachedScrollViewViewport;
            Rect currentContentViewRect = _cachedContentViewRect;

            Vector2 oldScrollPosition = ScrollPosition.Value;

            if (ScrollHorizontal.Value)
            {
                Vector2 newScrollPosition = oldScrollPosition;
                Widgets.ScrollHorizontal(
                    currentViewport,
                    ref newScrollPosition,
                    currentContentViewRect
                );
                ScrollPosition = newScrollPosition;
            }

            Vector2 beginScrollPosition = ScrollPosition.Value;
            Widgets.BeginScrollView(
                currentViewport,
                ref beginScrollPosition,
                currentContentViewRect,
                false
            );
            try
            {
                ScrollPosition = beginScrollPosition;

                if (ScrollPosition.Value != oldScrollPosition)
                    OnScroll.Value?.Invoke(ScrollPosition.Value);

                return new ChildrenGuiSpace(
                    currentViewport.position,
                    appliesChildrenTransform: true,
                    clipsChildren: true
                );
            }
            catch
            {
                Widgets.EndScrollView();
                throw;
            }
        }

        public override void EndChildrenGUI()
        {
            Widgets.EndScrollView();
        }

        private void DrawBackgroundAndBorder(Rect calculatedRect)
        {
            if (BackgroundColor.Value != default)
            {
                Widgets.DrawBoxSolid(calculatedRect, BackgroundColor.Value);
            }

            if (
                BorderAll.Value != null
                && BorderAll.Value.unit == Unit.Point
                && BorderAll.Value.value > 0
            )
            {
                Color borderColor =
                    (Color.Value != default) ? Color.Value : UnityEngine.Color.white;
                var originalColor = GUI.color;
                GUI.color = borderColor;
                Widgets.DrawBox(calculatedRect, (int)BorderAll.Value.value);
                GUI.color = originalColor;
            }
        }

        protected Rect CalculateContentRect(
            Rect scrollViewViewport,
            IReadOnlyDictionary<Widget, Rect> layoutResults,
            Rect calculatedRect
        )
        {
            float maxContentWidth = 0;
            float maxContentHeight = 0;

            var absoluteRect = layoutResults[this];
            var absoluteViewportOrigin =
                absoluteRect.position + scrollViewViewport.position - calculatedRect.position;

            var children = Engine.GetChildren(this);
            for (int i = 0; i < children.Count; i++)
            {
                var childInstance = children[i];
                if (
                    childInstance != null
                    && layoutResults.TryGetValue(childInstance, out Rect childGlobalRect)
                )
                {
                    float childRelativeXMin = childGlobalRect.xMin - absoluteViewportOrigin.x;
                    float childRelativeYMin = childGlobalRect.yMin - absoluteViewportOrigin.y;

                    float childExtentRight = childRelativeXMin + childGlobalRect.width;
                    float childExtentBottom = childRelativeYMin + childGlobalRect.height;

                    if (childExtentRight > maxContentWidth)
                        maxContentWidth = childExtentRight;
                    if (childExtentBottom > maxContentHeight)
                        maxContentHeight = childExtentBottom;
                }
            }

            var node = Engine.GetNode(this);
            if (ScrollHorizontal.Value)
                maxContentWidth = Mathf.Max(
                    scrollViewViewport.width,
                    maxContentWidth + node.LayoutGetPadding(Edge.Right)
                );
            else
                maxContentWidth = scrollViewViewport.width;

            if (ScrollVertical.Value)
                maxContentHeight = Mathf.Max(
                    scrollViewViewport.height,
                    maxContentHeight + node.LayoutGetPadding(Edge.Bottom)
                );
            else
                maxContentHeight = scrollViewViewport.height;

            return new Rect(0, 0, maxContentWidth, maxContentHeight);
        }
    }
}
