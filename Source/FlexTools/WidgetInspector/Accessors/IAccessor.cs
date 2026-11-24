// parents: @.frostline/graph/m-dbg-access.xml

using System;
using FlexVerse;

namespace FlexTools.WidgetInspector.Accessors
{
    /// <summary>Provides read-only access to a named member value.</summary>
    public interface IAccessor
    {
        /// <summary>Gets the member name.</summary>
        string Name { get; }

        /// <summary>Gets the member value type.</summary>
        Type Type { get; }

        /// <summary>Gets the display category.</summary>
        string Category { get; }

        /// <summary>Returns the current member value.</summary>
        object GetValue();
    }
}
