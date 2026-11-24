// parents: @.frostline/graph/m-dbg-inspect.xml

using FlexVerse;

namespace FlexTools.WidgetInspector.Models
{
    /// <summary>Holds expansion and selection state for a hierarchy item.</summary>
    public class HierarchyItemState
    {
        /// <summary>Gets or sets a value indicating whether the item is expanded.</summary>
        public bool IsExpanded { get; set; }

        /// <summary>Gets or sets a value indicating whether the item is selected.</summary>
        public bool IsSelected { get; set; }
    }
}
