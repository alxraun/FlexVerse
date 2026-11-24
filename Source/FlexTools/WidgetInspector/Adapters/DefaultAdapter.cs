// parents: @.frostline/graph/m-dbg-adapt.xml

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using FlexTools.WidgetInspector.Accessors;
using FlexVerse;

namespace FlexTools.WidgetInspector.Adapters
{
    /// <summary>Default adapter that exposes public behavior properties as slots.</summary>
    public class DefaultAdapter : IBehaviorAdapter
    {
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> Properties = new();

        /// <summary>Returns accessors for the target's public properties.</summary>
        public IEnumerable<IAccessor> GetSlots(Behavior target)
        {
            foreach (
                var property in Properties.GetOrAdd(
                    target.GetType(),
                    type =>
                        type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                            .Where(p =>
                                p.CanRead
                                && p.GetMethod.IsPublic
                                && p.GetIndexParameters().Length == 0
                            )
                            .OrderBy(p => p.Name)
                            .ToArray()
                )
            )
                yield return AccessorFactory.Create(target, property);
        }
    }
}
