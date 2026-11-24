// parents: @.frostline/graph/m-dbg-access.xml

using System;
using FlexVerse;

namespace FlexTools.WidgetInspector.Accessors
{
    /// <summary>Accessor that reads a value through a getter delegate.</summary>
    public class LambdaAccessor : IAccessor
    {
        private readonly Func<object> _getter;

        /// <summary>Creates an accessor from name, type, category, and getter.</summary>
        public LambdaAccessor(string name, Type type, string category, Func<object> getter)
        {
            Name = name;
            Type = type;
            Category = category;
            _getter = getter;
        }

        /// <summary>Gets the member name.</summary>
        public string Name { get; }

        /// <summary>Gets the member value type.</summary>
        public Type Type { get; }

        /// <summary>Gets the display category.</summary>
        public string Category { get; }

        /// <summary>Returns the current value from the getter.</summary>
        public object GetValue()
        {
            return _getter();
        }
    }
}
