// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Reflection;

namespace FlexVerse.Test.Explorer
{
    public static class ReflectionUtils
    {
        public static void InvokeEvent(this object target, string eventName, params object[] args)
        {
            var type = target.GetType();
            FieldInfo field = null;

            while (type != null)
            {
                field = type.GetField(
                    eventName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                if (field != null)
                    break;

                field = type.GetField(
                    "_" + eventName,
                    BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                );

                if (field != null)
                    break;

                if (eventName.Length > 0)
                {
                    string camel =
                        "_" + char.ToLowerInvariant(eventName[0]) + eventName.Substring(1);
                    field = type.GetField(
                        camel,
                        BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public
                    );

                    if (field != null)
                        break;
                }

                type = type.BaseType;
            }

            if (field == null)
            {
                throw new ArgumentException(
                    $"Event backing field '{eventName}' not found on type '{target.GetType().FullName}'"
                );
            }

            if (field.GetValue(target) is MulticastDelegate multicastDelegate)
            {
                multicastDelegate.DynamicInvoke(args);
            }
        }
    }
}
