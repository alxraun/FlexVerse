// parents: @.frostline/graph/m-dbg-access.xml

using System;
using System.Collections.Concurrent;
using System.Reflection;
using FlexVerse;

namespace FlexTools.WidgetInspector.Accessors
{
    /// <summary>Creates reactive property refs and read-only accessors.</summary>
    public static class AccessorFactory
    {
        private static readonly ConcurrentDictionary<
            PropertyInfo,
            ConstructorInfo
        > RefConstructors = new();

        /// <summary>Returns the reactive property ref for a widget property.</summary>
        public static IPropertyRef GetRef(Widget widget, PropertyInfo property)
        {
            if (property.DeclaringType == typeof(Widget))
                return widget.GetProperty(property.Name);
            var constructor = RefConstructors.GetOrAdd(
                property,
                prop =>
                {
                    var owner = prop.DeclaringType;
                    var reference = owner.GetNestedType(prop.Name + "Ref", BindingFlags.Public);
                    if (
                        reference != null
                        && reference.ContainsGenericParameters
                        && owner.IsGenericType
                    )
                        reference = reference.MakeGenericType(owner.GetGenericArguments());
                    return reference?.GetConstructor(new[] { owner })
                        ?? throw new InvalidOperationException(
                            $"Missing generated ref for {owner}.{prop.Name}"
                        );
                }
            );
            return (IPropertyRef)constructor.Invoke(new object[] { widget });
        }

        /// <summary>Creates a lambda accessor for a public behavior property.</summary>
        public static IAccessor Create(object target, PropertyInfo property)
        {
            return new LambdaAccessor(
                property.Name,
                property.PropertyType,
                "Behavior",
                () => property.GetValue(target, null)
            );
        }

        /// <summary>Creates a ref accessor around a reactive property ref.</summary>
        public static IAccessor Create(IPropertyRef propertyRef, string category)
        {
            return new RefAccessor(propertyRef, category);
        }
    }
}
