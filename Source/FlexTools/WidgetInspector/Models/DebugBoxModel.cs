// parents: @.frostline/graph/m-dbg-inspect.xml

using FlexVerse;
using UnityEngine;

namespace FlexTools.WidgetInspector.Models
{
    /// <summary>Box model rectangles for one widget.</summary>
    public struct DebugBoxModel
    {
        /// <summary>Margin rectangle.</summary>
        public Rect Margin;

        /// <summary>Border rectangle.</summary>
        public Rect Border;

        /// <summary>Padding rectangle.</summary>
        public Rect Padding;

        /// <summary>Content rectangle.</summary>
        public Rect Content;
    }
}
