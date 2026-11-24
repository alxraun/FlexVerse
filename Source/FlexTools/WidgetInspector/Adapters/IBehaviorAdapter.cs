// parents: @.frostline/graph/m-dbg-adapt.xml

using System.Collections.Generic;
using FlexTools.WidgetInspector.Accessors;
using FlexVerse;

namespace FlexTools.WidgetInspector.Adapters
{
    /// <summary>Exposes behavior parameters as readable accessors.</summary>
    public interface IBehaviorAdapter
    {
        /// <summary>Returns accessors for the target's readable parameters.</summary>
        IEnumerable<IAccessor> GetSlots(Behavior target);
    }
}
