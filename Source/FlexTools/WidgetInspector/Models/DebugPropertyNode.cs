// parents: @.frostline/graph/m-dbg-inspect.xml

#nullable enable

using FlexVerse;

namespace FlexTools.WidgetInspector.Models
{
    /// <summary>Immutable snapshot of one inspected property.</summary>
    public class DebugPropertyNode
    {
        /// <summary>Creates a property snapshot node.</summary>
        public DebugPropertyNode(
            string name,
            string kind,
            string valueDisplay,
            DebugBehaviorNode? behaviorNode = null
        )
        {
            Name = name;
            Kind = kind;
            ValueDisplay = valueDisplay;
            BehaviorNode = behaviorNode;
        }

        /// <summary>Gets the property name.</summary>
        public string Name { get; }

        /// <summary>Gets the property category.</summary>
        public string Kind { get; }

        /// <summary>Gets the current value display string.</summary>
        public string ValueDisplay { get; }

        /// <summary>Gets the behavior snapshot, or null.</summary>
        public DebugBehaviorNode? BehaviorNode { get; }
    }
}
