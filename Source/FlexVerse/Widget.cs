// parents: @.frostline/graph/m-widget.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Declares the base declarative widget with reactive props and native hooks. (M-WIDGET)</summary>
    public abstract partial class Widget
    {
        /// <summary>Describes whether a widget is a definition or an instance.</summary>
        public enum Lifecycle
        {
            Definition,
            Instance,
        }

        protected internal WidgetEngine Engine { get; internal set; } = null!;

        /// <summary>Reports the current lifecycle position of the widget.</summary>
        public Lifecycle LifecycleState { get; internal set; } = Lifecycle.Definition;

        /// <summary>Gets or sets the reconciliation key of the widget.</summary>
        public virtual object Key { get; set; }

        /// <summary>Opens an optional native child context and reports its coordinate space. (REQ-004)</summary>
        public virtual ChildrenGuiSpace BeginChildrenGUI(Rect calculatedRect, Rect viewport) =>
            default;

        /// <summary>Closes the native child context opened by BeginChildrenGUI.</summary>
        public virtual void EndChildrenGUI() { }

        protected virtual MeasureFunc MeasureFunc => null;

        protected virtual BaselineFunc BaselineFunc => null;

        /// <summary>Supplies an additional transform of the content coordinate space without affecting layout.</summary>
        protected virtual Matrix4x4 GetChildrenTransform() => Matrix4x4.identity;

        internal Matrix4x4 GetChildrenMatrix() => GetChildrenTransform();

        /// <summary>Declares the child widgets of this widget.</summary>
        [Prop(ReconcileOnSet = true, ReconcileOnUpdate = true)]
        public Behavior<WidgetList> Children { get; set; } = new WidgetList();

        /// <summary>Sets the paint priority among sibling widgets without affecting layout. (REQ-007)</summary>
        [Prop]
        public Behavior<int> ZIndex { get; set; } = 0;

        /// <summary>Translates the widget surface for rendering and hit testing. (REQ-006)</summary>
        [Prop]
        public Behavior<Vector2> Translate { get; set; } = Vector2.zero;

        /// <summary>Scales the widget surface for rendering and hit testing.</summary>
        [Prop]
        public Behavior<Vector2> Scale { get; set; } = Vector2.one;

        /// <summary>Rotates the widget surface for rendering and hit testing.</summary>
        [Prop]
        public Behavior<float> Rotate { get; set; } = 0f;

        /// <summary>Sets the pivot used by the self transforms.</summary>
        [Prop]
        public Behavior<Vector2> TransformOrigin { get; set; } = new Vector2(0.5f, 0.5f);

        /// <summary>Sets the inherited text layout direction.</summary>
        [Prop]
        public Behavior<Directions> Direction { get; set; } = Directions.Inherit;

        /// <summary>Sets the main-axis direction of the flex container.</summary>
        [Prop]
        public Behavior<FlexDirections> FlexDirection { get; set; } = FlexDirections.Row;

        /// <summary>Sets the default cross-axis alignment of flex children.</summary>
        [Prop]
        public Behavior<Align> AlignItems { get; set; } = Align.Stretch;

        /// <summary>Overrides the cross-axis alignment of this widget.</summary>
        [Prop]
        public Behavior<Align> AlignSelf { get; set; } = Align.Auto;

        /// <summary>Sets the cross-axis alignment of wrapped lines.</summary>
        [Prop]
        public Behavior<Align> AlignContent { get; set; } = Align.Stretch;

        /// <summary>Sets the main-axis distribution of flex children.</summary>
        [Prop]
        public Behavior<Justify> JustifyContent { get; set; } = Justify.FlexStart;

        /// <summary>Sets the line wrapping behavior of flex children.</summary>
        [Prop]
        public Behavior<Wrap> FlexWrap { get; set; } = Wrap.NoWrap;

        /// <summary>Sets how content overflowing the widget is handled.</summary>
        [Prop]
        public Behavior<Overflows> Overflow { get; set; } = Overflows.Visible;

        /// <summary>Sets whether the widget is positioned relatively or absolutely.</summary>
        [Prop]
        public Behavior<PositionTypes> PositionType { get; set; } = PositionTypes.Relative;

        /// <summary>Sets whether the widget participates in flex layout.</summary>
        [Prop]
        public Behavior<Displays> Display { get; set; } = Displays.Flex;

        /// <summary>Sets the flex grow factor of the widget.</summary>
        [Prop]
        public Behavior<float> FlexGrow { get; set; } = 0;

        /// <summary>Sets the flex shrink factor of the widget.</summary>
        [Prop]
        public Behavior<float> FlexShrink { get; set; } = 1;

        /// <summary>Sets the flex basis of the widget.</summary>
        [Prop]
        public Behavior<Length> FlexBasis { get; set; } = Length.Auto;

        /// <summary>Sets the width of the widget.</summary>
        [Prop]
        public Behavior<Length> Width { get; set; } = Length.Auto;

        /// <summary>Sets the height of the widget.</summary>
        [Prop]
        public Behavior<Length> Height { get; set; } = Length.Auto;

        /// <summary>Sets the minimum width of the widget.</summary>
        [Prop]
        public Behavior<Length> MinWidth { get; set; } = Length.Undefined;

        /// <summary>Sets the minimum height of the widget.</summary>
        [Prop]
        public Behavior<Length> MinHeight { get; set; } = Length.Undefined;

        /// <summary>Sets the maximum width of the widget.</summary>
        [Prop]
        public Behavior<Length> MaxWidth { get; set; } = Length.Undefined;

        /// <summary>Sets the maximum height of the widget.</summary>
        [Prop]
        public Behavior<Length> MaxHeight { get; set; } = Length.Undefined;

        /// <summary>Sets the width-to-height aspect ratio of the widget.</summary>
        [Prop]
        public Behavior<float> AspectRatio { get; set; } = float.NaN;

        /// <summary>Sets the margin on all edges of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginAll { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the left edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginLeft { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the top edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginTop { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the right edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginRight { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the bottom edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginBottom { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the start edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginStart { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the end edge of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginEnd { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the horizontal edges of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginHorizontal { get; set; } = Length.Undefined;

        /// <summary>Sets the margin on the vertical edges of the widget.</summary>
        [Prop]
        public Behavior<Length> MarginVertical { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on all edges of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingAll { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the left edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingLeft { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the top edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingTop { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the right edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingRight { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the bottom edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingBottom { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the start edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingStart { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the end edge of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingEnd { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the horizontal edges of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingHorizontal { get; set; } = Length.Undefined;

        /// <summary>Sets the padding on the vertical edges of the widget.</summary>
        [Prop]
        public Behavior<Length> PaddingVertical { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on all edges of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderAll { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the left edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderLeft { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the top edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderTop { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the right edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderRight { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the bottom edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderBottom { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the start edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderStart { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the end edge of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderEnd { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the horizontal edges of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderHorizontal { get; set; } = Length.Undefined;

        /// <summary>Sets the border width on the vertical edges of the widget.</summary>
        [Prop]
        public Behavior<Length> BorderVertical { get; set; } = Length.Undefined;

        /// <summary>Sets the left position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionLeft { get; set; } = Length.Undefined;

        /// <summary>Sets the top position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionTop { get; set; } = Length.Undefined;

        /// <summary>Sets the right position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionRight { get; set; } = Length.Undefined;

        /// <summary>Sets the bottom position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionBottom { get; set; } = Length.Undefined;

        /// <summary>Sets the start position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionStart { get; set; } = Length.Undefined;

        /// <summary>Sets the end position offset of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionEnd { get; set; } = Length.Undefined;

        /// <summary>Sets the horizontal position offsets of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionHorizontal { get; set; } = Length.Undefined;

        /// <summary>Sets the vertical position offsets of the widget.</summary>
        [Prop]
        public Behavior<Length> PositionVertical { get; set; } = Length.Undefined;

        protected void SignalReconcileChildren()
        {
            Engine.Post(new SignalInfo(this, () => Engine.ReconcileChildren(this)));
        }

        /// <summary>Returns the property ref registered under the given key. (REQ-003)</summary>
        public IPropertyRef GetProperty(string key)
        {
            return Refs[key];
        }

        protected void SignalMarkNodeAsDirty()
        {
            Engine.Post(new SignalInfo(this, () => Engine.MarkNodeAsDirty(this)));
        }

        internal MeasureFunc GetMeasureFunc() => MeasureFunc;

        internal BaselineFunc GetBaselineFunc() => BaselineFunc;

        /// <summary>Handles a native mouse-down event bubbling to this widget.</summary>
        public virtual void OnMouseDown(FlexEvent e) { }

        /// <summary>Handles a native mouse-up event bubbling to this widget.</summary>
        public virtual void OnMouseUp(FlexEvent e) { }

        /// <summary>Handles a native mouse-drag event bubbling to this widget.</summary>
        public virtual void OnMouseDrag(FlexEvent e) { }

        /// <summary>Handles a synthesized mouse-move event bubbling to this widget.</summary>
        public virtual void OnMouseMove(FlexEvent e) { }

        /// <summary>Handles hover entry into this widget.</summary>
        public virtual void OnMouseEnter(FlexEvent e) { }

        /// <summary>Handles hover exit from this widget.</summary>
        public virtual void OnMouseLeave(FlexEvent e) { }

        /// <summary>Handles a scroll-wheel event bubbling to this widget.</summary>
        public virtual void OnScrollWheel(FlexEvent e) { }

        /// <summary>Handles a key-down event while this widget holds focus. (REQ-003)</summary>
        public virtual void OnKeyDown(FlexEvent e) { }

        /// <summary>Handles a key-up event while this widget holds focus.</summary>
        public virtual void OnKeyUp(FlexEvent e) { }

        /// <summary>Runs when the widget instance mounts into the tree.</summary>
        public virtual void OnMount() { }

        /// <summary>Runs when the widget instance unmounts from the tree.</summary>
        public virtual void OnUnmount() { }

        /// <summary>Raises after the widget instance mounts.</summary>
        public event Action Mount;

        /// <summary>Raises before the widget instance unmounts.</summary>
        public event Action Unmount;

        internal void MountInvoke() => Mount?.Invoke();

        internal void UnmountInvoke() => Unmount?.Invoke();

        /// <summary>Yields the child widget definitions for reconciliation.</summary>
        public virtual IEnumerable<Widget> BuildRenderTree()
        {
            yield break;
        }

        /// <summary>Draws the widget surface inside the calculated layout rectangle. (REQ-002)</summary>
        public virtual void OnGUI(Rect calculatedRect) { }

        /// <summary>Returns a short debug identifier for the widget.</summary>
        public override string ToString()
        {
            var type = GetType().Name;
            var hash = GetHashCode() & 0xFFFFF;

            if (Key != null)
            {
                var keyStr = Key.ToString();
                if (!string.IsNullOrEmpty(keyStr))
                {
                    return $"{type}:{keyStr}:{hash:X5}";
                }
            }

            return $"{type}:{hash:X5}";
        }
    }
}
