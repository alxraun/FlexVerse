// parents: @.frostline/graph/m-dbg-access.xml

using System;
using FlexVerse;

namespace FlexTools.WidgetInspector.Accessors
{
    /// <summary>Accessor that reads a value through a reactive property ref.</summary>
    public class RefAccessor : IAccessor
    {
        /// <summary>Creates an accessor around a property ref and category.</summary>
        public RefAccessor(IPropertyRef propertyRef, string category)
        {
            Property = propertyRef;
            Category = category;
        }

        /// <summary>Gets the wrapped reactive property ref.</summary>
        public IPropertyRef Property { get; }

        /// <summary>Gets the property name.</summary>
        public string Name => Property.Name;

        /// <summary>Gets the current value type.</summary>
        public Type Type => Property.Get()?.GetType() ?? typeof(Behavior);

        /// <summary>Gets the display category.</summary>
        public string Category { get; }

        /// <summary>Returns the current value from the property ref.</summary>
        public object GetValue() => Property.Get();
    }
}
