// parents: @.frostline/graph/m-dbg-inspect.xml

#nullable enable

using System.Collections.Generic;
using FlexVerse;

namespace FlexTools.WidgetInspector.Models
{
    /// <summary>Immutable snapshot of a behavior's public parameters.</summary>
    public class DebugBehaviorNode
    {
        /// <summary>Creates a behavior snapshot node.</summary>
        public DebugBehaviorNode(
            string typeName,
            Dictionary<string, string>? parameters,
            int? refCount,
            int liveBehaviorId
        )
        {
            TypeName = typeName;
            Parameters = parameters ?? new Dictionary<string, string>();
            RefCount = refCount;
            LiveBehaviorId = liveBehaviorId;
        }

        /// <summary>Gets the behavior type name.</summary>
        public string TypeName { get; }

        /// <summary>Gets the parameter name-to-display map.</summary>
        public IReadOnlyDictionary<string, string> Parameters { get; }

        /// <summary>Gets the reference count, or null when unavailable.</summary>
        public int? RefCount { get; }

        /// <summary>Gets the live behavior identity hash.</summary>
        public int LiveBehaviorId { get; }
    }
}
