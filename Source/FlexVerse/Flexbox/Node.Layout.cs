// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using System;

namespace FlexVerse
{
    /// <summary>Implements layout storage and queries for layout nodes. (M-FLEXBOX)</summary>
    public partial class Node
    {
        /// <summary>Stores the eight layout dimensions of a box.</summary>
        public struct Layout8D : IEquatable<Layout8D>
        {
            /// <summary>Holds the left, right, top and bottom edge sizes.</summary>
            public int left,
                right,
                top,
                bottom;

            /// <summary>Holds the inner box position and size.</summary>
            public int x,
                y,
                width,
                height;

            internal Layout8D(int left, int right, int top, int bottom)
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;

                x = 0;
                y = 0;
                width = 0;
                height = 0;
            }

            internal Layout8D(
                int left,
                int right,
                int top,
                int bottom,
                int x,
                int y,
                int width,
                int height
            )
            {
                this.left = left;
                this.right = right;
                this.top = top;
                this.bottom = bottom;

                this.x = x;
                this.y = y;
                this.width = width;
                this.height = height;
            }

            internal void SetInnerEdge(Layout8D layout)
            {
                x = layout.x + layout.left;
                y = layout.y + layout.top;
                width = layout.width - (layout.left + layout.right);
                height = layout.height - (layout.top + layout.bottom);
            }

            internal void SetOuterEdge(Layout8D layout)
            {
                x = layout.x - left;
                y = layout.y - top;
                width = layout.width + (left + right);
                height = layout.height + (top + bottom);
            }

            /// <summary>Returns a human-readable representation of the box.</summary>
            public override string ToString()
            {
                return string.Format(
                    "(x:{0} y:{1} w:{2} h:{3}) (l:{4} t:{5} r:{6} b:{7}))",
                    x,
                    y,
                    width,
                    height,
                    left,
                    top,
                    right,
                    bottom
                );
            }

            /// <summary>Indicates whether this box equals the other box.</summary>
            public bool Equals(Layout8D l)
            {
                return x == l.x
                    && y == l.y
                    && width == l.width
                    && height == l.height
                    && left == l.left
                    && right == l.right
                    && top == l.top
                    && bottom == l.bottom;
            }

            /// <summary>Compares two boxes for equality.</summary>
            public static bool operator ==(Layout8D a, Layout8D b) => a.Equals(b);

            /// <summary>Compares two boxes for inequality.</summary>
            public static bool operator !=(Layout8D a, Layout8D b) => !a.Equals(b);

            /// <summary>Indicates whether this box equals the given object.</summary>
            public override bool Equals(object l)
            {
                return base.Equals(l);
            }

            /// <summary>Returns a hash code for the box.</summary>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }

        /// <summary>Represents a node's computed layout box with edges and spacing.</summary>
        public struct Layout : IEquatable<Layout>
        {
            /// <summary>Indicates whether layout values have been computed.</summary>
            public bool setted;

            /// <summary>Holds the outer box edge offsets.</summary>
            public int left,
                right,
                top,
                bottom;

            /// <summary>Holds the outer box position and size.</summary>
            public int x,
                y,
                width,
                height;

            /// <summary>Holds the computed margin box.</summary>
            public Layout8D margin;

            /// <summary>Holds the computed border box.</summary>
            public Layout8D border;

            /// <summary>Holds the computed padding box.</summary>
            public Layout8D padding;

            /// <summary>Holds the computed content box.</summary>
            public Layout8D content;

            /// <summary>Indicates whether the layout pass overflowed.</summary>
            public bool hadOverflow;

            /// <summary>Holds the resolved layout direction.</summary>
            public Directions direction;

            internal Layout(Node node)
            {
                setted = true;
                x = (int)node.LayoutGetX();
                y = (int)node.LayoutGetY();
                left = (int)node.LayoutGetLeft();
                right = (int)node.LayoutGetRight();
                top = (int)node.LayoutGetTop();
                bottom = (int)node.LayoutGetBottom();
                width = (int)node.LayoutGetWidth();
                height = (int)node.LayoutGetHeight();

                border = new Layout8D(
                    (int)node.LayoutGetBorder(Edge.Left),
                    (int)node.LayoutGetBorder(Edge.Right),
                    (int)node.LayoutGetBorder(Edge.Top),
                    (int)node.LayoutGetBorder(Edge.Bottom),
                    x,
                    y,
                    width,
                    height
                );
                padding = new Layout8D(
                    (int)node.LayoutGetPadding(Edge.Left),
                    (int)node.LayoutGetPadding(Edge.Right),
                    (int)node.LayoutGetPadding(Edge.Top),
                    (int)node.LayoutGetPadding(Edge.Bottom)
                );
                padding.SetInnerEdge(border);
                content = new Layout8D(0, 0, 0, 0);
                content.SetInnerEdge(padding);

                margin = new Layout8D(
                    (int)node.LayoutGetMargin(Edge.Left),
                    (int)node.LayoutGetMargin(Edge.Right),
                    (int)node.LayoutGetMargin(Edge.Top),
                    (int)node.LayoutGetMargin(Edge.Bottom)
                );
                margin.SetOuterEdge(border);

                hadOverflow = node.LayoutGetHadOverflow();
                direction = node.LayoutGetDirection();
            }

            /// <summary>Returns a human-readable representation of the layout.</summary>
            public override string ToString()
            {
                return ToStr(0);
            }

            /// <summary>Returns an indented human-readable layout description.</summary>
            public string ToStr(int indent)
            {
                string line = "{\n";
                indent++;
                string tab = new(' ', indent * 2);
                line +=
                    tab
                    + "box = "
                    + string.Format(
                        "(x:{0} y:{1} w:{2} h:{3}) (l:{4} t:{5} r:{6} b:{7})",
                        x,
                        y,
                        width,
                        height,
                        left,
                        top,
                        right,
                        bottom
                    )
                    + "\n";
                line += tab + "margin = " + margin.ToString() + "\n";
                line += tab + "border = " + border.ToString() + "\n";
                line += tab + "padding = " + padding.ToString() + "\n";
                line += tab + "content = " + content.ToString() + "\n";
                indent--;
                line += new System.String(' ', indent * 2) + "}";
                return line;
            }

            /// <summary>Compares two layouts for equality.</summary>
            public static bool operator ==(Layout a, Layout b) => a.Equals(b);

            /// <summary>Compares two layouts for inequality.</summary>
            public static bool operator !=(Layout a, Layout b) => !a.Equals(b);

            /// <summary>Indicates whether this layout equals the other layout.</summary>
            public bool Equals(Layout l)
            {
                return x == l.x
                    && y == l.y
                    && width == l.width
                    && height == l.height
                    && left == l.left
                    && right == l.right
                    && top == l.top
                    && bottom == l.bottom
                    && margin == l.margin
                    && border == l.border
                    && padding == l.padding
                    && content == l.content
                    && hadOverflow == l.hadOverflow
                    && direction == l.direction;
            }

            /// <summary>Indicates whether this layout equals the given object.</summary>
            public override bool Equals(object l)
            {
                return base.Equals(l);
            }

            /// <summary>Returns a hash code for the layout.</summary>
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
    }
}
