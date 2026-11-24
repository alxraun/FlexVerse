// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    /// <summary>Implements the Flexbox layout calculation engine.</summary>
    public partial class Flex
    {
        /// <summary>Compares two floats with layout-grade tolerance.</summary>
        public static bool FloatsEqual(float a, float b)
        {
            if (FloatIsUndefined(a))
            {
                return FloatIsUndefined(b);
            }
            return System.Math.Abs(a - b) < 0.0001f;
        }

        /// <summary>Rounds a layout value to the nearest pixel grid position.</summary>
        public static float RoundValueToPixelGrid(
            float value,
            float pointScaleFactor,
            bool forceCeil,
            bool forceFloor
        )
        {
            var scaledValue = value * pointScaleFactor;
            var fractial = fmodf(scaledValue, 1f);
            if (FloatsEqual(fractial, 0))
            {
                scaledValue = scaledValue - fractial;
            }
            else if (FloatsEqual(fractial, 1))
            {
                scaledValue = scaledValue - fractial + 1;
            }
            else if (forceCeil)
            {
                scaledValue = scaledValue - fractial + 1;
            }
            else if (forceFloor)
            {
                scaledValue = scaledValue - fractial;
            }
            else
            {
                float f = 0;
                if (fractial >= 0.5f)
                {
                    f = 1.0f;
                }
                scaledValue = scaledValue - fractial + f;
            }
            return scaledValue / pointScaleFactor;
        }

        /// <summary>Copies the style from one node to another.</summary>
        public static void NodeCopyStyle(Node dstNode, Node srcNode)
        {
            if (!styleEq(dstNode.nodeStyle, srcNode.nodeStyle))
            {
                Style.Copy(dstNode.nodeStyle, srcNode.nodeStyle);
                nodeMarkDirtyInternal(dstNode);
            }
        }

        /// <summary>Resets a childless, detached node to its default state.</summary>
        public static void Reset(ref Node node)
        {
            assertWithNode(
                node,
                node.Children.Count == 0,
                "Cannot reset a node which still has children attached"
            );
            assertWithNode(
                node,
                node.Parent == null,
                "Cannot reset a node still attached to a parent"
            );
            node.Children.Clear();

            var config = node.config;
            node = CreateDefaultNode();
            if (config.UseWebDefaults)
            {
                node.nodeStyle.FlexDirection = FlexDirections.Row;
                node.nodeStyle.AlignContent = Align.Stretch;
            }
            node.config = config;
        }

        /// <summary>Creates a node with default style and config.</summary>
        public static Node CreateDefaultNode()
        {
            return new Node();
        }

        /// <summary>Creates a node with the given style.</summary>
        public static Node CreateDefaultNode(Style style)
        {
            return new Node(style);
        }

        /// <summary>Creates a node configured with the given config.</summary>
        public static Node CreateDefaultNode(Config config)
        {
            var node = new Node();
            if (config.UseWebDefaults)
            {
                node.nodeStyle.FlexDirection = FlexDirections.Row;
                node.nodeStyle.AlignContent = Align.Stretch;
            }
            node.config = config;
            return node;
        }

        /// <summary>Creates a config with default values.</summary>
        public static Config CreateDefaultConfig()
        {
            return new Config();
        }

        /// <summary>Computes layout for the node subtree within the parent size.</summary>
        public static void CalculateLayout(
            Node node,
            float parentWidth,
            float parentHeight,
            Directions parentDirection
        )
        {
            currentGenerationCount++;

            resolveDimensions(node);

            calcStartWidth(node, parentWidth, out float width, out MeasureMode widthMeasureMode);
            calcStartHeight(
                node,
                parentWidth,
                parentHeight,
                out float height,
                out MeasureMode heightMeasureMode
            );

            if (
                layoutNodeInternal(
                    node,
                    width,
                    height,
                    parentDirection,
                    widthMeasureMode,
                    heightMeasureMode,
                    parentWidth,
                    parentHeight,
                    true,
                    "initial",
                    node.config
                )
            )
            {
                nodeSetPosition(
                    node,
                    node.nodeLayout.Direction,
                    parentWidth,
                    parentHeight,
                    parentWidth
                );
                roundToPixelGrid(node, node.config.PointScaleFactor, 0, 0);

                if (gPrintTree)
                {
                    System.Console.WriteLine(
                        "NodePrint(node, PrintOptionsLayout|PrintOptionsChildren|PrintOptionsStyle);"
                    );
                }
            }
        }

        internal static readonly Length ValueZero = new(0, Unit.Point);
        internal static readonly Length ValueUndefined = new(float.NaN, Unit.Undefined);

        internal static readonly Length ValueAuto = new(float.NaN, Unit.Auto);

        internal static bool feq(float a, float b)
        {
            if (float.IsNaN(a) && float.IsNaN(b))
                return true;

            return a == b;
        }

        internal static Length computedEdgeValue(Length[] edges, Edge edge, Length defaultValue)
        {
            if (edges[(int)edge].unit != Unit.Undefined)
            {
                return edges[(int)edge];
            }

            bool isVertEdge = (edge == Edge.Top || edge == Edge.Bottom);
            if (isVertEdge && edges[(int)(Edge.Vertical)].unit != Unit.Undefined)
            {
                return edges[(int)Edge.Vertical];
            }

            bool isHorizEdge = (
                edge == Edge.Left || edge == Edge.Right || edge == Edge.Start || edge == Edge.End
            );
            if (isHorizEdge && edges[(int)Edge.Horizontal].unit != Unit.Undefined)
            {
                return edges[(int)Edge.Horizontal];
            }

            if (edges[(int)Edge.All].unit != Unit.Undefined)
            {
                return edges[(int)Edge.All];
            }

            if (edge == Edge.Start || edge == Edge.End)
            {
                return ValueUndefined;
            }

            return defaultValue;
        }

        internal static float resolveValue(Length value, float parentSize)
        {
            switch (value.unit)
            {
                case Unit.Undefined:
                case Unit.Auto:
                    return float.NaN;
                case Unit.Point:
                    return value.value;
                case Unit.Percent:
                    return value.value * parentSize / 100f;
            }
            return float.NaN;
        }

        internal static float resolveValueMargin(Length value, float parentSize)
        {
            if (value.unit == Unit.Auto)
            {
                return 0;
            }

            return resolveValue(value, parentSize);
        }

        internal static Node NewNodeWithConfig(Config config)
        {
            var node = CreateDefaultNode();

            if (config.UseWebDefaults)
            {
                node.nodeStyle.FlexDirection = FlexDirections.Row;
                node.nodeStyle.AlignContent = Align.Stretch;
            }
            node.config = config;
            return node;
        }

        internal static Node NewNode()
        {
            return NewNodeWithConfig(CreateDefaultConfig());
        }

        internal static Config ConfigGetDefault()
        {
            return CreateDefaultConfig();
        }

        internal static Config NewConfig()
        {
            return CreateDefaultConfig();
        }

        internal static void ConfigCopy(Config dest, Config src)
        {
            Config.Copy(dest, src);
        }

        internal static void nodeMarkDirtyInternal(Node node)
        {
            if (!node.IsDirty)
            {
                node.IsDirty = true;
                node.nodeLayout.computedFlexBasis = float.NaN;
                if (node.Parent != null)
                {
                    nodeMarkDirtyInternal(node.Parent);
                }
            }
        }

        internal static void SetMeasureFunc(Node node, MeasureFunc measureFunc)
        {
            if (measureFunc == null)
            {
                node.measureFunc = null;

                node.NodeType = NodeType.Default;
            }
            else
            {
                assertWithNode(
                    node,
                    node.Children.Count == 0,
                    "Cannot set measure function: Nodes with measure functions cannot have children."
                );
                node.measureFunc = measureFunc;

                node.NodeType = NodeType.Text;
            }
        }

        internal static void InsertChild(Node node, Node child, int idx)
        {
            assertWithNode(
                node,
                child.Parent == null,
                "Child already has a parent, it must be removed first."
            );
            assertWithNode(
                node,
                node.measureFunc == null,
                "Cannot add child: Nodes with measure functions cannot have children."
            );

            node.Children.Insert(idx, child);
            child.Parent = node;
            nodeMarkDirtyInternal(node);
        }

        internal static void RemoveChild(Node node, Node child)
        {
            if (node.Children.Remove(child))
            {
                child.nodeLayout.ResetToDefault();
                child.Parent = null;
                nodeMarkDirtyInternal(node);
            }
        }

        internal static Node GetChild(Node node, int idx)
        {
            return idx < node.Children.Count ? node.Children[idx] : null;
        }

        internal static void MarkDirty(Node node)
        {
            assertWithNode(
                node,
                node.measureFunc != null,
                "Only leaf nodes with custom measure functions should manually mark themselves as dirty"
            );
            nodeMarkDirtyInternal(node);
        }

        internal static bool styleEq(Style s1, Style s2)
        {
            if (
                s1.Direction != s2.Direction
                || s1.FlexDirection != s2.FlexDirection
                || s1.JustifyContent != s2.JustifyContent
                || s1.AlignContent != s2.AlignContent
                || s1.AlignItems != s2.AlignItems
                || s1.AlignSelf != s2.AlignSelf
                || s1.PositionType != s2.PositionType
                || s1.FlexWrap != s2.FlexWrap
                || s1.Overflow != s2.Overflow
                || s1.Display != s2.Display
                || !feq(s1.FlexGrow, s2.FlexGrow)
                || !feq(s1.FlexShrink, s2.FlexShrink)
                || s1.FlexBasis != s2.FlexBasis
            )
            {
                return false;
            }
            for (int i = 0; i < Constant.EdgeCount; i++)
            {
                if (
                    s1.Margin[i] != s2.Margin[i]
                    || s1.Position[i] != s2.Position[i]
                    || s1.Padding[i] != s2.Padding[i]
                    || s1.Border[i] != s2.Border[i]
                )
                {
                    return false;
                }
            }
            for (int i = 0; i < 2; i++)
            {
                if (
                    s1.Dimensions[i] != s2.Dimensions[i]
                    || s1.MinDimensions[i] != s2.MinDimensions[i]
                    || s1.MaxDimensions[i] != s2.MaxDimensions[i]
                )
                {
                    return false;
                }
            }
            return true;
        }

        internal static float resolveFlexGrow(Node node)
        {
            if (node.Parent == null)
            {
                return 0;
            }
            if (!FloatIsUndefined(node.nodeStyle.FlexGrow))
            {
                return node.nodeStyle.FlexGrow;
            }
            return Constant.defaultFlexGrow;
        }

        internal static float nodeResolveFlexShrink(Node node)
        {
            if (node.Parent == null)
            {
                return 0;
            }
            if (!FloatIsUndefined(node.nodeStyle.FlexShrink))
            {
                return node.nodeStyle.FlexShrink;
            }
            if (node.config.UseWebDefaults)
            {
                return Constant.webDefaultFlexShrink;
            }
            return Constant.defaultFlexShrink;
        }

        internal static Length nodeResolveFlexBasisPtr(Node node)
        {
            var style = node.nodeStyle;
            if (style.FlexBasis.unit != Unit.Auto && style.FlexBasis.unit != Unit.Undefined)
            {
                return style.FlexBasis;
            }
            return ValueAuto;
        }

        internal static int currentGenerationCount = 0;

        internal static bool FloatIsUndefined(float value)
        {
            return float.IsNaN(value);
        }

        internal static bool ValueEqual(Length a, Length b)
        {
            if (a.unit != b.unit)
            {
                return false;
            }

            if (a.unit == Unit.Undefined)
            {
                return true;
            }

            return System.Math.Abs(a.value - b.value) < 0.0001f;
        }

        internal static void resolveDimensions(Node node)
        {
            for (int dim = (int)Dimension.Width; dim <= (int)Dimension.Height; dim++)
            {
                if (
                    node.nodeStyle.MaxDimensions[dim].unit != Unit.Undefined
                    && ValueEqual(
                        node.nodeStyle.MaxDimensions[dim],
                        node.nodeStyle.MinDimensions[dim]
                    )
                )
                {
                    node.resolvedDimensions[dim] = node.nodeStyle.MaxDimensions[dim];
                }
                else
                {
                    node.resolvedDimensions[dim] = node.nodeStyle.Dimensions[dim];
                }
            }
        }

        internal static readonly Edge[] leading = [Edge.Top, Edge.Bottom, Edge.Left, Edge.Right];
        internal static readonly Edge[] trailing = [Edge.Bottom, Edge.Top, Edge.Right, Edge.Left];
        internal static readonly Edge[] pos = [Edge.Top, Edge.Bottom, Edge.Left, Edge.Right];
        internal static readonly Dimension[] dim =
        [
            Dimension.Height,
            Dimension.Height,
            Dimension.Width,
            Dimension.Width,
        ];

        internal static bool flexDirectionIsRow(FlexDirections flexDirection)
        {
            return flexDirection == FlexDirections.Row
                || flexDirection == FlexDirections.RowReverse;
        }

        internal static bool flexDirectionIsColumn(FlexDirections flexDirection)
        {
            return flexDirection == FlexDirections.Column
                || flexDirection == FlexDirections.ColumnReverse;
        }

        internal static float nodeLeadingMargin(Node node, FlexDirections axis, float widthSize)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Margin[(int)Edge.Start].unit != Unit.Undefined
            )
            {
                return resolveValueMargin(node.nodeStyle.Margin[(int)Edge.Start], widthSize);
            }

            var v = computedEdgeValue(node.nodeStyle.Margin, leading[(int)axis], ValueZero);
            return resolveValueMargin(v, widthSize);
        }

        internal static float nodeTrailingMargin(Node node, FlexDirections axis, float widthSize)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Margin[(int)Edge.End].unit != Unit.Undefined
            )
            {
                return resolveValueMargin(node.nodeStyle.Margin[(int)Edge.End], widthSize);
            }

            return resolveValueMargin(
                computedEdgeValue(node.nodeStyle.Margin, trailing[(int)axis], ValueZero),
                widthSize
            );
        }

        internal static float nodeLeadingPadding(Node node, FlexDirections axis, float widthSize)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Padding[(int)Edge.Start].unit != Unit.Undefined
                && resolveValue(node.nodeStyle.Padding[(int)Edge.Start], widthSize) >= 0
            )
            {
                return resolveValue(node.nodeStyle.Padding[(int)Edge.Start], widthSize);
            }

            return fmaxf(
                resolveValue(
                    computedEdgeValue(node.nodeStyle.Padding, leading[(int)axis], ValueZero),
                    widthSize
                ),
                0
            );
        }

        internal static float nodeTrailingPadding(Node node, FlexDirections axis, float widthSize)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Padding[(int)Edge.End].unit != Unit.Undefined
                && resolveValue(node.nodeStyle.Padding[(int)Edge.End], widthSize) >= 0
            )
            {
                return resolveValue(node.nodeStyle.Padding[(int)Edge.End], widthSize);
            }

            return fmaxf(
                resolveValue(
                    computedEdgeValue(node.nodeStyle.Padding, trailing[(int)axis], ValueZero),
                    widthSize
                ),
                0
            );
        }

        internal static float nodeLeadingBorder(Node node, FlexDirections axis)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Border[(int)Edge.Start].unit != Unit.Undefined
                && node.nodeStyle.Border[(int)Edge.Start].value >= 0
            )
            {
                return node.nodeStyle.Border[(int)Edge.Start].value;
            }

            return fmaxf(
                computedEdgeValue(node.nodeStyle.Border, leading[(int)axis], ValueZero).value,
                0
            );
        }

        internal static float nodeTrailingBorder(Node node, FlexDirections axis)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Border[(int)Edge.End].unit != Unit.Undefined
                && node.nodeStyle.Border[(int)Edge.End].value >= 0
            )
            {
                return node.nodeStyle.Border[(int)Edge.End].value;
            }

            return fmaxf(
                computedEdgeValue(node.nodeStyle.Border, trailing[(int)axis], ValueZero).value,
                0
            );
        }

        internal static float nodeLeadingPaddingAndBorder(
            Node node,
            FlexDirections axis,
            float widthSize
        )
        {
            return nodeLeadingPadding(node, axis, widthSize) + nodeLeadingBorder(node, axis);
        }

        internal static float nodeTrailingPaddingAndBorder(
            Node node,
            FlexDirections axis,
            float widthSize
        )
        {
            return nodeTrailingPadding(node, axis, widthSize) + nodeTrailingBorder(node, axis);
        }

        internal static float nodeMarginForAxis(Node node, FlexDirections axis, float widthSize)
        {
            var leading = nodeLeadingMargin(node, axis, widthSize);
            var trailing = nodeTrailingMargin(node, axis, widthSize);
            return leading + trailing;
        }

        internal static float nodePaddingAndBorderForAxis(
            Node node,
            FlexDirections axis,
            float widthSize
        )
        {
            return nodeLeadingPaddingAndBorder(node, axis, widthSize)
                + nodeTrailingPaddingAndBorder(node, axis, widthSize);
        }

        internal static Align nodeAlignItem(Node node, Node child)
        {
            var align = child.nodeStyle.AlignSelf;
            if (child.nodeStyle.AlignSelf == Align.Auto)
            {
                align = node.nodeStyle.AlignItems;
            }
            if (align == Align.Baseline && flexDirectionIsColumn(node.nodeStyle.FlexDirection))
            {
                return Align.FlexStart;
            }
            return align;
        }

        internal static Directions nodeResolveDirection(Node node, Directions parentDirection)
        {
            if (node.nodeStyle.Direction == Directions.Inherit)
            {
                if (parentDirection > Directions.Inherit)
                {
                    return parentDirection;
                }
                return Directions.LTR;
            }
            return node.nodeStyle.Direction;
        }

        internal static float Baseline(Node node)
        {
            if (node.baselineFunc != null)
            {
                var baseline = node.baselineFunc(
                    node,
                    node.nodeLayout.measuredDimensions[(int)Dimension.Width],
                    node.nodeLayout.measuredDimensions[(int)Dimension.Height]
                );
                assertWithNode(
                    node,
                    !FloatIsUndefined(baseline),
                    "Expect custom baseline function to not return NaN"
                );
                return baseline;
            }
            else
            {
                Node baselineChild = null;
                foreach (var child in node.Children)
                {
                    if (child.lineIndex > 0)
                    {
                        break;
                    }
                    if (child.nodeStyle.PositionType == PositionTypes.Absolute)
                    {
                        continue;
                    }
                    if (nodeAlignItem(node, child) == Align.Baseline)
                    {
                        baselineChild = child;
                        break;
                    }

                    if (baselineChild == null)
                    {
                        baselineChild = child;
                    }
                }

                if (baselineChild == null)
                {
                    return node.nodeLayout.measuredDimensions[(int)Dimension.Height];
                }

                var baseline = Baseline(baselineChild);
                return baseline + baselineChild.nodeLayout.Position[(int)Edge.Top];
            }
        }

        internal static FlexDirections resolveFlexDirection(
            FlexDirections flexDirection,
            Directions direction
        )
        {
            if (direction == Directions.RTL)
            {
                if (flexDirection == FlexDirections.Row)
                {
                    return FlexDirections.RowReverse;
                }
                else if (flexDirection == FlexDirections.RowReverse)
                {
                    return FlexDirections.Row;
                }
            }
            return flexDirection;
        }

        internal static FlexDirections flexDirectionCross(
            FlexDirections flexDirection,
            Directions direction
        )
        {
            if (flexDirectionIsColumn(flexDirection))
            {
                return resolveFlexDirection(FlexDirections.Row, direction);
            }
            return FlexDirections.Column;
        }

        internal static bool nodeIsFlex(Node node)
        {
            return (
                node.nodeStyle.PositionType == PositionTypes.Relative
                && (resolveFlexGrow(node) != 0 || nodeResolveFlexShrink(node) != 0)
            );
        }

        internal static bool isBaselineLayout(Node node)
        {
            if (flexDirectionIsColumn(node.nodeStyle.FlexDirection))
            {
                return false;
            }
            if (node.nodeStyle.AlignItems == Align.Baseline)
            {
                return true;
            }
            foreach (var child in node.Children)
            {
                if (
                    child.nodeStyle.PositionType == PositionTypes.Relative
                    && child.nodeStyle.AlignSelf == Align.Baseline
                )
                {
                    return true;
                }
            }

            return false;
        }

        internal static float nodeDimWithMargin(Node node, FlexDirections axis, float widthSize)
        {
            return node.nodeLayout.measuredDimensions[(int)dim[(int)axis]]
                + nodeLeadingMargin(node, axis, widthSize)
                + nodeTrailingMargin(node, axis, widthSize);
        }

        internal static bool nodeIsStyleDimDefined(Node node, FlexDirections axis, float parentSize)
        {
            var v = node.resolvedDimensions[(int)dim[(int)axis]];
            var isNotDefined = (
                v.unit == Unit.Auto
                || v.unit == Unit.Undefined
                || (v.unit == Unit.Point && v.value < 0)
                || (v.unit == Unit.Percent && (v.value < 0 || FloatIsUndefined(parentSize)))
            );
            return !isNotDefined;
        }

        internal static bool nodeIsLayoutDimDefined(Node node, FlexDirections axis)
        {
            var value = node.nodeLayout.measuredDimensions[(int)dim[(int)axis]];
            return (!FloatIsUndefined(value) && value >= 0);
        }

        internal static bool nodeIsLeadingPosDefined(Node node, FlexDirections axis)
        {
            return (
                    flexDirectionIsRow(axis)
                    && computedEdgeValue(node.nodeStyle.Position, Edge.Start, ValueUndefined).unit
                        != Unit.Undefined
                )
                || computedEdgeValue(
                    node.nodeStyle.Position,
                    leading[(int)axis],
                    ValueUndefined
                ).unit != Unit.Undefined;
        }

        internal static bool nodeIsTrailingPosDefined(Node node, FlexDirections axis)
        {
            return (
                    flexDirectionIsRow(axis)
                    && computedEdgeValue(node.nodeStyle.Position, Edge.End, ValueUndefined).unit
                        != Unit.Undefined
                )
                || computedEdgeValue(
                    node.nodeStyle.Position,
                    trailing[(int)axis],
                    ValueUndefined
                ).unit != Unit.Undefined;
        }

        internal static float nodeLeadingPosition(Node node, FlexDirections axis, float axisSize)
        {
            if (flexDirectionIsRow(axis))
            {
                var leadingPosition = computedEdgeValue(
                    node.nodeStyle.Position,
                    Edge.Start,
                    ValueUndefined
                );
                if (leadingPosition.unit != Unit.Undefined)
                {
                    return resolveValue(leadingPosition, axisSize);
                }
            }

            {
                var leadingPosition = computedEdgeValue(
                    node.nodeStyle.Position,
                    leading[(int)axis],
                    ValueUndefined
                );

                if (leadingPosition.unit == Unit.Undefined)
                {
                    return 0;
                }
                return resolveValue(leadingPosition, axisSize);
            }
        }

        internal static float nodeTrailingPosition(Node node, FlexDirections axis, float axisSize)
        {
            if (flexDirectionIsRow(axis))
            {
                var trailingPosition = computedEdgeValue(
                    node.nodeStyle.Position,
                    Edge.End,
                    ValueUndefined
                );
                if (trailingPosition.unit != Unit.Undefined)
                {
                    return resolveValue(trailingPosition, axisSize);
                }
            }

            {
                var trailingPosition = computedEdgeValue(
                    node.nodeStyle.Position,
                    trailing[(int)axis],
                    ValueUndefined
                );

                if (trailingPosition.unit == Unit.Undefined)
                {
                    return 0;
                }
                return resolveValue(trailingPosition, axisSize);
            }
        }

        internal static float nodeBoundAxisWithinMinAndMax(
            Node node,
            FlexDirections axis,
            float value,
            float axisSize
        )
        {
            var min = float.NaN;
            var max = float.NaN;

            if (flexDirectionIsColumn(axis))
            {
                min = resolveValue(node.nodeStyle.MinDimensions[(int)Dimension.Height], axisSize);
                max = resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Height], axisSize);
            }
            else if (flexDirectionIsRow(axis))
            {
                min = resolveValue(node.nodeStyle.MinDimensions[(int)Dimension.Width], axisSize);
                max = resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Width], axisSize);
            }

            var boundValue = value;

            if (!FloatIsUndefined(max) && max >= 0 && boundValue > max)
            {
                boundValue = max;
            }

            if (!FloatIsUndefined(min) && min >= 0 && boundValue < min)
            {
                boundValue = min;
            }

            return boundValue;
        }

        internal static Length marginLeadingValue(Node node, FlexDirections axis)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Margin[(int)Edge.Start].unit != Unit.Undefined
            )
            {
                return node.nodeStyle.Margin[(int)Edge.Start];
            }
            return node.nodeStyle.Margin[(int)leading[(int)axis]];
        }

        internal static Length marginTrailingValue(Node node, FlexDirections axis)
        {
            if (
                flexDirectionIsRow(axis)
                && node.nodeStyle.Margin[(int)Edge.End].unit != Unit.Undefined
            )
            {
                return node.nodeStyle.Margin[(int)Edge.End];
            }
            return node.nodeStyle.Margin[(int)trailing[(int)axis]];
        }

        internal static float nodeBoundAxis(
            Node node,
            FlexDirections axis,
            float value,
            float axisSize,
            float widthSize
        )
        {
            return fmaxf(
                nodeBoundAxisWithinMinAndMax(node, axis, value, axisSize),
                nodePaddingAndBorderForAxis(node, axis, widthSize)
            );
        }

        internal static void nodeSetChildTrailingPosition(
            Node node,
            Node child,
            FlexDirections axis
        )
        {
            var size = child.nodeLayout.measuredDimensions[(int)dim[(int)axis]];
            child.nodeLayout.Position[(int)trailing[(int)axis]] =
                node.nodeLayout.measuredDimensions[(int)dim[(int)axis]]
                - size
                - child.nodeLayout.Position[(int)pos[(int)axis]];
        }

        internal static float nodeRelativePosition(Node node, FlexDirections axis, float axisSize)
        {
            if (nodeIsLeadingPosDefined(node, axis))
            {
                return nodeLeadingPosition(node, axis, axisSize);
            }
            return -nodeTrailingPosition(node, axis, axisSize);
        }

        internal static void constrainMaxSizeForMode(
            Node node,
            FlexDirections axis,
            float parentAxisSize,
            float parentWidth,
            ref MeasureMode mode,
            ref float size
        )
        {
            var maxSize =
                resolveValue(node.nodeStyle.MaxDimensions[(int)dim[(int)axis]], parentAxisSize)
                + nodeMarginForAxis(node, axis, parentWidth);
            switch (mode)
            {
                case MeasureMode.Exactly:
                case MeasureMode.AtMost:
                    if (FloatIsUndefined(maxSize) || size < maxSize) { }
                    else
                    {
                        size = maxSize;
                    }
                    break;
                case MeasureMode.Undefined:
                    if (!FloatIsUndefined(maxSize))
                    {
                        mode = MeasureMode.AtMost;
                        size = maxSize;
                    }
                    break;
            }
        }

        internal static void nodeSetPosition(
            Node node,
            Directions direction,
            float mainSize,
            float crossSize,
            float parentWidth
        )
        {
            var directionRespectingRoot = Directions.LTR;
            if (node.Parent != null)
            {
                directionRespectingRoot = direction;
            }

            var mainAxis = resolveFlexDirection(
                node.nodeStyle.FlexDirection,
                directionRespectingRoot
            );
            var crossAxis = flexDirectionCross(mainAxis, directionRespectingRoot);

            var relativePositionMain = nodeRelativePosition(node, mainAxis, mainSize);
            var relativePositionCross = nodeRelativePosition(node, crossAxis, crossSize);

            var pos = node.nodeLayout.Position;
            pos[(int)leading[(int)mainAxis]] =
                nodeLeadingMargin(node, mainAxis, parentWidth) + relativePositionMain;
            pos[(int)trailing[(int)mainAxis]] =
                nodeTrailingMargin(node, mainAxis, parentWidth) + relativePositionMain;
            pos[(int)leading[(int)crossAxis]] =
                nodeLeadingMargin(node, crossAxis, parentWidth) + relativePositionCross;
            pos[(int)trailing[(int)crossAxis]] =
                nodeTrailingMargin(node, crossAxis, parentWidth) + relativePositionCross;
        }

        internal static void nodeComputeFlexBasisForChild(
            Node node,
            Node child,
            float width,
            MeasureMode widthMode,
            float height,
            float parentWidth,
            float parentHeight,
            MeasureMode heightMode,
            Directions direction,
            Config config
        )
        {
            var mainAxis = resolveFlexDirection(node.nodeStyle.FlexDirection, direction);
            var isMainAxisRow = flexDirectionIsRow(mainAxis);
            var mainAxisSize = height;
            var mainAxisParentSize = parentHeight;
            if (isMainAxisRow)
            {
                mainAxisSize = width;
                mainAxisParentSize = parentWidth;
            }

            float childWidth;
            float childHeight;
            MeasureMode childWidthMeasureMode;
            MeasureMode childHeightMeasureMode;

            var resolvedFlexBasis = resolveValue(
                nodeResolveFlexBasisPtr(child),
                mainAxisParentSize
            );
            var isRowStyleDimDefined = nodeIsStyleDimDefined(
                child,
                FlexDirections.Row,
                parentWidth
            );
            var isColumnStyleDimDefined = nodeIsStyleDimDefined(
                child,
                FlexDirections.Column,
                parentHeight
            );

            if (!FloatIsUndefined(resolvedFlexBasis) && !FloatIsUndefined(mainAxisSize))
            {
                if (
                    FloatIsUndefined(child.nodeLayout.computedFlexBasis)
                    || (
                        child.config.IsExperimentalFeatureEnabled(ExperimentalFeature.WebFlexBasis)
                        && child.nodeLayout.computedFlexBasisGeneration != currentGenerationCount
                    )
                )
                {
                    child.nodeLayout.computedFlexBasis = fmaxf(
                        resolvedFlexBasis,
                        nodePaddingAndBorderForAxis(child, mainAxis, parentWidth)
                    );
                }
            }
            else if (isMainAxisRow && isRowStyleDimDefined)
            {
                child.nodeLayout.computedFlexBasis = fmaxf(
                    resolveValue(child.resolvedDimensions[(int)Dimension.Width], parentWidth),
                    nodePaddingAndBorderForAxis(child, FlexDirections.Row, parentWidth)
                );
            }
            else if (!isMainAxisRow && isColumnStyleDimDefined)
            {
                child.nodeLayout.computedFlexBasis = fmaxf(
                    resolveValue(child.resolvedDimensions[(int)Dimension.Height], parentHeight),
                    nodePaddingAndBorderForAxis(child, FlexDirections.Column, parentWidth)
                );
            }
            else
            {
                childWidth = float.NaN;
                childHeight = float.NaN;
                childWidthMeasureMode = MeasureMode.Undefined;
                childHeightMeasureMode = MeasureMode.Undefined;

                var marginRow = nodeMarginForAxis(child, FlexDirections.Row, parentWidth);
                var marginColumn = nodeMarginForAxis(child, FlexDirections.Column, parentWidth);

                if (isRowStyleDimDefined)
                {
                    childWidth =
                        resolveValue(child.resolvedDimensions[(int)Dimension.Width], parentWidth)
                        + marginRow;
                    childWidthMeasureMode = MeasureMode.Exactly;
                }
                if (isColumnStyleDimDefined)
                {
                    childHeight =
                        resolveValue(child.resolvedDimensions[(int)Dimension.Height], parentHeight)
                        + marginColumn;
                    childHeightMeasureMode = MeasureMode.Exactly;
                }

                if (
                    (!isMainAxisRow && node.nodeStyle.Overflow == Overflows.Scroll)
                    || node.nodeStyle.Overflow != Overflows.Scroll
                )
                {
                    if (FloatIsUndefined(childWidth) && !FloatIsUndefined(width))
                    {
                        childWidth = width;
                        childWidthMeasureMode = MeasureMode.AtMost;
                    }
                }

                if (
                    (isMainAxisRow && node.nodeStyle.Overflow == Overflows.Scroll)
                    || node.nodeStyle.Overflow != Overflows.Scroll
                )
                {
                    if (FloatIsUndefined(childHeight) && !FloatIsUndefined(height))
                    {
                        childHeight = height;
                        childHeightMeasureMode = MeasureMode.AtMost;
                    }
                }

                if (
                    !isMainAxisRow
                    && !FloatIsUndefined(width)
                    && !isRowStyleDimDefined
                    && widthMode == MeasureMode.Exactly
                    && node.nodeStyle.FlexWrap == Wrap.NoWrap
                    && nodeAlignItem(node, child) == Align.Stretch
                )
                {
                    childWidth = width;
                    childWidthMeasureMode = MeasureMode.Exactly;
                }
                if (
                    isMainAxisRow
                    && !FloatIsUndefined(height)
                    && !isColumnStyleDimDefined
                    && heightMode == MeasureMode.Exactly
                    && node.nodeStyle.FlexWrap == Wrap.NoWrap
                    && nodeAlignItem(node, child) == Align.Stretch
                )
                {
                    childHeight = height;
                    childHeightMeasureMode = MeasureMode.Exactly;
                }

                if (!FloatIsUndefined(child.nodeStyle.AspectRatio))
                {
                    if (!isMainAxisRow && childWidthMeasureMode == MeasureMode.Exactly)
                    {
                        child.nodeLayout.computedFlexBasis = fmaxf(
                            (childWidth - marginRow) / child.nodeStyle.AspectRatio,
                            nodePaddingAndBorderForAxis(child, FlexDirections.Column, parentWidth)
                        );
                        return;
                    }
                    else if (isMainAxisRow && childHeightMeasureMode == MeasureMode.Exactly)
                    {
                        child.nodeLayout.computedFlexBasis = fmaxf(
                            (childHeight - marginColumn) * child.nodeStyle.AspectRatio,
                            nodePaddingAndBorderForAxis(child, FlexDirections.Row, parentWidth)
                        );
                        return;
                    }
                }

                constrainMaxSizeForMode(
                    child,
                    FlexDirections.Row,
                    parentWidth,
                    parentWidth,
                    ref childWidthMeasureMode,
                    ref childWidth
                );
                constrainMaxSizeForMode(
                    child,
                    FlexDirections.Column,
                    parentHeight,
                    parentWidth,
                    ref childHeightMeasureMode,
                    ref childHeight
                );

                layoutNodeInternal(
                    child,
                    childWidth,
                    childHeight,
                    direction,
                    childWidthMeasureMode,
                    childHeightMeasureMode,
                    parentWidth,
                    parentHeight,
                    false,
                    "measure",
                    config
                );

                child.nodeLayout.computedFlexBasis = fmaxf(
                    child.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]],
                    nodePaddingAndBorderForAxis(child, mainAxis, parentWidth)
                );
            }

            child.nodeLayout.computedFlexBasisGeneration = currentGenerationCount;
        }

        internal static void nodeAbsoluteLayoutChild(
            Node node,
            Node child,
            float width,
            MeasureMode widthMode,
            float height,
            Directions direction,
            Config config
        )
        {
            var mainAxis = resolveFlexDirection(node.nodeStyle.FlexDirection, direction);
            var crossAxis = flexDirectionCross(mainAxis, direction);
            var isMainAxisRow = flexDirectionIsRow(mainAxis);

            var childWidth = float.NaN;
            var childHeight = float.NaN;
            var childWidthMeasureMode = MeasureMode.Undefined;
            var childHeightMeasureMode = MeasureMode.Undefined;

            var marginRow = nodeMarginForAxis(child, FlexDirections.Row, width);
            var marginColumn = nodeMarginForAxis(child, FlexDirections.Column, width);

            if (nodeIsStyleDimDefined(child, FlexDirections.Row, width))
            {
                childWidth =
                    resolveValue(child.resolvedDimensions[(int)Dimension.Width], width) + marginRow;
            }
            else
            {
                if (
                    nodeIsLeadingPosDefined(child, FlexDirections.Row)
                    && nodeIsTrailingPosDefined(child, FlexDirections.Row)
                )
                {
                    childWidth =
                        node.nodeLayout.measuredDimensions[(int)Dimension.Width]
                        - (
                            nodeLeadingBorder(node, FlexDirections.Row)
                            + nodeTrailingBorder(node, FlexDirections.Row)
                        )
                        - (
                            nodeLeadingPosition(child, FlexDirections.Row, width)
                            + nodeTrailingPosition(child, FlexDirections.Row, width)
                        );
                    childWidth = nodeBoundAxis(child, FlexDirections.Row, childWidth, width, width);
                }
            }

            if (nodeIsStyleDimDefined(child, FlexDirections.Column, height))
            {
                childHeight =
                    resolveValue(child.resolvedDimensions[(int)Dimension.Height], height)
                    + marginColumn;
            }
            else
            {
                if (
                    nodeIsLeadingPosDefined(child, FlexDirections.Column)
                    && nodeIsTrailingPosDefined(child, FlexDirections.Column)
                )
                {
                    childHeight =
                        node.nodeLayout.measuredDimensions[(int)Dimension.Height]
                        - (
                            nodeLeadingBorder(node, FlexDirections.Column)
                            + nodeTrailingBorder(node, FlexDirections.Column)
                        )
                        - (
                            nodeLeadingPosition(child, FlexDirections.Column, height)
                            + nodeTrailingPosition(child, FlexDirections.Column, height)
                        );
                    childHeight = nodeBoundAxis(
                        child,
                        FlexDirections.Column,
                        childHeight,
                        height,
                        width
                    );
                }
            }

            if (FloatIsUndefined(childWidth) != FloatIsUndefined(childHeight))
            {
                if (!FloatIsUndefined(child.nodeStyle.AspectRatio))
                {
                    if (FloatIsUndefined(childWidth))
                    {
                        childWidth =
                            marginRow
                            + fmaxf(
                                (childHeight - marginColumn) * child.nodeStyle.AspectRatio,
                                nodePaddingAndBorderForAxis(child, FlexDirections.Column, width)
                            );
                    }
                    else if (FloatIsUndefined(childHeight))
                    {
                        childHeight =
                            marginColumn
                            + fmaxf(
                                (childWidth - marginRow) / child.nodeStyle.AspectRatio,
                                nodePaddingAndBorderForAxis(child, FlexDirections.Row, width)
                            );
                    }
                }
            }

            if (FloatIsUndefined(childWidth) || FloatIsUndefined(childHeight))
            {
                childWidthMeasureMode = MeasureMode.Exactly;
                if (FloatIsUndefined(childWidth))
                {
                    childWidthMeasureMode = MeasureMode.Undefined;
                }
                childHeightMeasureMode = MeasureMode.Exactly;
                if (FloatIsUndefined(childHeight))
                {
                    childHeightMeasureMode = MeasureMode.Undefined;
                }

                if (
                    !isMainAxisRow
                    && FloatIsUndefined(childWidth)
                    && widthMode != MeasureMode.Undefined
                    && width > 0
                )
                {
                    childWidth = width;
                    childWidthMeasureMode = MeasureMode.AtMost;
                }

                layoutNodeInternal(
                    child,
                    childWidth,
                    childHeight,
                    direction,
                    childWidthMeasureMode,
                    childHeightMeasureMode,
                    childWidth,
                    childHeight,
                    false,
                    "abs-measure",
                    config
                );
                childWidth =
                    child.nodeLayout.measuredDimensions[(int)Dimension.Width]
                    + nodeMarginForAxis(child, FlexDirections.Row, width);
                childHeight =
                    child.nodeLayout.measuredDimensions[(int)Dimension.Height]
                    + nodeMarginForAxis(child, FlexDirections.Column, width);
            }

            layoutNodeInternal(
                child,
                childWidth,
                childHeight,
                direction,
                MeasureMode.Exactly,
                MeasureMode.Exactly,
                childWidth,
                childHeight,
                true,
                "abs-layout",
                config
            );

            if (
                nodeIsTrailingPosDefined(child, mainAxis)
                && !nodeIsLeadingPosDefined(child, mainAxis)
            )
            {
                var axisSize = height;
                if (isMainAxisRow)
                {
                    axisSize = width;
                }
                child.nodeLayout.Position[(int)leading[(int)mainAxis]] =
                    node.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                    - child.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                    - nodeTrailingBorder(node, mainAxis)
                    - nodeTrailingMargin(child, mainAxis, width)
                    - nodeTrailingPosition(child, mainAxis, axisSize);
            }
            else if (
                !nodeIsLeadingPosDefined(child, mainAxis)
                && node.nodeStyle.JustifyContent == Justify.Center
            )
            {
                child.nodeLayout.Position[(int)leading[(int)mainAxis]] =
                    (
                        node.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                        - child.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                    ) / 2.0f;
            }
            else if (
                !nodeIsLeadingPosDefined(child, mainAxis)
                && node.nodeStyle.JustifyContent == Justify.FlexEnd
            )
            {
                child.nodeLayout.Position[(int)leading[(int)mainAxis]] = (
                    node.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                    - child.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]]
                );
            }

            if (
                nodeIsTrailingPosDefined(child, crossAxis)
                && !nodeIsLeadingPosDefined(child, crossAxis)
            )
            {
                var axisSize = width;
                if (isMainAxisRow)
                {
                    axisSize = height;
                }

                child.nodeLayout.Position[(int)leading[(int)crossAxis]] =
                    node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                    - child.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                    - nodeTrailingBorder(node, crossAxis)
                    - nodeTrailingMargin(child, crossAxis, width)
                    - nodeTrailingPosition(child, crossAxis, axisSize);
            }
            else if (
                !nodeIsLeadingPosDefined(child, crossAxis)
                && nodeAlignItem(node, child) == Align.Center
            )
            {
                child.nodeLayout.Position[(int)leading[(int)crossAxis]] =
                    (
                        node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                        - child.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                    ) / 2.0f;
            }
            else if (
                !nodeIsLeadingPosDefined(child, crossAxis)
                && (
                    (nodeAlignItem(node, child) == Align.FlexEnd)
                    != (node.nodeStyle.FlexWrap == Wrap.WrapReverse)
                )
            )
            {
                child.nodeLayout.Position[(int)leading[(int)crossAxis]] = (
                    node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                    - child.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                );
            }
        }

        internal static void nodeWithMeasureFuncSetMeasuredDimensions(
            Node node,
            float availableWidth,
            float availableHeight,
            MeasureMode widthMeasureMode,
            MeasureMode heightMeasureMode,
            float parentWidth,
            float parentHeight
        )
        {
            assertWithNode(
                node,
                node.measureFunc != null,
                "Expected node to have custom measure function"
            );

            var paddingAndBorderAxisRow = nodePaddingAndBorderForAxis(
                node,
                FlexDirections.Row,
                availableWidth
            );
            var paddingAndBorderAxisColumn = nodePaddingAndBorderForAxis(
                node,
                FlexDirections.Column,
                availableWidth
            );
            var marginAxisRow = nodeMarginForAxis(node, FlexDirections.Row, availableWidth);
            var marginAxisColumn = nodeMarginForAxis(node, FlexDirections.Column, availableWidth);

            var innerWidth = fmaxf(0, availableWidth - marginAxisRow - paddingAndBorderAxisRow);
            if (FloatIsUndefined(availableWidth))
            {
                innerWidth = availableWidth;
            }
            var innerHeight = fmaxf(
                0,
                availableHeight - marginAxisColumn - paddingAndBorderAxisColumn
            );
            if (FloatIsUndefined(availableHeight))
            {
                innerHeight = availableHeight;
            }

            if (widthMeasureMode == MeasureMode.Exactly && heightMeasureMode == MeasureMode.Exactly)
            {
                node.nodeLayout.measuredDimensions[(int)Dimension.Width] = nodeBoundAxis(
                    node,
                    FlexDirections.Row,
                    availableWidth - marginAxisRow,
                    parentWidth,
                    parentWidth
                );
                node.nodeLayout.measuredDimensions[(int)Dimension.Height] = nodeBoundAxis(
                    node,
                    FlexDirections.Column,
                    availableHeight - marginAxisColumn,
                    parentHeight,
                    parentWidth
                );
            }
            else
            {
                var measuredSize = node.measureFunc(
                    node,
                    innerWidth,
                    widthMeasureMode,
                    innerHeight,
                    heightMeasureMode
                );

                var width = availableWidth - marginAxisRow;
                if (
                    widthMeasureMode == MeasureMode.Undefined
                    || widthMeasureMode == MeasureMode.AtMost
                )
                {
                    width = measuredSize.Width + paddingAndBorderAxisRow;
                }

                node.nodeLayout.measuredDimensions[(int)Dimension.Width] = nodeBoundAxis(
                    node,
                    FlexDirections.Row,
                    width,
                    availableWidth,
                    availableWidth
                );

                var height = availableHeight - marginAxisColumn;
                if (
                    heightMeasureMode == MeasureMode.Undefined
                    || heightMeasureMode == MeasureMode.AtMost
                )
                {
                    height = measuredSize.Height + paddingAndBorderAxisColumn;
                }

                node.nodeLayout.measuredDimensions[(int)Dimension.Height] = nodeBoundAxis(
                    node,
                    FlexDirections.Column,
                    height,
                    availableHeight,
                    availableWidth
                );
            }
        }

        internal static void nodeEmptyContainerSetMeasuredDimensions(
            Node node,
            float availableWidth,
            float availableHeight,
            MeasureMode widthMeasureMode,
            MeasureMode heightMeasureMode,
            float parentWidth,
            float parentHeight
        )
        {
            var paddingAndBorderAxisRow = nodePaddingAndBorderForAxis(
                node,
                FlexDirections.Row,
                parentWidth
            );
            var paddingAndBorderAxisColumn = nodePaddingAndBorderForAxis(
                node,
                FlexDirections.Column,
                parentWidth
            );
            var marginAxisRow = nodeMarginForAxis(node, FlexDirections.Row, parentWidth);
            var marginAxisColumn = nodeMarginForAxis(node, FlexDirections.Column, parentWidth);

            var width = availableWidth - marginAxisRow;
            if (widthMeasureMode == MeasureMode.Undefined || widthMeasureMode == MeasureMode.AtMost)
            {
                width = paddingAndBorderAxisRow;
            }
            node.nodeLayout.measuredDimensions[(int)Dimension.Width] = nodeBoundAxis(
                node,
                FlexDirections.Row,
                width,
                parentWidth,
                parentWidth
            );

            var height = availableHeight - marginAxisColumn;
            if (
                heightMeasureMode == MeasureMode.Undefined
                || heightMeasureMode == MeasureMode.AtMost
            )
            {
                height = paddingAndBorderAxisColumn;
            }
            node.nodeLayout.measuredDimensions[(int)Dimension.Height] = nodeBoundAxis(
                node,
                FlexDirections.Column,
                height,
                parentHeight,
                parentWidth
            );
        }

        internal static bool nodeFixedSizeSetMeasuredDimensions(
            Node node,
            float availableWidth,
            float availableHeight,
            MeasureMode widthMeasureMode,
            MeasureMode heightMeasureMode,
            float parentWidth,
            float parentHeight
        )
        {
            if (
                (widthMeasureMode == MeasureMode.AtMost && availableWidth <= 0)
                || (heightMeasureMode == MeasureMode.AtMost && availableHeight <= 0)
                || (
                    widthMeasureMode == MeasureMode.Exactly
                    && heightMeasureMode == MeasureMode.Exactly
                )
            )
            {
                var marginAxisColumn = nodeMarginForAxis(node, FlexDirections.Column, parentWidth);
                var marginAxisRow = nodeMarginForAxis(node, FlexDirections.Row, parentWidth);

                var width = availableWidth - marginAxisRow;
                if (
                    FloatIsUndefined(availableWidth)
                    || (widthMeasureMode == MeasureMode.AtMost && availableWidth < 0)
                )
                {
                    width = 0;
                }
                node.nodeLayout.measuredDimensions[(int)Dimension.Width] = nodeBoundAxis(
                    node,
                    FlexDirections.Row,
                    width,
                    parentWidth,
                    parentWidth
                );

                var height = availableHeight - marginAxisColumn;
                if (
                    FloatIsUndefined(availableHeight)
                    || (heightMeasureMode == MeasureMode.AtMost && availableHeight < 0)
                )
                {
                    height = 0;
                }
                node.nodeLayout.measuredDimensions[(int)Dimension.Height] = nodeBoundAxis(
                    node,
                    FlexDirections.Column,
                    height,
                    parentHeight,
                    parentWidth
                );

                return true;
            }

            return false;
        }

        internal static void zeroOutLayoutRecursivly(Node node)
        {
            node.nodeLayout.Dimensions[(int)Dimension.Height] = 0;
            node.nodeLayout.Dimensions[(int)Dimension.Width] = 0;
            node.nodeLayout.Position[(int)Edge.Top] = 0;
            node.nodeLayout.Position[(int)Edge.Bottom] = 0;
            node.nodeLayout.Position[(int)Edge.Left] = 0;
            node.nodeLayout.Position[(int)Edge.Right] = 0;
            node.nodeLayout.cachedLayout.availableHeight = 0;
            node.nodeLayout.cachedLayout.availableWidth = 0;
            node.nodeLayout.cachedLayout.heightMeasureMode = MeasureMode.Exactly;
            node.nodeLayout.cachedLayout.widthMeasureMode = MeasureMode.Exactly;
            node.nodeLayout.cachedLayout.computedWidth = 0;
            node.nodeLayout.cachedLayout.computedHeight = 0;
            node.hasNewLayout = true;
            foreach (var child in node.Children)
            {
                zeroOutLayoutRecursivly(child);
            }
        }

        internal static void nodelayoutImpl(
            Node node,
            float availableWidth,
            float availableHeight,
            Directions parentDirection,
            MeasureMode widthMeasureMode,
            MeasureMode heightMeasureMode,
            float parentWidth,
            float parentHeight,
            bool performLayout,
            Config config
        )
        {
            var direction = nodeResolveDirection(node, parentDirection);
            node.nodeLayout.Direction = direction;

            var flexRowDirection = resolveFlexDirection(FlexDirections.Row, direction);
            var flexColumnDirection = resolveFlexDirection(FlexDirections.Column, direction);

            node.nodeLayout.Margin[(int)Edge.Start] = nodeLeadingMargin(
                node,
                flexRowDirection,
                parentWidth
            );
            node.nodeLayout.Margin[(int)Edge.End] = nodeTrailingMargin(
                node,
                flexRowDirection,
                parentWidth
            );
            node.nodeLayout.Margin[(int)Edge.Top] = nodeLeadingMargin(
                node,
                flexColumnDirection,
                parentWidth
            );
            node.nodeLayout.Margin[(int)Edge.Bottom] = nodeTrailingMargin(
                node,
                flexColumnDirection,
                parentWidth
            );

            node.nodeLayout.Border[(int)Edge.Start] = nodeLeadingBorder(node, flexRowDirection);
            node.nodeLayout.Border[(int)Edge.End] = nodeTrailingBorder(node, flexRowDirection);
            node.nodeLayout.Border[(int)Edge.Top] = nodeLeadingBorder(node, flexColumnDirection);
            node.nodeLayout.Border[(int)Edge.Bottom] = nodeTrailingBorder(
                node,
                flexColumnDirection
            );

            node.nodeLayout.Padding[(int)Edge.Start] = nodeLeadingPadding(
                node,
                flexRowDirection,
                parentWidth
            );
            node.nodeLayout.Padding[(int)Edge.End] = nodeTrailingPadding(
                node,
                flexRowDirection,
                parentWidth
            );
            node.nodeLayout.Padding[(int)Edge.Top] = nodeLeadingPadding(
                node,
                flexColumnDirection,
                parentWidth
            );
            node.nodeLayout.Padding[(int)Edge.Bottom] = nodeTrailingPadding(
                node,
                flexColumnDirection,
                parentWidth
            );

            if (node.measureFunc != null)
            {
                nodeWithMeasureFuncSetMeasuredDimensions(
                    node,
                    availableWidth,
                    availableHeight,
                    widthMeasureMode,
                    heightMeasureMode,
                    parentWidth,
                    parentHeight
                );
                return;
            }

            var childCount = node.Children.Count;
            if (childCount == 0)
            {
                nodeEmptyContainerSetMeasuredDimensions(
                    node,
                    availableWidth,
                    availableHeight,
                    widthMeasureMode,
                    heightMeasureMode,
                    parentWidth,
                    parentHeight
                );
                return;
            }

            if (
                !performLayout
                && nodeFixedSizeSetMeasuredDimensions(
                    node,
                    availableWidth,
                    availableHeight,
                    widthMeasureMode,
                    heightMeasureMode,
                    parentWidth,
                    parentHeight
                )
            )
            {
                return;
            }

            node.nodeLayout.HadOverflow = false;

            var mainAxis = resolveFlexDirection(node.nodeStyle.FlexDirection, direction);
            var crossAxis = flexDirectionCross(mainAxis, direction);
            var isMainAxisRow = flexDirectionIsRow(mainAxis);
            var justifyContent = node.nodeStyle.JustifyContent;
            var isNodeFlexWrap = node.nodeStyle.FlexWrap != Wrap.NoWrap;

            var mainAxisParentSize = parentHeight;
            var crossAxisParentSize = parentWidth;
            if (isMainAxisRow)
            {
                mainAxisParentSize = parentWidth;
                crossAxisParentSize = parentHeight;
            }

            Node firstAbsoluteChild = null;
            Node currentAbsoluteChild = null;

            var leadingPaddingAndBorderMain = nodeLeadingPaddingAndBorder(
                node,
                mainAxis,
                parentWidth
            );
            var trailingPaddingAndBorderMain = nodeTrailingPaddingAndBorder(
                node,
                mainAxis,
                parentWidth
            );
            var leadingPaddingAndBorderCross = nodeLeadingPaddingAndBorder(
                node,
                crossAxis,
                parentWidth
            );
            var paddingAndBorderAxisMain = nodePaddingAndBorderForAxis(node, mainAxis, parentWidth);
            var paddingAndBorderAxisCross = nodePaddingAndBorderForAxis(
                node,
                crossAxis,
                parentWidth
            );

            var measureModeMainDim = heightMeasureMode;
            var measureModeCrossDim = widthMeasureMode;

            if (isMainAxisRow)
            {
                measureModeMainDim = widthMeasureMode;
                measureModeCrossDim = heightMeasureMode;
            }

            var paddingAndBorderAxisRow = paddingAndBorderAxisCross;
            var paddingAndBorderAxisColumn = paddingAndBorderAxisMain;
            if (isMainAxisRow)
            {
                paddingAndBorderAxisRow = paddingAndBorderAxisMain;
                paddingAndBorderAxisColumn = paddingAndBorderAxisCross;
            }

            var marginAxisRow = nodeMarginForAxis(node, FlexDirections.Row, parentWidth);
            var marginAxisColumn = nodeMarginForAxis(node, FlexDirections.Column, parentWidth);

            var minInnerWidth =
                resolveValue(node.nodeStyle.MinDimensions[(int)Dimension.Width], parentWidth)
                - marginAxisRow
                - paddingAndBorderAxisRow;
            var maxInnerWidth =
                resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Width], parentWidth)
                - marginAxisRow
                - paddingAndBorderAxisRow;
            var minInnerHeight =
                resolveValue(node.nodeStyle.MinDimensions[(int)Dimension.Height], parentHeight)
                - marginAxisColumn
                - paddingAndBorderAxisColumn;
            var maxInnerHeight =
                resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Height], parentHeight)
                - marginAxisColumn
                - paddingAndBorderAxisColumn;

            var minInnerMainDim = minInnerHeight;
            var maxInnerMainDim = maxInnerHeight;
            if (isMainAxisRow)
            {
                minInnerMainDim = minInnerWidth;
                maxInnerMainDim = maxInnerWidth;
            }

            var availableInnerWidth = availableWidth - marginAxisRow - paddingAndBorderAxisRow;
            if (!FloatIsUndefined(availableInnerWidth))
            {
                availableInnerWidth = fmaxf(
                    fminf(availableInnerWidth, maxInnerWidth),
                    minInnerWidth
                );
            }

            var availableInnerHeight =
                availableHeight - marginAxisColumn - paddingAndBorderAxisColumn;
            if (!FloatIsUndefined(availableInnerHeight))
            {
                availableInnerHeight = fmaxf(
                    fminf(availableInnerHeight, maxInnerHeight),
                    minInnerHeight
                );
            }

            var availableInnerMainDim = availableInnerHeight;
            var availableInnerCrossDim = availableInnerWidth;
            if (isMainAxisRow)
            {
                availableInnerMainDim = availableInnerWidth;
                availableInnerCrossDim = availableInnerHeight;
            }

            Node singleFlexChild = null;
            if (measureModeMainDim == MeasureMode.Exactly)
            {
                foreach (var child in node.Children)
                {
                    if (singleFlexChild != null)
                    {
                        if (nodeIsFlex(child))
                        {
                            singleFlexChild = null;
                            break;
                        }
                    }
                    else if (resolveFlexGrow(child) > 0 && nodeResolveFlexShrink(child) > 0)
                    {
                        singleFlexChild = child;
                    }
                }
            }

            float totalOuterFlexBasis = 0;

            foreach (var child in node.Children)
            {
                if (child.nodeStyle.Display == Displays.None)
                {
                    zeroOutLayoutRecursivly(child);
                    child.hasNewLayout = true;
                    child.IsDirty = false;
                    continue;
                }
                resolveDimensions(child);
                if (performLayout)
                {
                    var childDirection = nodeResolveDirection(child, direction);
                    nodeSetPosition(
                        child,
                        childDirection,
                        availableInnerMainDim,
                        availableInnerCrossDim,
                        availableInnerWidth
                    );
                }

                if (child.nodeStyle.PositionType == PositionTypes.Absolute)
                {
                    if (firstAbsoluteChild == null)
                    {
                        firstAbsoluteChild = child;
                    }
                    if (currentAbsoluteChild != null)
                    {
                        currentAbsoluteChild.NextChild = child;
                    }
                    currentAbsoluteChild = child;
                    child.NextChild = null;
                }
                else
                {
                    if (child == singleFlexChild)
                    {
                        child.nodeLayout.computedFlexBasisGeneration = currentGenerationCount;
                        child.nodeLayout.computedFlexBasis = 0;
                    }
                    else
                    {
                        nodeComputeFlexBasisForChild(
                            node,
                            child,
                            availableInnerWidth,
                            widthMeasureMode,
                            availableInnerHeight,
                            availableInnerWidth,
                            availableInnerHeight,
                            heightMeasureMode,
                            direction,
                            config
                        );
                    }
                    ;
                }

                totalOuterFlexBasis +=
                    child.nodeLayout.computedFlexBasis
                    + nodeMarginForAxis(child, mainAxis, availableInnerWidth);
            }

            var flexBasisOverflows = totalOuterFlexBasis > availableInnerMainDim;
            if (measureModeMainDim == MeasureMode.Undefined)
            {
                flexBasisOverflows = false;
            }
            if (isNodeFlexWrap && flexBasisOverflows && measureModeMainDim == MeasureMode.AtMost)
            {
                measureModeMainDim = MeasureMode.Exactly;
            }

            int startOfLineIndex = 0;
            int endOfLineIndex = 0;

            int lineCount = 0;

            float totalLineCrossDim = 0;

            float maxLineMainDim = 0;

            while (endOfLineIndex < childCount)
            {
                int itemsOnLine = 0;

                float sizeConsumedOnCurrentLine = 0;
                float sizeConsumedOnCurrentLineIncludingMinConstraint = 0;

                float totalFlexGrowFactors = 0;
                float totalFlexShrinkScaledFactors = 0;

                Node firstRelativeChild = null;
                Node currentRelativeChild = null;

                for (int i = startOfLineIndex; i < childCount; i++)
                {
                    var child = node.Children[i];
                    if (child.nodeStyle.Display == Displays.None)
                    {
                        endOfLineIndex++;
                        continue;
                    }
                    child.lineIndex = lineCount;

                    if (child.nodeStyle.PositionType != PositionTypes.Absolute)
                    {
                        var childMarginMainAxis = nodeMarginForAxis(
                            child,
                            mainAxis,
                            availableInnerWidth
                        );
                        var flexBasisWithMaxConstraints = fminf(
                            resolveValue(
                                child.nodeStyle.MaxDimensions[(int)dim[(int)mainAxis]],
                                mainAxisParentSize
                            ),
                            child.nodeLayout.computedFlexBasis
                        );
                        var flexBasisWithMinAndMaxConstraints = fmaxf(
                            resolveValue(
                                child.nodeStyle.MinDimensions[(int)dim[(int)mainAxis]],
                                mainAxisParentSize
                            ),
                            flexBasisWithMaxConstraints
                        );

                        if (
                            sizeConsumedOnCurrentLineIncludingMinConstraint
                                + flexBasisWithMinAndMaxConstraints
                                + childMarginMainAxis
                                > availableInnerMainDim
                            && isNodeFlexWrap
                            && itemsOnLine > 0
                        )
                        {
                            break;
                        }

                        sizeConsumedOnCurrentLineIncludingMinConstraint +=
                            flexBasisWithMinAndMaxConstraints + childMarginMainAxis;
                        sizeConsumedOnCurrentLine +=
                            flexBasisWithMinAndMaxConstraints + childMarginMainAxis;
                        itemsOnLine++;

                        if (nodeIsFlex(child))
                        {
                            totalFlexGrowFactors += resolveFlexGrow(child);

                            totalFlexShrinkScaledFactors +=
                                -nodeResolveFlexShrink(child) * child.nodeLayout.computedFlexBasis;
                        }

                        if (firstRelativeChild == null)
                        {
                            firstRelativeChild = child;
                        }
                        if (currentRelativeChild != null)
                        {
                            currentRelativeChild.NextChild = child;
                        }
                        currentRelativeChild = child;
                        child.NextChild = null;
                    }
                    endOfLineIndex++;
                }

                if (totalFlexGrowFactors > 0 && totalFlexGrowFactors < 1)
                {
                    totalFlexGrowFactors = 1;
                }

                if (totalFlexShrinkScaledFactors > 0 && totalFlexShrinkScaledFactors < 1)
                {
                    totalFlexShrinkScaledFactors = 1;
                }

                var canSkipFlex = !performLayout && measureModeCrossDim == MeasureMode.Exactly;

                float leadingMainDim = 0;
                float betweenMainDim = 0;

                if (measureModeMainDim != MeasureMode.Exactly)
                {
                    if (
                        !FloatIsUndefined(minInnerMainDim)
                        && sizeConsumedOnCurrentLine < minInnerMainDim
                    )
                    {
                        availableInnerMainDim = minInnerMainDim;
                    }
                    else if (
                        !FloatIsUndefined(maxInnerMainDim)
                        && sizeConsumedOnCurrentLine > maxInnerMainDim
                    )
                    {
                        availableInnerMainDim = maxInnerMainDim;
                    }
                    else
                    {
                        if (
                            !node.config.UseLegacyStretchBehaviour
                            && (totalFlexGrowFactors == 0 || resolveFlexGrow(node) == 0)
                        )
                        {
                            availableInnerMainDim = sizeConsumedOnCurrentLine;
                        }
                    }
                }

                float remainingFreeSpace = 0;
                if (!FloatIsUndefined(availableInnerMainDim))
                {
                    remainingFreeSpace = availableInnerMainDim - sizeConsumedOnCurrentLine;
                }
                else if (sizeConsumedOnCurrentLine < 0)
                {
                    remainingFreeSpace = -sizeConsumedOnCurrentLine;
                }

                float originalRemainingFreeSpace = remainingFreeSpace;
                float deltaFreeSpace = 0;

                if (!canSkipFlex)
                {
                    float childFlexBasis;
                    float flexShrinkScaledFactor;
                    float flexGrowFactor;
                    float baseMainSize;
                    float boundMainSize;

                    float deltaFlexShrinkScaledFactors = 0;
                    float deltaFlexGrowFactors = 0;
                    currentRelativeChild = firstRelativeChild;
                    while (currentRelativeChild != null)
                    {
                        childFlexBasis = fminf(
                            resolveValue(
                                currentRelativeChild.nodeStyle.MaxDimensions[
                                    (int)dim[(int)mainAxis]
                                ],
                                mainAxisParentSize
                            ),
                            fmaxf(
                                resolveValue(
                                    currentRelativeChild.nodeStyle.MinDimensions[
                                        (int)dim[(int)mainAxis]
                                    ],
                                    mainAxisParentSize
                                ),
                                currentRelativeChild.nodeLayout.computedFlexBasis
                            )
                        );

                        if (remainingFreeSpace < 0)
                        {
                            flexShrinkScaledFactor =
                                -nodeResolveFlexShrink(currentRelativeChild) * childFlexBasis;

                            if (flexShrinkScaledFactor != 0)
                            {
                                baseMainSize =
                                    childFlexBasis
                                    + remainingFreeSpace
                                        / totalFlexShrinkScaledFactors
                                        * flexShrinkScaledFactor;
                                boundMainSize = nodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    baseMainSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                                if (baseMainSize != boundMainSize)
                                {
                                    deltaFreeSpace -= boundMainSize - childFlexBasis;
                                    deltaFlexShrinkScaledFactors -= flexShrinkScaledFactor;
                                }
                            }
                        }
                        else if (remainingFreeSpace > 0)
                        {
                            flexGrowFactor = resolveFlexGrow(currentRelativeChild);

                            if (flexGrowFactor != 0)
                            {
                                baseMainSize =
                                    childFlexBasis
                                    + remainingFreeSpace / totalFlexGrowFactors * flexGrowFactor;
                                boundMainSize = nodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    baseMainSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );

                                if (baseMainSize != boundMainSize)
                                {
                                    deltaFreeSpace -= boundMainSize - childFlexBasis;
                                    deltaFlexGrowFactors -= flexGrowFactor;
                                }
                            }
                        }

                        currentRelativeChild = currentRelativeChild.NextChild;
                    }

                    totalFlexShrinkScaledFactors += deltaFlexShrinkScaledFactors;
                    totalFlexGrowFactors += deltaFlexGrowFactors;
                    remainingFreeSpace += deltaFreeSpace;

                    deltaFreeSpace = 0;
                    currentRelativeChild = firstRelativeChild;
                    while (currentRelativeChild != null)
                    {
                        childFlexBasis = fminf(
                            resolveValue(
                                currentRelativeChild.nodeStyle.MaxDimensions[
                                    (int)dim[(int)mainAxis]
                                ],
                                mainAxisParentSize
                            ),
                            fmaxf(
                                resolveValue(
                                    currentRelativeChild.nodeStyle.MinDimensions[
                                        (int)dim[(int)mainAxis]
                                    ],
                                    mainAxisParentSize
                                ),
                                currentRelativeChild.nodeLayout.computedFlexBasis
                            )
                        );
                        float updatedMainSize = childFlexBasis;

                        if (remainingFreeSpace < 0)
                        {
                            flexShrinkScaledFactor =
                                -nodeResolveFlexShrink(currentRelativeChild) * childFlexBasis;

                            if (flexShrinkScaledFactor != 0)
                            {
                                float childSize = 0;

                                if (totalFlexShrinkScaledFactors == 0)
                                {
                                    childSize = childFlexBasis + flexShrinkScaledFactor;
                                }
                                else
                                {
                                    childSize =
                                        childFlexBasis
                                        + (remainingFreeSpace / totalFlexShrinkScaledFactors)
                                            * flexShrinkScaledFactor;
                                }

                                updatedMainSize = nodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    childSize,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                            }
                        }
                        else if (remainingFreeSpace > 0)
                        {
                            flexGrowFactor = resolveFlexGrow(currentRelativeChild);

                            if (flexGrowFactor != 0)
                            {
                                updatedMainSize = nodeBoundAxis(
                                    currentRelativeChild,
                                    mainAxis,
                                    childFlexBasis
                                        + remainingFreeSpace
                                            / totalFlexGrowFactors
                                            * flexGrowFactor,
                                    availableInnerMainDim,
                                    availableInnerWidth
                                );
                            }
                        }

                        deltaFreeSpace -= updatedMainSize - childFlexBasis;

                        var marginMain = nodeMarginForAxis(
                            currentRelativeChild,
                            mainAxis,
                            availableInnerWidth
                        );
                        var marginCross = nodeMarginForAxis(
                            currentRelativeChild,
                            crossAxis,
                            availableInnerWidth
                        );

                        float childCrossSize = 0;
                        float childMainSize = updatedMainSize + marginMain;
                        MeasureMode childCrossMeasureMode = MeasureMode.Undefined;
                        var childMainMeasureMode = MeasureMode.Exactly;

                        if (
                            !FloatIsUndefined(availableInnerCrossDim)
                            && !nodeIsStyleDimDefined(
                                currentRelativeChild,
                                crossAxis,
                                availableInnerCrossDim
                            )
                            && measureModeCrossDim == MeasureMode.Exactly
                            && !isNodeFlexWrap
                            && nodeAlignItem(node, currentRelativeChild) == Align.Stretch
                        )
                        {
                            childCrossSize = availableInnerCrossDim;
                            childCrossMeasureMode = MeasureMode.Exactly;
                        }
                        else if (
                            !nodeIsStyleDimDefined(
                                currentRelativeChild,
                                crossAxis,
                                availableInnerCrossDim
                            )
                        )
                        {
                            childCrossSize = availableInnerCrossDim;
                            childCrossMeasureMode = MeasureMode.AtMost;
                            if (FloatIsUndefined(childCrossSize))
                            {
                                childCrossMeasureMode = MeasureMode.Undefined;
                            }
                        }
                        else
                        {
                            childCrossSize =
                                resolveValue(
                                    currentRelativeChild.resolvedDimensions[
                                        (int)dim[(int)crossAxis]
                                    ],
                                    availableInnerCrossDim
                                ) + marginCross;
                            var isLoosePercentageMeasurement =
                                currentRelativeChild
                                    .resolvedDimensions[(int)dim[(int)crossAxis]]
                                    .unit == Unit.Percent
                                && measureModeCrossDim != MeasureMode.Exactly;
                            childCrossMeasureMode = MeasureMode.Exactly;
                            if (FloatIsUndefined(childCrossSize) || isLoosePercentageMeasurement)
                            {
                                childCrossMeasureMode = MeasureMode.Undefined;
                            }
                        }

                        if (!FloatIsUndefined(currentRelativeChild.nodeStyle.AspectRatio))
                        {
                            float v =
                                (childMainSize - marginMain)
                                * currentRelativeChild.nodeStyle.AspectRatio;
                            if (isMainAxisRow)
                            {
                                v =
                                    (childMainSize - marginMain)
                                    / currentRelativeChild.nodeStyle.AspectRatio;
                            }
                            childCrossSize = fmaxf(
                                v,
                                nodePaddingAndBorderForAxis(
                                    currentRelativeChild,
                                    crossAxis,
                                    availableInnerWidth
                                )
                            );
                            childCrossMeasureMode = MeasureMode.Exactly;

                            if (nodeIsFlex(currentRelativeChild))
                            {
                                childCrossSize = fminf(
                                    childCrossSize - marginCross,
                                    availableInnerCrossDim
                                );
                                childMainSize = marginMain;
                                if (isMainAxisRow)
                                {
                                    childMainSize +=
                                        childCrossSize * currentRelativeChild.nodeStyle.AspectRatio;
                                }
                                else
                                {
                                    childMainSize +=
                                        childCrossSize / currentRelativeChild.nodeStyle.AspectRatio;
                                }
                            }

                            childCrossSize += marginCross;
                        }

                        constrainMaxSizeForMode(
                            currentRelativeChild,
                            mainAxis,
                            availableInnerMainDim,
                            availableInnerWidth,
                            ref childMainMeasureMode,
                            ref childMainSize
                        );
                        constrainMaxSizeForMode(
                            currentRelativeChild,
                            crossAxis,
                            availableInnerCrossDim,
                            availableInnerWidth,
                            ref childCrossMeasureMode,
                            ref childCrossSize
                        );

                        var requiresStretchLayout =
                            !nodeIsStyleDimDefined(
                                currentRelativeChild,
                                crossAxis,
                                availableInnerCrossDim
                            )
                            && nodeAlignItem(node, currentRelativeChild) == Align.Stretch;

                        float childWidth = childCrossSize;
                        if (isMainAxisRow)
                        {
                            childWidth = childMainSize;
                        }
                        float childHeight = childCrossSize;
                        if (!isMainAxisRow)
                        {
                            childHeight = childMainSize;
                        }

                        var childWidthMeasureMode = childCrossMeasureMode;
                        if (isMainAxisRow)
                        {
                            childWidthMeasureMode = childMainMeasureMode;
                        }
                        var childHeightMeasureMode = childCrossMeasureMode;
                        if (!isMainAxisRow)
                        {
                            childHeightMeasureMode = childMainMeasureMode;
                        }

                        layoutNodeInternal(
                            currentRelativeChild,
                            childWidth,
                            childHeight,
                            direction,
                            childWidthMeasureMode,
                            childHeightMeasureMode,
                            availableInnerWidth,
                            availableInnerHeight,
                            performLayout && !requiresStretchLayout,
                            "flex",
                            config
                        );
                        if (currentRelativeChild.nodeLayout.HadOverflow)
                        {
                            node.nodeLayout.HadOverflow = true;
                        }

                        currentRelativeChild = currentRelativeChild.NextChild;
                    }
                }

                remainingFreeSpace = originalRemainingFreeSpace + deltaFreeSpace;
                if (remainingFreeSpace < 0)
                {
                    node.nodeLayout.HadOverflow = true;
                }

                if (measureModeMainDim == MeasureMode.AtMost && remainingFreeSpace > 0)
                {
                    if (
                        node.nodeStyle.MinDimensions[(int)dim[(int)mainAxis]].unit != Unit.Undefined
                        && resolveValue(
                            node.nodeStyle.MinDimensions[(int)dim[(int)mainAxis]],
                            mainAxisParentSize
                        ) >= 0
                    )
                    {
                        remainingFreeSpace = fmaxf(
                            0,
                            resolveValue(
                                node.nodeStyle.MinDimensions[(int)dim[(int)mainAxis]],
                                mainAxisParentSize
                            ) - (availableInnerMainDim - remainingFreeSpace)
                        );
                    }
                    else
                    {
                        remainingFreeSpace = 0;
                    }
                }

                int numberOfAutoMarginsOnCurrentLine = 0;
                for (int i = startOfLineIndex; i < endOfLineIndex; i++)
                {
                    var child = node.Children[i];
                    if (child.nodeStyle.PositionType == PositionTypes.Relative)
                    {
                        if (marginLeadingValue(child, mainAxis).unit == Unit.Auto)
                        {
                            numberOfAutoMarginsOnCurrentLine++;
                        }
                        if (marginTrailingValue(child, mainAxis).unit == Unit.Auto)
                        {
                            numberOfAutoMarginsOnCurrentLine++;
                        }
                    }
                }

                if (numberOfAutoMarginsOnCurrentLine == 0)
                {
                    switch (justifyContent)
                    {
                        case Justify.Center:
                            leadingMainDim = remainingFreeSpace / 2;
                            break;
                        case Justify.FlexEnd:
                            leadingMainDim = remainingFreeSpace;
                            break;
                        case Justify.SpaceBetween:
                            if (itemsOnLine > 1)
                            {
                                betweenMainDim = fmaxf(remainingFreeSpace, 0) / (itemsOnLine - 1);
                            }
                            else
                            {
                                betweenMainDim = 0;
                            }
                            break;
                        case Justify.SpaceAround:

                            betweenMainDim = remainingFreeSpace / itemsOnLine;
                            leadingMainDim = betweenMainDim / 2;
                            break;
                        case Justify.FlexStart:
                            break;
                    }
                }

                float mainDim = leadingPaddingAndBorderMain + leadingMainDim;
                float crossDim = 0;

                for (int i = startOfLineIndex; i < endOfLineIndex; i++)
                {
                    var child = node.Children[i];
                    if (child.nodeStyle.Display == Displays.None)
                    {
                        continue;
                    }
                    if (
                        child.nodeStyle.PositionType == PositionTypes.Absolute
                        && nodeIsLeadingPosDefined(child, mainAxis)
                    )
                    {
                        if (performLayout)
                        {
                            child.nodeLayout.Position[(int)pos[(int)mainAxis]] =
                                nodeLeadingPosition(child, mainAxis, availableInnerMainDim)
                                + nodeLeadingBorder(node, mainAxis)
                                + nodeLeadingMargin(child, mainAxis, availableInnerWidth);
                        }
                    }
                    else
                    {
                        if (child.nodeStyle.PositionType == PositionTypes.Relative)
                        {
                            if (marginLeadingValue(child, mainAxis).unit == Unit.Auto)
                            {
                                mainDim += remainingFreeSpace / numberOfAutoMarginsOnCurrentLine;
                            }

                            if (performLayout)
                            {
                                child.nodeLayout.Position[(int)pos[(int)mainAxis]] += mainDim;
                            }

                            if (marginTrailingValue(child, mainAxis).unit == Unit.Auto)
                            {
                                mainDim += remainingFreeSpace / numberOfAutoMarginsOnCurrentLine;
                            }

                            if (canSkipFlex)
                            {
                                mainDim +=
                                    betweenMainDim
                                    + nodeMarginForAxis(child, mainAxis, availableInnerWidth)
                                    + child.nodeLayout.computedFlexBasis;
                                crossDim = availableInnerCrossDim;
                            }
                            else
                            {
                                mainDim +=
                                    betweenMainDim
                                    + nodeDimWithMargin(child, mainAxis, availableInnerWidth);

                                crossDim = fmaxf(
                                    crossDim,
                                    nodeDimWithMargin(child, crossAxis, availableInnerWidth)
                                );
                            }
                        }
                        else if (performLayout)
                        {
                            child.nodeLayout.Position[(int)pos[(int)mainAxis]] +=
                                nodeLeadingBorder(node, mainAxis) + leadingMainDim;
                        }
                    }
                }

                mainDim += trailingPaddingAndBorderMain;

                float containerCrossAxis = availableInnerCrossDim;
                if (
                    measureModeCrossDim == MeasureMode.Undefined
                    || measureModeCrossDim == MeasureMode.AtMost
                )
                {
                    containerCrossAxis =
                        nodeBoundAxis(
                            node,
                            crossAxis,
                            crossDim + paddingAndBorderAxisCross,
                            crossAxisParentSize,
                            parentWidth
                        ) - paddingAndBorderAxisCross;
                }

                if (!isNodeFlexWrap && measureModeCrossDim == MeasureMode.Exactly)
                {
                    crossDim = availableInnerCrossDim;
                }

                crossDim =
                    nodeBoundAxis(
                        node,
                        crossAxis,
                        crossDim + paddingAndBorderAxisCross,
                        crossAxisParentSize,
                        parentWidth
                    ) - paddingAndBorderAxisCross;

                if (performLayout)
                {
                    for (int i = startOfLineIndex; i < endOfLineIndex; i++)
                    {
                        var child = node.Children[i];
                        if (child.nodeStyle.Display == Displays.None)
                        {
                            continue;
                        }
                        if (child.nodeStyle.PositionType == PositionTypes.Absolute)
                        {
                            if (nodeIsLeadingPosDefined(child, crossAxis))
                            {
                                child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                    nodeLeadingPosition(child, crossAxis, availableInnerCrossDim)
                                    + nodeLeadingBorder(node, crossAxis)
                                    + nodeLeadingMargin(child, crossAxis, availableInnerWidth);
                            }
                            else
                            {
                                child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                    nodeLeadingBorder(node, crossAxis)
                                    + nodeLeadingMargin(child, crossAxis, availableInnerWidth);
                            }
                        }
                        else
                        {
                            float leadingCrossDim = leadingPaddingAndBorderCross;

                            var alignItem = nodeAlignItem(node, child);

                            if (
                                alignItem == Align.Stretch
                                && marginLeadingValue(child, crossAxis).unit != Unit.Auto
                                && marginTrailingValue(child, crossAxis).unit != Unit.Auto
                            )
                            {
                                if (
                                    !nodeIsStyleDimDefined(child, crossAxis, availableInnerCrossDim)
                                )
                                {
                                    float childMainSize = child.nodeLayout.measuredDimensions[
                                        (int)dim[(int)mainAxis]
                                    ];
                                    float childCrossSize = crossDim;
                                    if (!FloatIsUndefined(child.nodeStyle.AspectRatio))
                                    {
                                        childCrossSize = nodeMarginForAxis(
                                            child,
                                            crossAxis,
                                            availableInnerWidth
                                        );
                                        if (isMainAxisRow)
                                        {
                                            childCrossSize +=
                                                childMainSize / child.nodeStyle.AspectRatio;
                                        }
                                        else
                                        {
                                            childCrossSize +=
                                                childMainSize * child.nodeStyle.AspectRatio;
                                        }
                                    }

                                    childMainSize += nodeMarginForAxis(
                                        child,
                                        mainAxis,
                                        availableInnerWidth
                                    );

                                    var childMainMeasureMode = MeasureMode.Exactly;
                                    var childCrossMeasureMode = MeasureMode.Exactly;
                                    constrainMaxSizeForMode(
                                        child,
                                        mainAxis,
                                        availableInnerMainDim,
                                        availableInnerWidth,
                                        ref childMainMeasureMode,
                                        ref childMainSize
                                    );
                                    constrainMaxSizeForMode(
                                        child,
                                        crossAxis,
                                        availableInnerCrossDim,
                                        availableInnerWidth,
                                        ref childCrossMeasureMode,
                                        ref childCrossSize
                                    );

                                    float childWidth = childCrossSize;
                                    if (isMainAxisRow)
                                    {
                                        childWidth = childMainSize;
                                    }
                                    float childHeight = childCrossSize;
                                    if (!isMainAxisRow)
                                    {
                                        childHeight = childMainSize;
                                    }

                                    var childWidthMeasureMode = MeasureMode.Exactly;
                                    if (FloatIsUndefined(childWidth))
                                    {
                                        childWidthMeasureMode = MeasureMode.Undefined;
                                    }

                                    var childHeightMeasureMode = MeasureMode.Exactly;
                                    if (FloatIsUndefined(childHeight))
                                    {
                                        childHeightMeasureMode = MeasureMode.Undefined;
                                    }

                                    layoutNodeInternal(
                                        child,
                                        childWidth,
                                        childHeight,
                                        direction,
                                        childWidthMeasureMode,
                                        childHeightMeasureMode,
                                        availableInnerWidth,
                                        availableInnerHeight,
                                        true,
                                        "stretch",
                                        config
                                    );
                                }
                            }
                            else
                            {
                                float remainingCrossDim =
                                    containerCrossAxis
                                    - nodeDimWithMargin(child, crossAxis, availableInnerWidth);

                                if (
                                    marginLeadingValue(child, crossAxis).unit == Unit.Auto
                                    && marginTrailingValue(child, crossAxis).unit == Unit.Auto
                                )
                                {
                                    leadingCrossDim += fmaxf(0, remainingCrossDim / 2);
                                }
                                else if (marginTrailingValue(child, crossAxis).unit == Unit.Auto)
                                { }
                                else if (marginLeadingValue(child, crossAxis).unit == Unit.Auto)
                                {
                                    leadingCrossDim += fmaxf(0, remainingCrossDim);
                                }
                                else if (alignItem == Align.FlexStart) { }
                                else if (alignItem == Align.Center)
                                {
                                    leadingCrossDim += remainingCrossDim / 2;
                                }
                                else
                                {
                                    leadingCrossDim += remainingCrossDim;
                                }
                            }

                            child.nodeLayout.Position[(int)pos[(int)crossAxis]] +=
                                totalLineCrossDim + leadingCrossDim;
                        }
                    }
                }

                totalLineCrossDim += crossDim;
                maxLineMainDim = fmaxf(maxLineMainDim, mainDim);

                lineCount++;
                startOfLineIndex = endOfLineIndex;
            }

            if (
                performLayout
                && (lineCount > 1 || isBaselineLayout(node))
                && !FloatIsUndefined(availableInnerCrossDim)
            )
            {
                float remainingAlignContentDim = availableInnerCrossDim - totalLineCrossDim;

                float crossDimLead = 0;
                float currentLead = leadingPaddingAndBorderCross;

                switch (node.nodeStyle.AlignContent)
                {
                    case Align.FlexEnd:
                        currentLead += remainingAlignContentDim;
                        break;
                    case Align.Center:
                        currentLead += remainingAlignContentDim / 2;
                        break;
                    case Align.Stretch:
                        if (availableInnerCrossDim > totalLineCrossDim)
                        {
                            crossDimLead = remainingAlignContentDim / lineCount;
                        }
                        break;
                    case Align.SpaceAround:
                        if (availableInnerCrossDim > totalLineCrossDim)
                        {
                            currentLead += remainingAlignContentDim / (2 * lineCount);
                            if (lineCount > 1)
                            {
                                crossDimLead = remainingAlignContentDim / lineCount;
                            }
                        }
                        else
                        {
                            currentLead += remainingAlignContentDim / 2;
                        }
                        break;
                    case Align.SpaceBetween:
                        if (availableInnerCrossDim > totalLineCrossDim && lineCount > 1)
                        {
                            crossDimLead = remainingAlignContentDim / (lineCount - 1);
                        }
                        break;
                    case Align.Auto:
                    case Align.FlexStart:
                    case Align.Baseline:
                        break;
                }

                int endIndex = 0;
                for (int i = 0; i < lineCount; i++)
                {
                    int startIndex = endIndex;
                    int ii = 0;

                    float lineHeight = 0;
                    float maxAscentForCurrentLine = 0;
                    float maxDescentForCurrentLine = 0;
                    for (ii = startIndex; ii < childCount; ii++)
                    {
                        var child = node.Children[ii];
                        if (child.nodeStyle.Display == Displays.None)
                        {
                            continue;
                        }
                        if (child.nodeStyle.PositionType == PositionTypes.Relative)
                        {
                            if (child.lineIndex != i)
                            {
                                break;
                            }
                            if (nodeIsLayoutDimDefined(child, crossAxis))
                            {
                                lineHeight = fmaxf(
                                    lineHeight,
                                    child.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                                        + nodeMarginForAxis(child, crossAxis, availableInnerWidth)
                                );
                            }
                            if (nodeAlignItem(node, child) == Align.Baseline)
                            {
                                float ascent =
                                    Baseline(child)
                                    + nodeLeadingMargin(
                                        child,
                                        FlexDirections.Column,
                                        availableInnerWidth
                                    );
                                float descent =
                                    child.nodeLayout.measuredDimensions[(int)Dimension.Height]
                                    + nodeMarginForAxis(
                                        child,
                                        FlexDirections.Column,
                                        availableInnerWidth
                                    )
                                    - ascent;
                                maxAscentForCurrentLine = fmaxf(maxAscentForCurrentLine, ascent);
                                maxDescentForCurrentLine = fmaxf(maxDescentForCurrentLine, descent);
                                lineHeight = fmaxf(
                                    lineHeight,
                                    maxAscentForCurrentLine + maxDescentForCurrentLine
                                );
                            }
                        }
                    }
                    endIndex = ii;
                    lineHeight += crossDimLead;

                    if (performLayout)
                    {
                        for (ii = startIndex; ii < endIndex; ii++)
                        {
                            var child = node.Children[ii];
                            if (child.nodeStyle.Display == Displays.None)
                            {
                                continue;
                            }
                            if (child.nodeStyle.PositionType == PositionTypes.Relative)
                            {
                                switch (nodeAlignItem(node, child))
                                {
                                    case Align.FlexStart:
                                        {
                                            child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                                currentLead
                                                + nodeLeadingMargin(
                                                    child,
                                                    crossAxis,
                                                    availableInnerWidth
                                                );
                                        }
                                        break;
                                    case Align.FlexEnd:
                                        {
                                            child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                                currentLead
                                                + lineHeight
                                                - nodeTrailingMargin(
                                                    child,
                                                    crossAxis,
                                                    availableInnerWidth
                                                )
                                                - child.nodeLayout.measuredDimensions[
                                                    (int)dim[(int)crossAxis]
                                                ];
                                        }
                                        break;
                                    case Align.Center:
                                        {
                                            float childHeight = child.nodeLayout.measuredDimensions[
                                                (int)dim[(int)crossAxis]
                                            ];
                                            child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                                currentLead + (lineHeight - childHeight) / 2;
                                        }
                                        break;
                                    case Align.Stretch:
                                        {
                                            child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                                                currentLead
                                                + nodeLeadingMargin(
                                                    child,
                                                    crossAxis,
                                                    availableInnerWidth
                                                );

                                            if (
                                                !nodeIsStyleDimDefined(
                                                    child,
                                                    crossAxis,
                                                    availableInnerCrossDim
                                                )
                                            )
                                            {
                                                float childWidth;
                                                float childHeight;

                                                if (isMainAxisRow)
                                                {
                                                    childWidth =
                                                        child.nodeLayout.measuredDimensions[
                                                            (int)Dimension.Width
                                                        ]
                                                        + nodeMarginForAxis(
                                                            child,
                                                            mainAxis,
                                                            availableInnerWidth
                                                        );
                                                    childHeight = lineHeight;
                                                }
                                                else
                                                {
                                                    childHeight =
                                                        child.nodeLayout.measuredDimensions[
                                                            (int)Dimension.Height
                                                        ]
                                                        + nodeMarginForAxis(
                                                            child,
                                                            mainAxis,
                                                            availableInnerWidth
                                                        );
                                                    childWidth = lineHeight;
                                                }

                                                if (
                                                    !(
                                                        FloatsEqual(
                                                            childWidth,
                                                            child.nodeLayout.measuredDimensions[
                                                                (int)Dimension.Width
                                                            ]
                                                        )
                                                        && FloatsEqual(
                                                            childHeight,
                                                            child.nodeLayout.measuredDimensions[
                                                                (int)Dimension.Height
                                                            ]
                                                        )
                                                    )
                                                )
                                                {
                                                    layoutNodeInternal(
                                                        child,
                                                        childWidth,
                                                        childHeight,
                                                        direction,
                                                        MeasureMode.Exactly,
                                                        MeasureMode.Exactly,
                                                        availableInnerWidth,
                                                        availableInnerHeight,
                                                        true,
                                                        "multiline-stretch",
                                                        config
                                                    );
                                                }
                                            }
                                        }
                                        break;
                                    case Align.Baseline:
                                        {
                                            child.nodeLayout.Position[(int)Edge.Top] =
                                                currentLead
                                                + maxAscentForCurrentLine
                                                - Baseline(child)
                                                + nodeLeadingPosition(
                                                    child,
                                                    FlexDirections.Column,
                                                    availableInnerCrossDim
                                                );
                                        }
                                        break;
                                    case Align.Auto:
                                    case Align.SpaceBetween:
                                    case Align.SpaceAround:
                                        break;
                                }
                            }
                        }
                    }

                    currentLead += lineHeight;
                }
            }

            node.nodeLayout.measuredDimensions[(int)Dimension.Width] = nodeBoundAxis(
                node,
                FlexDirections.Row,
                availableWidth - marginAxisRow,
                parentWidth,
                parentWidth
            );
            node.nodeLayout.measuredDimensions[(int)Dimension.Height] = nodeBoundAxis(
                node,
                FlexDirections.Column,
                availableHeight - marginAxisColumn,
                parentHeight,
                parentWidth
            );

            if (
                measureModeMainDim == MeasureMode.Undefined
                || (
                    node.nodeStyle.Overflow != Overflows.Scroll
                    && measureModeMainDim == MeasureMode.AtMost
                )
            )
            {
                node.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]] = nodeBoundAxis(
                    node,
                    mainAxis,
                    maxLineMainDim,
                    mainAxisParentSize,
                    parentWidth
                );
            }
            else if (
                measureModeMainDim == MeasureMode.AtMost
                && node.nodeStyle.Overflow == Overflows.Scroll
            )
            {
                node.nodeLayout.measuredDimensions[(int)dim[(int)mainAxis]] = fmaxf(
                    fminf(
                        availableInnerMainDim + paddingAndBorderAxisMain,
                        nodeBoundAxisWithinMinAndMax(
                            node,
                            mainAxis,
                            maxLineMainDim,
                            mainAxisParentSize
                        )
                    ),
                    paddingAndBorderAxisMain
                );
            }

            if (
                measureModeCrossDim == MeasureMode.Undefined
                || (
                    node.nodeStyle.Overflow != Overflows.Scroll
                    && measureModeCrossDim == MeasureMode.AtMost
                )
            )
            {
                node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]] = nodeBoundAxis(
                    node,
                    crossAxis,
                    totalLineCrossDim + paddingAndBorderAxisCross,
                    crossAxisParentSize,
                    parentWidth
                );
            }
            else if (
                measureModeCrossDim == MeasureMode.AtMost
                && node.nodeStyle.Overflow == Overflows.Scroll
            )
            {
                node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]] = fmaxf(
                    fminf(
                        availableInnerCrossDim + paddingAndBorderAxisCross,
                        nodeBoundAxisWithinMinAndMax(
                            node,
                            crossAxis,
                            totalLineCrossDim + paddingAndBorderAxisCross,
                            crossAxisParentSize
                        )
                    ),
                    paddingAndBorderAxisCross
                );
            }

            if (performLayout && node.nodeStyle.FlexWrap == Wrap.WrapReverse)
            {
                foreach (var child in node.Children)
                {
                    if (child.nodeStyle.PositionType == PositionTypes.Relative)
                    {
                        child.nodeLayout.Position[(int)pos[(int)crossAxis]] =
                            node.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]]
                            - child.nodeLayout.Position[(int)pos[(int)crossAxis]]
                            - child.nodeLayout.measuredDimensions[(int)dim[(int)crossAxis]];
                    }
                }
            }

            if (performLayout)
            {
                for (
                    currentAbsoluteChild = firstAbsoluteChild;
                    currentAbsoluteChild != null;
                    currentAbsoluteChild = currentAbsoluteChild.NextChild
                )
                {
                    var mode = measureModeCrossDim;
                    if (isMainAxisRow)
                    {
                        mode = measureModeMainDim;
                    }

                    nodeAbsoluteLayoutChild(
                        node,
                        currentAbsoluteChild,
                        availableInnerWidth,
                        mode,
                        availableInnerHeight,
                        direction,
                        config
                    );
                }

                var needsMainTrailingPos =
                    mainAxis == FlexDirections.RowReverse
                    || mainAxis == FlexDirections.ColumnReverse;
                var needsCrossTrailingPos =
                    crossAxis == FlexDirections.RowReverse
                    || crossAxis == FlexDirections.ColumnReverse;

                if (needsMainTrailingPos || needsCrossTrailingPos)
                {
                    foreach (var child in node.Children)
                    {
                        if (child.nodeStyle.Display == Displays.None)
                        {
                            continue;
                        }
                        if (needsMainTrailingPos)
                        {
                            nodeSetChildTrailingPosition(node, child, mainAxis);
                        }

                        if (needsCrossTrailingPos)
                        {
                            nodeSetChildTrailingPosition(node, child, crossAxis);
                        }
                    }
                }
            }
        }

        const string spacerStr = "";

        internal static string spacer(int level)
        {
            if (level > spacerStr.Length)
            {
                level = spacerStr.Length;
            }
            return spacerStr.Substring(0, level);
        }

        internal static string measureModeName(MeasureMode mode, bool performLayout)
        {
            if ((int)mode >= Constant.measureModeCount)
            {
                return "";
            }

            if (performLayout)
            {
                return Constant.layoutModeNames[(int)mode];
            }
            return Constant.measureModeNames[(int)mode];
        }

        internal static bool measureModeSizeIsExactAndMatchesOldMeasuredSize(
            MeasureMode sizeMode,
            float size,
            float lastComputedSize
        )
        {
            return sizeMode == MeasureMode.Exactly && FloatsEqual(size, lastComputedSize);
        }

        internal static bool measureModeOldSizeIsUnspecifiedAndStillFits(
            MeasureMode sizeMode,
            float size,
            MeasureMode lastSizeMode,
            float lastComputedSize
        )
        {
            return sizeMode == MeasureMode.AtMost
                && lastSizeMode == MeasureMode.Undefined
                && (size >= lastComputedSize || FloatsEqual(size, lastComputedSize));
        }

        internal static bool measureModeNewMeasureSizeIsStricterAndStillValid(
            MeasureMode sizeMode,
            float size,
            MeasureMode lastSizeMode,
            float lastSize,
            float lastComputedSize
        )
        {
            return lastSizeMode == MeasureMode.AtMost
                && sizeMode == MeasureMode.AtMost
                && lastSize > size
                && (lastComputedSize <= size || FloatsEqual(size, lastComputedSize));
        }

        internal static bool nodeCanUseCachedMeasurement(
            MeasureMode widthMode,
            float width,
            MeasureMode heightMode,
            float height,
            MeasureMode lastWidthMode,
            float lastWidth,
            MeasureMode lastHeightMode,
            float lastHeight,
            float lastComputedWidth,
            float lastComputedHeight,
            float marginRow,
            float marginColumn,
            Config config
        )
        {
            if (lastComputedHeight < 0 || lastComputedWidth < 0)
            {
                return false;
            }
            var useRoundedComparison = config != null && config.PointScaleFactor != 0;
            var effectiveWidth = width;
            var effectiveHeight = height;
            var effectiveLastWidth = lastWidth;
            var effectiveLastHeight = lastHeight;

            if (useRoundedComparison)
            {
                effectiveWidth = RoundValueToPixelGrid(
                    width,
                    config.PointScaleFactor,
                    false,
                    false
                );
                effectiveHeight = RoundValueToPixelGrid(
                    height,
                    config.PointScaleFactor,
                    false,
                    false
                );
                effectiveLastWidth = RoundValueToPixelGrid(
                    lastWidth,
                    config.PointScaleFactor,
                    false,
                    false
                );
                effectiveLastHeight = RoundValueToPixelGrid(
                    lastHeight,
                    config.PointScaleFactor,
                    false,
                    false
                );
            }

            var hasSameWidthSpec =
                lastWidthMode == widthMode && FloatsEqual(effectiveLastWidth, effectiveWidth);
            var hasSameHeightSpec =
                lastHeightMode == heightMode && FloatsEqual(effectiveLastHeight, effectiveHeight);

            var widthIsCompatible =
                hasSameWidthSpec
                || measureModeSizeIsExactAndMatchesOldMeasuredSize(
                    widthMode,
                    width - marginRow,
                    lastComputedWidth
                )
                || measureModeOldSizeIsUnspecifiedAndStillFits(
                    widthMode,
                    width - marginRow,
                    lastWidthMode,
                    lastComputedWidth
                )
                || measureModeNewMeasureSizeIsStricterAndStillValid(
                    widthMode,
                    width - marginRow,
                    lastWidthMode,
                    lastWidth,
                    lastComputedWidth
                );

            var heightIsCompatible =
                hasSameHeightSpec
                || measureModeSizeIsExactAndMatchesOldMeasuredSize(
                    heightMode,
                    height - marginColumn,
                    lastComputedHeight
                )
                || measureModeOldSizeIsUnspecifiedAndStillFits(
                    heightMode,
                    height - marginColumn,
                    lastHeightMode,
                    lastComputedHeight
                )
                || measureModeNewMeasureSizeIsStricterAndStillValid(
                    heightMode,
                    height - marginColumn,
                    lastHeightMode,
                    lastHeight,
                    lastComputedHeight
                );

            return widthIsCompatible && heightIsCompatible;
        }

        internal static int gDepth = 0;
        internal static bool gPrintTree = false;
        internal static bool gPrintChanges = false;
        internal static bool gPrintSkips = false;

        internal static bool layoutNodeInternal(
            Node node,
            float availableWidth,
            float availableHeight,
            Directions parentDirection,
            MeasureMode widthMeasureMode,
            MeasureMode heightMeasureMode,
            float parentWidth,
            float parentHeight,
            bool performLayout,
            string reason,
            Config config
        )
        {
            var layout = node.nodeLayout;

            gDepth++;

            var needToVisitNode =
                (node.IsDirty && layout.generationCount != currentGenerationCount)
                || layout.lastParentDirection != parentDirection;

            if (needToVisitNode)
            {
                layout.nextCachedMeasurementsIndex = 0;
                layout.cachedLayout.widthMeasureMode = MeasureMode.NeverUsed_1;
                layout.cachedLayout.heightMeasureMode = MeasureMode.NeverUsed_1;
                layout.cachedLayout.computedWidth = -1;
                layout.cachedLayout.computedHeight = -1;
            }

            CachedMeasurement cachedResults = null;

            if (node.measureFunc != null)
            {
                var marginAxisRow = nodeMarginForAxis(node, FlexDirections.Row, parentWidth);
                var marginAxisColumn = nodeMarginForAxis(node, FlexDirections.Column, parentWidth);

                if (
                    nodeCanUseCachedMeasurement(
                        widthMeasureMode,
                        availableWidth,
                        heightMeasureMode,
                        availableHeight,
                        layout.cachedLayout.widthMeasureMode,
                        layout.cachedLayout.availableWidth,
                        layout.cachedLayout.heightMeasureMode,
                        layout.cachedLayout.availableHeight,
                        layout.cachedLayout.computedWidth,
                        layout.cachedLayout.computedHeight,
                        marginAxisRow,
                        marginAxisColumn,
                        config
                    )
                )
                {
                    cachedResults = layout.cachedLayout;
                }
                else
                {
                    for (int i = 0; i < layout.nextCachedMeasurementsIndex; i++)
                    {
                        if (
                            nodeCanUseCachedMeasurement(
                                widthMeasureMode,
                                availableWidth,
                                heightMeasureMode,
                                availableHeight,
                                layout.cachedMeasurements[i].widthMeasureMode,
                                layout.cachedMeasurements[i].availableWidth,
                                layout.cachedMeasurements[i].heightMeasureMode,
                                layout.cachedMeasurements[i].availableHeight,
                                layout.cachedMeasurements[i].computedWidth,
                                layout.cachedMeasurements[i].computedHeight,
                                marginAxisRow,
                                marginAxisColumn,
                                config
                            )
                        )
                        {
                            cachedResults = layout.cachedMeasurements[i];
                            break;
                        }
                    }
                }
            }
            else if (performLayout)
            {
                if (
                    FloatsEqual(layout.cachedLayout.availableWidth, availableWidth)
                    && FloatsEqual(layout.cachedLayout.availableHeight, availableHeight)
                    && layout.cachedLayout.widthMeasureMode == widthMeasureMode
                    && layout.cachedLayout.heightMeasureMode == heightMeasureMode
                )
                {
                    cachedResults = layout.cachedLayout;
                }
            }
            else
            {
                for (int i = 0; i < layout.nextCachedMeasurementsIndex; i++)
                {
                    if (
                        FloatsEqual(layout.cachedMeasurements[i].availableWidth, availableWidth)
                        && FloatsEqual(
                            layout.cachedMeasurements[i].availableHeight,
                            availableHeight
                        )
                        && layout.cachedMeasurements[i].widthMeasureMode == widthMeasureMode
                        && layout.cachedMeasurements[i].heightMeasureMode == heightMeasureMode
                    )
                    {
                        cachedResults = layout.cachedMeasurements[i];
                        break;
                    }
                }
            }

            if (!needToVisitNode && cachedResults != null)
            {
                layout.measuredDimensions[(int)Dimension.Width] = cachedResults.computedWidth;
                layout.measuredDimensions[(int)Dimension.Height] = cachedResults.computedHeight;

                if (gPrintChanges && gPrintSkips)
                {
                    System.Console.WriteLine($"{spacer(gDepth)}{gDepth}.{{[skipped]");
                    if (node.printFunc != null)
                    {
                        node.printFunc(node);
                    }

                    System.Console.WriteLine(
                        "wm: {0}, hm: {1}, aw: {2} ah: {3} => d: ({4}, {5}) {6}\n",
                        measureModeName(widthMeasureMode, performLayout),
                        measureModeName(heightMeasureMode, performLayout),
                        availableWidth,
                        availableHeight,
                        cachedResults.computedWidth,
                        cachedResults.computedHeight,
                        reason
                    );
                }
            }
            else
            {
                if (gPrintChanges)
                {
                    string s = "";
                    if (needToVisitNode)
                    {
                        s = "*";
                    }

                    System.Console.WriteLine($"{spacer(gDepth)}{gDepth}.{{{s}");
                    if (node.printFunc != null)
                    {
                        node.printFunc(node);
                    }

                    System.Console.WriteLine(
                        "wm: {0}, hm: {1}, aw: {2} ah: {3} {4}\n",
                        measureModeName(widthMeasureMode, performLayout),
                        measureModeName(heightMeasureMode, performLayout),
                        availableWidth,
                        availableHeight,
                        reason
                    );
                }

                nodelayoutImpl(
                    node,
                    availableWidth,
                    availableHeight,
                    parentDirection,
                    widthMeasureMode,
                    heightMeasureMode,
                    parentWidth,
                    parentHeight,
                    performLayout,
                    config
                );

                if (gPrintChanges)
                {
                    string s = "";
                    if (needToVisitNode)
                    {
                        s = "*";
                    }

                    System.Console.WriteLine($"{spacer(gDepth)}{gDepth}.}}{s}");
                    if (node.printFunc != null)
                    {
                        node.printFunc(node);
                    }

                    System.Console.WriteLine(
                        "wm: {0}, hm: {1}, d: ({2}, {3}) {4}\n",
                        measureModeName(widthMeasureMode, performLayout),
                        measureModeName(heightMeasureMode, performLayout),
                        layout.measuredDimensions[(int)Dimension.Width],
                        layout.measuredDimensions[(int)Dimension.Height],
                        reason
                    );
                }

                layout.lastParentDirection = parentDirection;

                if (cachedResults == null)
                {
                    if (layout.nextCachedMeasurementsIndex == Constant.MaxCachedResultCount)
                    {
                        if (gPrintChanges)
                        {
                            System.Console.WriteLine("Out of cache entries!\n");
                        }
                        layout.nextCachedMeasurementsIndex = 0;
                    }

                    CachedMeasurement newCacheEntry = null;
                    if (performLayout)
                    {
                        newCacheEntry = layout.cachedLayout;
                    }
                    else
                    {
                        newCacheEntry = layout.cachedMeasurements[
                            layout.nextCachedMeasurementsIndex
                        ];
                        layout.nextCachedMeasurementsIndex++;
                    }

                    newCacheEntry.availableWidth = availableWidth;
                    newCacheEntry.availableHeight = availableHeight;
                    newCacheEntry.widthMeasureMode = widthMeasureMode;
                    newCacheEntry.heightMeasureMode = heightMeasureMode;
                    newCacheEntry.computedWidth = layout.measuredDimensions[(int)Dimension.Width];
                    newCacheEntry.computedHeight = layout.measuredDimensions[(int)Dimension.Height];
                }
            }

            if (performLayout)
            {
                node.nodeLayout.Dimensions[(int)Dimension.Width] =
                    node.nodeLayout.measuredDimensions[(int)Dimension.Width];
                node.nodeLayout.Dimensions[(int)Dimension.Height] =
                    node.nodeLayout.measuredDimensions[(int)Dimension.Height];
                node.hasNewLayout = true;
                node.IsDirty = false;
            }

            gDepth--;
            layout.generationCount = currentGenerationCount;
            return needToVisitNode || cachedResults == null;
        }

        internal static void roundToPixelGrid(
            Node node,
            float pointScaleFactor,
            float absoluteLeft,
            float absoluteTop
        )
        {
            if (pointScaleFactor == 0.0)
            {
                return;
            }

            var nodeLeft = node.nodeLayout.Position[(int)Edge.Left];
            var nodeTop = node.nodeLayout.Position[(int)Edge.Top];

            var nodeWidth = node.nodeLayout.Dimensions[(int)Dimension.Width];
            var nodeHeight = node.nodeLayout.Dimensions[(int)Dimension.Height];

            var absoluteNodeLeft = absoluteLeft + nodeLeft;
            var absoluteNodeTop = absoluteTop + nodeTop;

            var absoluteNodeRight = absoluteNodeLeft + nodeWidth;
            var absoluteNodeBottom = absoluteNodeTop + nodeHeight;

            var textRounding = node.NodeType == NodeType.Text;

            node.nodeLayout.Position[(int)Edge.Left] = RoundValueToPixelGrid(
                nodeLeft,
                pointScaleFactor,
                false,
                textRounding
            );
            node.nodeLayout.Position[(int)Edge.Top] = RoundValueToPixelGrid(
                nodeTop,
                pointScaleFactor,
                false,
                textRounding
            );

            var hasFractionalWidth =
                !FloatsEqual(fmodf(nodeWidth * pointScaleFactor, 1), 0)
                && !FloatsEqual(fmodf(nodeWidth * pointScaleFactor, 1), 1);
            var hasFractionalHeight =
                !FloatsEqual(fmodf(nodeHeight * pointScaleFactor, 1), 0)
                && !FloatsEqual(fmodf(nodeHeight * pointScaleFactor, 1), 1);

            node.nodeLayout.Dimensions[(int)Dimension.Width] =
                RoundValueToPixelGrid(
                    absoluteNodeRight,
                    pointScaleFactor,
                    (textRounding && hasFractionalWidth),
                    (textRounding && !hasFractionalWidth)
                ) - RoundValueToPixelGrid(absoluteNodeLeft, pointScaleFactor, false, textRounding);
            node.nodeLayout.Dimensions[(int)Dimension.Height] =
                RoundValueToPixelGrid(
                    absoluteNodeBottom,
                    pointScaleFactor,
                    (textRounding && hasFractionalHeight),
                    (textRounding && !hasFractionalHeight)
                ) - RoundValueToPixelGrid(absoluteNodeTop, pointScaleFactor, false, textRounding);

            foreach (var child in node.Children)
            {
                roundToPixelGrid(child, pointScaleFactor, absoluteNodeLeft, absoluteNodeTop);
            }
        }

        internal static void calcStartWidth(
            Node node,
            float parentWidth,
            out float out_width,
            out MeasureMode out_measureMode
        )
        {
            if (nodeIsStyleDimDefined(node, FlexDirections.Row, parentWidth))
            {
                var width = resolveValue(
                    node.resolvedDimensions[(int)dim[(int)FlexDirections.Row]],
                    parentWidth
                );
                var margin = nodeMarginForAxis(node, FlexDirections.Row, parentWidth);
                out_width = width + margin;
                out_measureMode = MeasureMode.Exactly;
                return;
            }
            if (resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Width], parentWidth) >= 0f)
            {
                out_width = resolveValue(
                    node.nodeStyle.MaxDimensions[(int)Dimension.Width],
                    parentWidth
                );
                out_measureMode = MeasureMode.AtMost;
                return;
            }

            {
                var width = parentWidth;
                var widthMeasureMode = MeasureMode.Exactly;
                if (FloatIsUndefined(width))
                {
                    widthMeasureMode = MeasureMode.Undefined;
                }
                out_width = width;
                out_measureMode = widthMeasureMode;
                return;
            }
        }

        internal static void calcStartHeight(
            Node node,
            float parentWidth,
            float parentHeight,
            out float out_height,
            out MeasureMode out_measureMode
        )
        {
            if (nodeIsStyleDimDefined(node, FlexDirections.Column, parentHeight))
            {
                var height = resolveValue(
                    node.resolvedDimensions[(int)dim[(int)FlexDirections.Column]],
                    parentHeight
                );
                var margin = nodeMarginForAxis(node, FlexDirections.Column, parentWidth);
                out_height = height + margin;
                out_measureMode = MeasureMode.Exactly;
                return;
            }
            if (
                resolveValue(node.nodeStyle.MaxDimensions[(int)Dimension.Height], parentHeight) >= 0
            )
            {
                out_height = resolveValue(
                    node.nodeStyle.MaxDimensions[(int)Dimension.Height],
                    parentHeight
                );
                out_measureMode = MeasureMode.AtMost;
                return;
            }

            {
                var height = parentHeight;
                var heightMeasureMode = MeasureMode.Exactly;
                if (FloatIsUndefined(height))
                {
                    heightMeasureMode = MeasureMode.Undefined;
                }
                out_height = height;
                out_measureMode = heightMeasureMode;
                return;
            }
        }

        internal static void log(Node node, LogLevel level, string format, params object[] args)
        {
            System.Console.WriteLine(format, args);
        }

        internal static void assertCond(bool cond, string format, params object[] args)
        {
            if (!cond)
            {
                throw new System.Exception(string.Format(format, args));
            }
        }

        internal static void assertWithNode(
            Node node,
            bool cond,
            string format,
            params object[] args
        )
        {
            assertCond(cond, format, args);
        }

        internal static float fmodf(float a, float b)
        {
            return a % b;
        }

        internal static float fmaxf(float a, float b)
        {
            if (float.IsNaN(a))
            {
                return b;
            }
            if (float.IsNaN(b))
            {
                return a;
            }

            if (a > b)
            {
                return a;
            }
            return b;
        }

        internal static float fminf(float a, float b)
        {
            if (float.IsNaN(a))
            {
                return b;
            }
            if (float.IsNaN(b))
            {
                return a;
            }

            if (a < b)
            {
                return a;
            }
            return b;
        }
    }
}
