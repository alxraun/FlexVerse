// parents: @.frostline/graph/m-dbg-ui.xml

using System.Collections.Generic;
using System.Linq;
using FlexTools.WidgetInspector.Inspection;
using FlexVerse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Lists the properties of a selected hierarchy node.</summary>
    public partial class HierarchyNodeProperties : View
    {
        /// <summary>Gets or sets the widget snapshot node.</summary>
        [Prop]
        public Behavior<DebugNode> Node { get; set; }

        /// <summary>Gets or sets the nesting depth.</summary>
        [Prop]
        public Behavior<int> Depth { get; set; }

        /// <summary>Gets or sets the active category filters.</summary>
        [Prop]
        public Behavior<HashSet<string>> ActiveFilters { get; set; }

        /// <summary>Builds a property row for each filtered property.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            var node = Node.Value;
            if (node == null)
                return [];
            return node
                .Properties.Where(property => ActiveFilters.Value.Contains(property.Kind))
                .OrderBy(property => property.Kind)
                .ThenBy(property => property.Name)
                .Select(property =>
                    (Widget)
                        new PropertyRow
                        {
                            Key = property.Name,
                            Property = property,
                            MarginLeft = (Depth.Value * 10f + 16f).Point(),
                        }
                );
        }
    }
}
