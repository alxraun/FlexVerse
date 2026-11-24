// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using System.Collections.Generic;
using System.Linq;
using FlexVerse.Flexbox;

namespace FlexVerse
{
    /// <summary>Represents a node in the flexbox layout tree.</summary>
    public partial class Node
    {
        /// <summary>Holds the style defining this node's layout properties.</summary>
        public Style nodeStyle = new();
        internal readonly Flex.Layout nodeLayout = new();
        internal int lineIndex;

        private Layout? _layout = null;

        /// <summary>Provides read access to this node's computed layout.</summary>
        public Layout layout
        {
            get
            {
                if (_layout == null)
                    _layout = new Layout(this);
                return (Layout)_layout;
            }
        }

        internal Node Parent = null;
        internal readonly List<Node> Children = [];

        /// <summary>Returns the number of child nodes.</summary>
        public int ChildrenCount
        {
            get { return Children.Count; }
        }

        /// <summary>Returns this node's first child, or null.</summary>
        public Node firstChild
        {
            get { return Children.Count > 0 ? Children.First() : null; }
        }

        /// <summary>Returns this node's last child, or null.</summary>
        public Node lastChild
        {
            get { return Children.Count > 0 ? Children.Last() : null; }
        }

        internal Node NextChild;

        internal MeasureFunc measureFunc;
        internal BaselineFunc baselineFunc;
        internal PrintFunc printFunc;
        internal Config config = Constant.configDefaults;

        /// <summary>Indicates whether the node needs a layout pass.</summary>
        public bool IsDirty { get; internal set; }

        /// <summary>Indicates whether the node produced a new layout.</summary>
        public bool hasNewLayout = true;
        internal NodeType NodeType = NodeType.Default;

        internal readonly Length[] resolvedDimensions = [Flex.ValueUndefined, Flex.ValueUndefined];

        /// <summary>Holds user-defined context data attached to the node.</summary>
        public object Context;

        /// <summary>Creates an empty node with default style.</summary>
        public Node() { }

        /// <summary>Creates a node using the given style.</summary>
        public Node(Style style)
        {
            nodeStyle = style;
        }

        /// <summary>Computes layout for this node's subtree within the parent size.</summary>
        public void CalculateLayout(
            float parentWidth,
            float parentHeight,
            Directions parentDirection
        )
        {
            _layout = null;
            Flex.CalculateLayout(this, parentWidth, parentHeight, parentDirection);
        }

        /// <summary>Marks the node as needing a new layout pass.</summary>
        public void MarkAsDirty()
        {
            Flex.nodeMarkDirtyInternal(this);
        }

        #region Layout

        internal float LayoutGetX()
        {
            var x = nodeLayout.Position[(int)Edge.Left];
            if (Parent != null)
                x += Parent.LayoutGetX();
            return x;
        }

        internal float LayoutGetY()
        {
            var y = nodeLayout.Position[(int)Edge.Top];
            if (Parent != null)
                y += Parent.LayoutGetY();
            return y;
        }

        internal float LayoutGetLeft()
        {
            return nodeLayout.Position[(int)Edge.Left];
        }

        internal float LayoutGetTop()
        {
            return nodeLayout.Position[(int)Edge.Top];
        }

        internal float LayoutGetRight()
        {
            return nodeLayout.Position[(int)Edge.Right];
        }

        internal float LayoutGetBottom()
        {
            return nodeLayout.Position[(int)Edge.Bottom];
        }

        internal float LayoutGetWidth()
        {
            return nodeLayout.Dimensions[(int)Dimension.Width];
        }

        internal float LayoutGetHeight()
        {
            return nodeLayout.Dimensions[(int)Dimension.Height];
        }

        /// <summary>Returns the computed layout margin for the given edge.</summary>
        public float LayoutGetMargin(Edge edge)
        {
            Flex.assertWithNode(
                this,
                edge < Edge.End,
                "Cannot get layout properties of multi-edge shorthands"
            );
            if (edge == Edge.Left)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Margin[(int)Edge.End];
                }
                return nodeLayout.Margin[(int)Edge.Start];
            }
            if (edge == Edge.Right)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Margin[(int)Edge.Start];
                }
                return nodeLayout.Margin[(int)Edge.End];
            }
            return nodeLayout.Margin[(int)edge];
        }

        /// <summary>Returns the computed layout border width for the given edge.</summary>
        public float LayoutGetBorder(Edge edge)
        {
            Flex.assertWithNode(
                this,
                edge < Edge.End,
                "Cannot get layout properties of multi-edge shorthands"
            );
            if (edge == Edge.Left)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Border[(int)Edge.End];
                }
                return nodeLayout.Border[(int)Edge.Start];
            }
            if (edge == Edge.Right)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Border[(int)Edge.Start];
                }
                return nodeLayout.Border[(int)Edge.End];
            }
            return nodeLayout.Border[(int)edge];
        }

        /// <summary>Returns the computed layout padding for the given edge.</summary>
        public float LayoutGetPadding(Edge edge)
        {
            Flex.assertWithNode(
                this,
                edge < Edge.End,
                "Cannot get layout properties of multi-edge shorthands"
            );
            if (edge == Edge.Left)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Padding[(int)Edge.End];
                }
                return nodeLayout.Padding[(int)Edge.Start];
            }
            if (edge == Edge.Right)
            {
                if (nodeLayout.Direction == Directions.RTL)
                {
                    return nodeLayout.Padding[(int)Edge.Start];
                }
                return nodeLayout.Padding[(int)Edge.End];
            }
            return nodeLayout.Padding[(int)edge];
        }

        internal Directions LayoutGetDirection()
        {
            return nodeLayout.Direction;
        }

        internal bool LayoutGetHadOverflow()
        {
            return nodeLayout.HadOverflow;
        }

        #endregion

        #region other props

        /// <summary>Sets the measure function used for leaf node sizing.</summary>
        public void SetMeasureFunc(MeasureFunc measureFunc)
        {
            Flex.SetMeasureFunc(this, measureFunc);
        }

        /// <summary>Returns the node's measure function.</summary>
        public MeasureFunc GetMeasureFunc()
        {
            return measureFunc;
        }

        /// <summary>Sets the baseline function used for baseline alignment.</summary>
        public void SetBaselineFunc(BaselineFunc baselineFunc)
        {
            this.baselineFunc = baselineFunc;
        }

        /// <summary>Returns the node's baseline function.</summary>
        public BaselineFunc GetBaselineFunc()
        {
            return baselineFunc;
        }

        /// <summary>Sets the print function used for debug output.</summary>
        public void SetPrintFunc(PrintFunc printFunc)
        {
            this.printFunc = printFunc;
        }

        /// <summary>Returns the node's print function.</summary>
        public PrintFunc GetPrintFunc()
        {
            return printFunc;
        }
        #endregion

        #region tree
        /// <summary>Returns the child node at the given index.</summary>
        public Node GetChild(int idx)
        {
            return Flex.GetChild(this, idx);
        }

        /// <summary>Appends a child node to this node.</summary>
        public void AddChild(Node child)
        {
            Flex.InsertChild(this, child, ChildrenCount);
        }

        /// <summary>Inserts a child node at the given index.</summary>
        public void InsertChild(Node child, int idx)
        {
            Flex.InsertChild(this, child, idx);
        }

        /// <summary>Removes the given child node from this node.</summary>
        public void RemoveChild(Node child)
        {
            Flex.RemoveChild(this, child);
        }
        #endregion
    }
}
