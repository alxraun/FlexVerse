// parents: @.frostline/graph/m-inherited-context.xml

using System;
using System.Collections.Generic;
using System.Reflection;

namespace FlexVerse
{
    /// <summary>Resolves consumers to the nearest ancestor context provider. (M-INHERITED-CONTEXT)</summary>
    public class InheritedContextManager(WidgetTree widgetTree)
    {
        private readonly WidgetTree _widgetTree = widgetTree;
        private readonly Dictionary<string, Dictionary<Widget, Behavior>> _providers = [];
        private readonly Dictionary<string, Dictionary<Widget, Widget>> _consumers = [];
        private readonly Dictionary<(Type, string), MethodInfo> _methodCache = [];

        /// <summary>Registers a provider and notifies descendants. (REQ-001)</summary>
        public void RegisterProvider(Widget provider, string propertyName, Behavior behavior)
        {
            Log.Info(
                "M-INHERITED-CONTEXT",
                "provider.registered",
                ("provider", provider),
                ("property", propertyName),
                ("behavior", behavior)
            );
            UnsubscribeConsumer(provider, propertyName);
            UpdateProviderState(provider, propertyName, behavior);
            NotifyDescendants(provider, propertyName, GetWrappedTypeName(behavior));
        }

        /// <summary>Binds a consumer to the nearest provider. (REQ-001, REQ-002)</summary>
        public void SubscribeConsumer(Widget consumer, string propertyName, string typeName)
        {
            Log.Info(
                "M-INHERITED-CONTEXT",
                "consumer.subscribed",
                ("consumer", consumer),
                ("property", propertyName),
                ("typeName", typeName)
            );
            var provider = FindNearestProvider(consumer, propertyName);
            BindConsumer(consumer, propertyName, provider, typeName);
        }

        /// <summary>Releases a widget's provider or consumer registration.</summary>
        public void ReleaseInheritedContext(Widget widget, string propertyName)
        {
            Log.Debug(
                "M-INHERITED-CONTEXT",
                "context.released",
                ("widget", widget),
                ("property", propertyName)
            );
            TryReleaseProvider(widget, propertyName);
            UnsubscribeConsumer(widget, propertyName);
        }

        private void UpdateProviderState(Widget provider, string propertyName, Behavior behavior)
        {
            Log.Info(
                "M-INHERITED-CONTEXT",
                "provider.state.updated",
                ("provider", provider),
                ("property", propertyName),
                ("behavior", behavior)
            );
            var registry = GetOrCreateProviderRegistry(propertyName);

            if (registry.TryGetValue(provider, out var oldBehavior))
            {
                provider.ReleaseBehavior(oldBehavior);
            }

            if (behavior != null)
            {
                provider.ActivateBehavior(behavior);
            }

            registry[provider] = behavior;
        }

        private void TryReleaseProvider(Widget provider, string propertyName)
        {
            Log.Debug(
                "M-INHERITED-CONTEXT",
                "provider.released",
                ("provider", provider),
                ("property", propertyName)
            );
            if (
                !_providers.TryGetValue(propertyName, out var registry)
                || !registry.TryGetValue(provider, out var behavior)
            )
            {
                return;
            }

            provider.ReleaseBehavior(behavior);
            registry.Remove(provider);

            if (registry.Count == 0)
            {
                _providers.Remove(propertyName);
            }

            NotifyDescendants(provider, propertyName, GetWrappedTypeName(behavior));
        }

        private void UnsubscribeConsumer(Widget consumer, string propertyName)
        {
            if (_consumers.TryGetValue(propertyName, out var bindings))
            {
                bindings.Remove(consumer);
                if (bindings.Count == 0)
                {
                    _consumers.Remove(propertyName);
                }
            }
        }

        private void NotifyDescendants(Widget root, string propertyName, string typeName)
        {
            var children = _widgetTree.GetChildInstances(root);
            for (var i = 0; i < children.Count; i++)
            {
                RebindDescendants(children[i], propertyName, typeName);
            }
        }

        private void RebindDescendants(Widget node, string propertyName, string typeName)
        {
            if (node == null || IsProvider(node, propertyName))
            {
                return;
            }

            if (IsConsumer(node, propertyName))
            {
                var provider = FindNearestProvider(node, propertyName);
                BindConsumer(node, propertyName, provider, typeName);
            }

            var children = _widgetTree.GetChildInstances(node);
            for (var i = 0; i < children.Count; i++)
            {
                RebindDescendants(children[i], propertyName, typeName);
            }
        }

        private void BindConsumer(
            Widget consumer,
            string propertyName,
            Widget provider,
            string typeName
        )
        {
            Log.Info(
                "M-INHERITED-CONTEXT",
                "consumer.bound",
                ("consumer", consumer),
                ("provider", provider),
                ("property", propertyName)
            );
            GetOrCreateConsumerBindings(propertyName)[consumer] = provider;
            var behavior = ResolveBehavior(provider, propertyName, typeName);
            InvokeConsumerUpdate(consumer, propertyName, behavior, typeName);
        }

