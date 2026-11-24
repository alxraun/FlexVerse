// parents: @.frostline/graph/m-behavior-registry.xml

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace FlexVerse
{
    /// <summary>Tracks behavior reference counts and per-owner subscription ledgers. (M-BEHAVIOR-REGISTRY)</summary>
    public sealed class BehaviorRegistry
    {
        private readonly ReferenceTracker _references = new();
        private readonly SubscriptionTracker _subscriptions = new();

        /// <summary>Increments the reference count of a target.</summary>
        public void Increment(object target) => _references.Increment(target);

        /// <summary>Decrements the reference count and reports release at zero. (REQ-001)</summary>
        public bool Decrement(object target) => _references.Decrement(target);

        /// <summary>Registers an owned subscription with its release action. (REQ-005)</summary>
        public void RegisterSubscription(
            object owner,
            object key,
            Delegate handler,
            Action releaseAction
        ) => _subscriptions.Register(owner, key, handler, releaseAction);

        /// <summary>Releases one owned subscription.</summary>
        public void UnregisterSubscription(object owner, object key, Delegate handler) =>
            _subscriptions.Unregister(owner, key, handler);

        /// <summary>Releases every subscription registered by an owner. (REQ-002)</summary>
        public void UnregisterSubscriptions(object owner) => _subscriptions.UnregisterAll(owner);

        private sealed class ReferenceTracker
        {
            private sealed class Counter
            {
                public int Value;
            }

            private readonly ConditionalWeakTable<object, Counter> _counts = [];

            public void Increment(object target)
            {
                if (target == null)
                    return;

                var counter = _counts.GetOrCreateValue(target);
                counter.Value++;
            }

            public bool Decrement(object target)
            {
                if (target == null)
                    return false;

                if (_counts.TryGetValue(target, out var counter))
                {
                    counter.Value--;
                    if (counter.Value <= 0)
                    {
                        _counts.Remove(target);
                        return true;
                    }
                }

                return false;
            }
        }

        private sealed class SubscriptionTracker
        {
            private readonly ConditionalWeakTable<
                object,
                Dictionary<object, Dictionary<Delegate, Action>>
            > _registry = [];

            public void Register(object owner, object key, Delegate handler, Action release)
            {
                if (owner == null || key == null || handler == null)
                    return;

                var keyMap = _registry.GetOrCreateValue(owner);

                if (!keyMap.TryGetValue(key, out var handlerMap))
                {
                    handlerMap = [];
                    keyMap[key] = handlerMap;
                }

                if (handlerMap.TryGetValue(handler, out var previousRelease))
                    previousRelease?.Invoke();

                handlerMap[handler] = release;
            }

            public void Unregister(object owner, object key, Delegate handler)
            {
                if (owner == null || key == null || handler == null)
                    return;

                if (!_registry.TryGetValue(owner, out var keyMap))
                    return;
                if (!keyMap.TryGetValue(key, out var handlerMap))
                    return;
                if (!handlerMap.TryGetValue(handler, out var release))
                    return;

                release?.Invoke();
                handlerMap.Remove(handler);

                if (handlerMap.Count == 0)
                {
                    keyMap.Remove(key);
                }
            }

            public void UnregisterAll(object owner)
            {
                if (owner == null)
                    return;

                if (!_registry.TryGetValue(owner, out var keyMap))
                    return;

                foreach (var handlerMap in keyMap.Values)
                {
                    foreach (var release in handlerMap.Values)
                    {
                        release?.Invoke();
                    }
                }

                _registry.Remove(owner);
            }
        }
    }
}
