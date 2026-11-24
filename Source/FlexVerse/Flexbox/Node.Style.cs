// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using FlexVerse.Flexbox;

namespace FlexVerse
{
    /// <summary>Implements style getters, setters and defaults for layout nodes. (M-FLEXBOX)</summary>
    public partial class Node
    {
        internal void Helper_SetDimensions(Length value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Auto)
                    StyleSetWidthAuto();
                else if (value.unit == Unit.Percent)
                    StyleSetWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetWidth(value.value);
            }
            else
            {
                if (value.unit == Unit.Auto)
                    StyleSetHeightAuto();
                else if (value.unit == Unit.Percent)
                    StyleSetHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetHeight(value.value);
            }
        }

        internal void Helper_SetMinDimensions(Length value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Percent)
                    StyleSetMinWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMinWidth(value.value);
                else
                    StyleSetMinWidth(float.NaN);
            }
            else
            {
                if (value.unit == Unit.Percent)
                    StyleSetMinHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMinHeight(value.value);
                else
                    StyleSetMinHeight(float.NaN);
            }
        }

        internal void Helper_SetMaxDimensions(Length value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Percent)
                    StyleSetMaxWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMaxWidth(value.value);
                else
                    StyleSetMaxWidth(float.NaN);
            }
            else
            {
                if (value.unit == Unit.Percent)
                    StyleSetMaxHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMaxHeight(value.value);
                else
                    StyleSetMaxHeight(float.NaN);
            }
        }

        internal void Helper_SetMarginPaddingBorder(string tag, Edge edge, Length value)
        {
            if (tag == "margin")
            {
                if (value.unit == Unit.Auto)
                    StyleSetMarginAuto(edge);
                else if (value.unit == Unit.Percent)
                    StyleSetMarginPercent(edge, value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMargin(edge, value.value);
                else
                    StyleSetMargin(edge, float.NaN);
            }
            else if (tag == "padding")
            {
                if (value.unit == Unit.Percent)
                    StyleSetPaddingPercent(edge, value.value);
                else if (value.unit == Unit.Point)
                    StyleSetPadding(edge, value.value);
                else
                    StyleSetPadding(edge, float.NaN);
            }
            else if (tag == "border")
            {
                if (value.unit == Unit.Point)
                    StyleSetBorder(edge, value.value);
                else
                    StyleSetBorder(edge, float.NaN);
            }
        }

        /// <summary>Sets the node's width in points.</summary>
        public void StyleSetWidth(float width)
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Width];
            if (dim.value != width || dim.unit != Unit.Point)
            {
                nodeStyle.Dimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(width)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(width, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's width as a percentage of the parent.</summary>
        public void StyleSetWidthPercent(float width)
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Width];
            if (dim.value != width || dim.unit != Unit.Percent)
            {
                nodeStyle.Dimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(width)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(width, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's width to resolve automatically.</summary>
        public void StyleSetWidthAuto()
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Width];
            if (dim.unit != Unit.Auto)
            {
                nodeStyle.Dimensions[(int)Dimension.Width] = new Length(float.NaN, Unit.Auto);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured width.</summary>
        public Length StyleGetWidth()
        {
            return nodeStyle.Dimensions[(int)Dimension.Width];
        }

        /// <summary>Sets the node's height in points.</summary>
        public void StyleSetHeight(float height)
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Height];
            if (dim.value != height || dim.unit != Unit.Point)
            {
                nodeStyle.Dimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(height)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(height, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's height as a percentage of the parent.</summary>
        public void StyleSetHeightPercent(float height)
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Height];
            if (dim.value != height || dim.unit != Unit.Percent)
            {
                nodeStyle.Dimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(height)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(height, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's height to resolve automatically.</summary>
        public void StyleSetHeightAuto()
        {
            var dim = nodeStyle.Dimensions[(int)Dimension.Height];
            if (dim.unit != Unit.Auto)
            {
                nodeStyle.Dimensions[(int)Dimension.Height] = new Length(float.NaN, Unit.Auto);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured height.</summary>
        public Length StyleGetHeight()
        {
            return nodeStyle.Dimensions[(int)Dimension.Height];
        }

        /// <summary>Sets how the node is positioned within its parent.</summary>
        public void StyleSetPositionType(PositionTypes positionType)
        {
            if (nodeStyle.PositionType != positionType)
            {
                nodeStyle.PositionType = positionType;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's position type.</summary>
        public PositionTypes StyleGetPositionType()
        {
            return nodeStyle.PositionType;
        }

        /// <summary>Sets the node's position offset for the given edge in points.</summary>
        public void StyleSetPosition(Edge edge, float position)
        {
            var pos = nodeStyle.Position[(int)edge];
            if (pos.value != position || pos.unit != Unit.Point)
            {
                nodeStyle.Position[(int)edge] = Flex.FloatIsUndefined(position)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(position, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's position offset for the given edge as a percentage.</summary>
        public void StyleSetPositionPercent(Edge edge, float position)
        {
            var pos = nodeStyle.Position[(int)edge];
            if (pos.value != position || pos.unit != Unit.Percent)
            {
                nodeStyle.Position[(int)edge] = Flex.FloatIsUndefined(position)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(position, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured position offset for the given edge.</summary>
        public Length StyleGetPosition(Edge edge)
        {
            return nodeStyle.Position[(int)edge];
        }

        /// <summary>Sets the node's text layout direction.</summary>
        public void StyleSetDirection(Directions direction)
        {
            if (nodeStyle.Direction != direction)
            {
                nodeStyle.Direction = direction;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's text layout direction.</summary>
        public Directions StyleGetDirection()
        {
            return nodeStyle.Direction;
        }

        /// <summary>Sets the node's main-axis flex direction.</summary>
        public void StyleSetFlexDirection(FlexDirections flexDirection)
        {
            if (nodeStyle.FlexDirection != flexDirection)
            {
                nodeStyle.FlexDirection = flexDirection;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's main-axis flex direction.</summary>
        public FlexDirections StyleGetFlexDirection()
        {
            return nodeStyle.FlexDirection;
        }

        /// <summary>Sets the main-axis distribution of the node's children.</summary>
        public void StyleSetJustifyContent(Justify justifyContent)
        {
            if (nodeStyle.JustifyContent != justifyContent)
            {
                nodeStyle.JustifyContent = justifyContent;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's main-axis content distribution.</summary>
        public Justify StyleGetJustifyContent()
        {
            return nodeStyle.JustifyContent;
        }

        /// <summary>Sets the cross-axis alignment of wrapped lines.</summary>
        public void StyleSetAlignContent(Align alignContent)
        {
            if (nodeStyle.AlignContent != alignContent)
            {
                nodeStyle.AlignContent = alignContent;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the cross-axis alignment of wrapped lines.</summary>
        public Align StyleGetAlignContent()
        {
            return nodeStyle.AlignContent;
        }

        /// <summary>Sets the default cross-axis alignment of the node's children.</summary>
        public void StyleSetAlignItems(Align alignItems)
        {
            if (nodeStyle.AlignItems != alignItems)
            {
                nodeStyle.AlignItems = alignItems;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the default cross-axis alignment of children.</summary>
        public Align StyleGetAlignItems()
        {
            return nodeStyle.AlignItems;
        }

        /// <summary>Sets how this node aligns itself along the cross axis.</summary>
        public void StyleSetAlignSelf(Align alignSelf)
        {
            if (nodeStyle.AlignSelf != alignSelf)
            {
                nodeStyle.AlignSelf = alignSelf;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's own cross-axis alignment.</summary>
        public Align StyleGetAlignSelf()
        {
            return nodeStyle.AlignSelf;
        }

        /// <summary>Sets the node's child wrapping behavior.</summary>
        public void StyleSetFlexWrap(Wrap flexWrap)
        {
            if (nodeStyle.FlexWrap != flexWrap)
            {
                nodeStyle.FlexWrap = flexWrap;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's child wrapping behavior.</summary>
        public Wrap StyleGetFlexWrap()
        {
            return nodeStyle.FlexWrap;
        }

        /// <summary>Sets how the node handles content overflow.</summary>
        public void StyleSetOverflow(Overflows overflow)
        {
            if (nodeStyle.Overflow != overflow)
            {
                nodeStyle.Overflow = overflow;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's overflow behavior.</summary>
        public Overflows StyleGetOverflow()
        {
            return nodeStyle.Overflow;
        }

        /// <summary>Sets whether the node participates in flex layout.</summary>
        public void StyleSetDisplay(Displays display)
        {
            if (nodeStyle.Display != display)
            {
                nodeStyle.Display = display;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's display mode.</summary>
        public Displays StyleGetDisplay()
        {
            return nodeStyle.Display;
        }

        /// <summary>Sets the node's flex grow factor.</summary>
        public void StyleSetFlexGrow(float flexGrow)
        {
            if (nodeStyle.FlexGrow != flexGrow)
            {
                nodeStyle.FlexGrow = flexGrow;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's effective flex grow factor.</summary>
        public float StyleGetFlexGrow()
        {
            if (float.IsNaN(nodeStyle.FlexGrow))
            {
                return Constant.defaultFlexGrow;
            }
            return nodeStyle.FlexGrow;
        }

        /// <summary>Returns the node's effective flex shrink factor.</summary>
        public float StyleGetFlexShrink()
        {
            if (float.IsNaN(nodeStyle.FlexShrink))
            {
                if (config.UseWebDefaults)
                {
                    return Constant.webDefaultFlexShrink;
                }
                return Constant.defaultFlexShrink;
            }
            return nodeStyle.FlexShrink;
        }

        /// <summary>Sets the node's flex shrink factor.</summary>
        public void StyleSetFlexShrink(float flexShrink)
        {
            if (nodeStyle.FlexShrink != flexShrink)
            {
                nodeStyle.FlexShrink = flexShrink;
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's flex basis in points.</summary>
        public void StyleSetFlexBasis(float flexBasis)
        {
            if (nodeStyle.FlexBasis.value != flexBasis || nodeStyle.FlexBasis.unit != Unit.Point)
            {
                nodeStyle.FlexBasis = Flex.FloatIsUndefined(flexBasis)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(flexBasis, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's flex basis as a percentage.</summary>
        public void StyleSetFlexBasisPercent(float flexBasis)
        {
            if (nodeStyle.FlexBasis.value != flexBasis || nodeStyle.FlexBasis.unit != Unit.Percent)
            {
                nodeStyle.FlexBasis = Flex.FloatIsUndefined(flexBasis)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(flexBasis, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's flex basis to resolve automatically.</summary>
        public void NodeStyleSetFlexBasisAuto()
        {
            if (nodeStyle.FlexBasis.unit != Unit.Auto)
            {
                nodeStyle.FlexBasis = new Length(float.NaN, Unit.Auto);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured flex basis.</summary>
        public Length NodeStyleGetFlexBasis()
        {
            return nodeStyle.FlexBasis;
        }

        /// <summary>Sets the node's margin for the given edge in points.</summary>
        public void StyleSetMargin(Edge edge, float margin)
        {
            if (
                nodeStyle.Margin[(int)edge].value != margin
                || nodeStyle.Margin[(int)edge].unit != Unit.Point
            )
            {
                nodeStyle.Margin[(int)edge] = Flex.FloatIsUndefined(margin)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(margin, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's margin for the given edge as a percentage.</summary>
        public void StyleSetMarginPercent(Edge edge, float margin)
        {
            if (
                nodeStyle.Margin[(int)edge].value != margin
                || nodeStyle.Margin[(int)edge].unit != Unit.Percent
            )
            {
                nodeStyle.Margin[(int)edge] = Flex.FloatIsUndefined(margin)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(margin, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured margin for the given edge.</summary>
        public Length StyleGetMargin(Edge edge)
        {
            return nodeStyle.Margin[(int)edge];
        }

        /// <summary>Sets the node's margin for the given edge to auto.</summary>
        public void StyleSetMarginAuto(Edge edge)
        {
            if (nodeStyle.Margin[(int)edge].unit != Unit.Auto)
            {
                nodeStyle.Margin[(int)edge] = new Length(float.NaN, Unit.Auto);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's padding for the given edge in points.</summary>
        public void StyleSetPadding(Edge edge, float padding)
        {
            if (
                nodeStyle.Padding[(int)edge].value != padding
                || nodeStyle.Padding[(int)edge].unit != Unit.Point
            )
            {
                nodeStyle.Padding[(int)edge] = Flex.FloatIsUndefined(padding)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(padding, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's padding for the given edge as a percentage.</summary>
        public void StyleSetPaddingPercent(Edge edge, float padding)
        {
            if (
                nodeStyle.Padding[(int)edge].value != padding
                || nodeStyle.Padding[(int)edge].unit != Unit.Percent
            )
            {
                nodeStyle.Padding[(int)edge] = Flex.FloatIsUndefined(padding)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(padding, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured padding for the given edge.</summary>
        public Length StyleGetPadding(Edge edge)
        {
            return nodeStyle.Padding[(int)edge];
        }

        /// <summary>Sets the node's border width for the given edge in points.</summary>
        public void StyleSetBorder(Edge edge, float border)
        {
            if (
                nodeStyle.Border[(int)edge].value != border
                || nodeStyle.Border[(int)edge].unit != Unit.Point
            )
            {
                nodeStyle.Border[(int)edge] = Flex.FloatIsUndefined(border)
                    ? new Length(float.NaN, Unit.Undefined)
                    : new Length(border, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's border width for the given edge.</summary>
        public float StyleGetBorder(Edge edge)
        {
            return nodeStyle.Border[(int)edge].value;
        }

        /// <summary>Sets the node's minimum width in points.</summary>
        public void StyleSetMinWidth(float minWidth)
        {
            if (
                nodeStyle.MinDimensions[(int)Dimension.Width].value != minWidth
                || nodeStyle.MinDimensions[(int)Dimension.Width].unit != Unit.Point
            )
            {
                nodeStyle.MinDimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(minWidth)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(minWidth, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's minimum width as a percentage.</summary>
        public void StyleSetMinWidthPercent(float minWidth)
        {
            if (
                nodeStyle.MinDimensions[(int)Dimension.Width].value != minWidth
                || nodeStyle.MinDimensions[(int)Dimension.Width].unit != Unit.Percent
            )
            {
                nodeStyle.MinDimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(minWidth)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(minWidth, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured minimum width.</summary>
        public Length StyleGetMinWidth()
        {
            return nodeStyle.MinDimensions[(int)Dimension.Width];
        }

        /// <summary>Sets the node's minimum height in points.</summary>
        public void StyleSetMinHeight(float minHeight)
        {
            if (
                nodeStyle.MinDimensions[(int)Dimension.Height].value != minHeight
                || nodeStyle.MinDimensions[(int)Dimension.Height].unit != Unit.Point
            )
            {
                nodeStyle.MinDimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(minHeight)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(minHeight, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's minimum height as a percentage.</summary>
        public void StyleSetMinHeightPercent(float minHeight)
        {
            if (
                nodeStyle.MinDimensions[(int)Dimension.Height].value != minHeight
                || nodeStyle.MinDimensions[(int)Dimension.Height].unit != Unit.Percent
            )
            {
                nodeStyle.MinDimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(minHeight)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(minHeight, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured minimum height.</summary>
        public Length StyleGetMinHeight()
        {
            return nodeStyle.MinDimensions[(int)Dimension.Height];
        }

        /// <summary>Sets the node's maximum width in points.</summary>
        public void StyleSetMaxWidth(float maxWidth)
        {
            if (
                nodeStyle.MaxDimensions[(int)Dimension.Width].value != maxWidth
                || nodeStyle.MaxDimensions[(int)Dimension.Width].unit != Unit.Point
            )
            {
                nodeStyle.MaxDimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(maxWidth)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(maxWidth, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's maximum width as a percentage.</summary>
        public void StyleSetMaxWidthPercent(float maxWidth)
        {
            if (
                nodeStyle.MaxDimensions[(int)Dimension.Width].value != maxWidth
                || nodeStyle.MaxDimensions[(int)Dimension.Width].unit != Unit.Percent
            )
            {
                nodeStyle.MaxDimensions[(int)Dimension.Width] = Flex.FloatIsUndefined(maxWidth)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(maxWidth, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured maximum width.</summary>
        public Length StyleGetMaxWidth()
        {
            return nodeStyle.MaxDimensions[(int)Dimension.Width];
        }

        /// <summary>Sets the node's maximum height in points.</summary>
        public void StyleSetMaxHeight(float maxHeight)
        {
            if (
                nodeStyle.MaxDimensions[(int)Dimension.Height].value != maxHeight
                || nodeStyle.MaxDimensions[(int)Dimension.Height].unit != Unit.Point
            )
            {
                nodeStyle.MaxDimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(maxHeight)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(maxHeight, Unit.Point);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Sets the node's maximum height as a percentage.</summary>
        public void StyleSetMaxHeightPercent(float maxHeight)
        {
            if (
                nodeStyle.MaxDimensions[(int)Dimension.Height].value != maxHeight
                || nodeStyle.MaxDimensions[(int)Dimension.Height].unit != Unit.Percent
            )
            {
                nodeStyle.MaxDimensions[(int)Dimension.Height] = Flex.FloatIsUndefined(maxHeight)
                    ? new Length(float.NaN, Unit.Auto)
                    : new Length(maxHeight, Unit.Percent);
                Flex.nodeMarkDirtyInternal(this);
            }
        }

        /// <summary>Returns the node's configured maximum height.</summary>
        public Length StyleGetMaxHeight()
        {
            return nodeStyle.MaxDimensions[(int)Dimension.Height];
        }

        /// <summary>Sets the node's width-to-height aspect ratio.</summary>
        public void StyleSetAspectRatio(float aspectRatio)
        {
            if (nodeStyle.AspectRatio != aspectRatio)
            {
                nodeStyle.AspectRatio = aspectRatio;
                Flex.nodeMarkDirtyInternal(this);
            }
        }
    }
}