        private Behavior ResolveBehavior(Widget provider, string propertyName, string typeName)
        {
            if (
                provider != null
                && _providers.TryGetValue(propertyName, out var registry)
                && registry.TryGetValue(provider, out var behavior)
            )
            {
                return behavior;
            }
            return CreateFallbackBehavior(typeName);
        }

        private Widget FindNearestProvider(Widget consumer, string propertyName)
        {
            if (!_providers.ContainsKey(propertyName))
            {
                return null;
            }

            var current = _widgetTree.GetParentInstance(consumer);
            while (current != null)
            {
                if (IsProvider(current, propertyName))
                {
                    return current;
                }
                current = _widgetTree.GetParentInstance(current);
            }
            return null;
        }

        private bool IsProvider(Widget widget, string propertyName)
        {
            return _providers.TryGetValue(propertyName, out var registry)
                && registry.ContainsKey(widget);
        }

        private bool IsConsumer(Widget widget, string propertyName)
        {
            return _consumers.TryGetValue(propertyName, out var bindings)
                && bindings.ContainsKey(widget);
        }

        private Dictionary<Widget, Behavior> GetOrCreateProviderRegistry(string propertyName)
        {
            if (!_providers.TryGetValue(propertyName, out var registry))
            {
                registry = [];
                _providers[propertyName] = registry;
            }
            return registry;
        }

        private Dictionary<Widget, Widget> GetOrCreateConsumerBindings(string propertyName)
        {
            if (!_consumers.TryGetValue(propertyName, out var bindings))
            {
                bindings = [];
                _consumers[propertyName] = bindings;
            }
            return bindings;
        }

        private void InvokeConsumerUpdate(
            Widget consumer,
            string propertyName,
            Behavior behavior,
            string typeName
        )
        {
            Log.Info(
                "M-INHERITED-CONTEXT",
                "consumer.updated",
                ("consumer", consumer),
                ("property", propertyName),
                ("behavior", behavior)
            );
            var key = (consumer.GetType(), propertyName);
            if (!_methodCache.TryGetValue(key, out var method))
            {
                method = ResolveUpdateMethod(consumer.GetType(), propertyName, typeName);
                _methodCache[key] = method;
            }
            method.Invoke(consumer, [behavior]);
        }

        private static MethodInfo ResolveUpdateMethod(
            Type consumerType,
            string propertyName,
            string typeName
        )
        {
            var methodName = $"_Update{propertyName}FromAncestor";
            var wrappedType =
                ResolveType(typeName)
                ?? throw new InvalidOperationException($"[ICM] Type not found: {typeName}");
            var paramType = typeof(Behavior<>).MakeGenericType(wrappedType);

            return consumerType.GetMethod(
                    methodName,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    [paramType],
                    null
                )
                ?? throw new InvalidOperationException(
                    $"[ICM] Method not found: {methodName} in {consumerType}"
                );
        }

        private static string GetWrappedTypeName(Behavior behavior)
        {
            var type = behavior.GetType();
            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Behavior<>))
                {
                    return type.GetGenericArguments()[0].FullName!;
                }
                type = type.BaseType;
            }
            throw new InvalidOperationException("Invalid behavior type hierarchy.");
        }

        private static Behavior CreateFallbackBehavior(string typeName)
        {
            var type =
                ResolveType(typeName)
                ?? throw new InvalidOperationException($"[ICM] Type not found: {typeName}");
            var behaviorType = typeof(StaticBehavior<>).MakeGenericType(type);
            return (Behavior)Activator.CreateInstance(behaviorType)!;
        }

        private static Type ResolveType(string name)
        {
            var isNullable = name.EndsWith("?");
            var baseName = isNullable ? name.TrimEnd('?') : name;

            var type = _aliases.TryGetValue(baseName, out var alias)
                ? Type.GetType(alias)
                : Type.GetType(baseName);

            if (type == null)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if ((type = asm.GetType(baseName)) != null)
                        break;
                }
            }

            if (type != null && isNullable && type.IsValueType)
            {
                return typeof(Nullable<>).MakeGenericType(type);
            }

            return type;
        }

        private static readonly Dictionary<string, string> _aliases = new()
        {
            ["bool"] = typeof(bool).FullName!,
            ["byte"] = typeof(byte).FullName!,
            ["sbyte"] = typeof(sbyte).FullName!,
            ["char"] = typeof(char).FullName!,
            ["decimal"] = typeof(decimal).FullName!,
            ["double"] = typeof(double).FullName!,
            ["float"] = typeof(float).FullName!,
            ["int"] = typeof(int).FullName!,
            ["uint"] = typeof(uint).FullName!,
            ["long"] = typeof(long).FullName!,
            ["ulong"] = typeof(ulong).FullName!,
            ["short"] = typeof(short).FullName!,
            ["ushort"] = typeof(ushort).FullName!,
            ["object"] = typeof(object).FullName!,
            ["string"] = typeof(string).FullName!,
        };
    }
}
