// parents: @.frostline/graph/m-bind-ref.xml

using System;

namespace FlexVerse
{
    /// <summary>Binds handlers to the behavior currently occupying a property slot. (M-BIND-REF)</summary>
    public readonly ref struct BindRef(IPropertyRef propertyRef)
    {
        private readonly IPropertyRef _ref = propertyRef;

        private static object EventKey(string name, string eventName) =>
            (nameof(BindRef), name, eventName);

        /// <summary>Subscribes to the current behavior's update events. (REQ-001)</summary>
        public event Action<float> OnUpdate
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                if (behavior is null)
                    return;
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Unregister()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnUpdate)),
                        value
                    );
                }

                void Release()
                {
                    behavior.OnUpdate -= value;
                    property.Changing -= Unregister;
                }

                behavior.OnUpdate += value;
                property.Changing += Unregister;

                registry.RegisterSubscription(
                    property.Owner,
                    EventKey(property.Name, nameof(OnUpdate)),
                    value,
                    Release
                );
            }
            remove
            {
                FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscription(
                    _ref.Owner,
                    EventKey(_ref.Name, nameof(OnUpdate)),
                    value
                );
            }
        }

        /// <summary>Subscribes to the current behavior's activation event.</summary>
        public event Action OnActivate
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                if (behavior is null)
                    return;
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Unregister()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnActivate)),
                        value
                    );
                }

                void Release()
                {
                    behavior.OnActivate -= value;
                    property.Changing -= Unregister;
                }

                behavior.OnActivate += value;
                property.Changing += Unregister;

                registry.RegisterSubscription(
                    property.Owner,
                    EventKey(property.Name, nameof(OnActivate)),
                    value,
                    Release
                );
            }
            remove
            {
                FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscription(
                    _ref.Owner,
                    EventKey(_ref.Name, nameof(OnActivate)),
                    value
                );
            }
        }

        /// <summary>Subscribes to the current behavior's start event.</summary>
        public event Action OnStart
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                if (behavior is null)
                    return;
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Unregister()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnStart)),
                        value
                    );
                }

                void Release()
                {
                    behavior.OnStart -= value;
                    property.Changing -= Unregister;
                }

                behavior.OnStart += value;
                property.Changing += Unregister;

                registry.RegisterSubscription(
                    property.Owner,
                    EventKey(property.Name, nameof(OnStart)),
                    value,
                    Release
                );
            }
            remove
            {
                FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscription(
                    _ref.Owner,
                    EventKey(_ref.Name, nameof(OnStart)),
                    value
                );
            }
        }

        /// <summary>Subscribes to the current behavior's completion event.</summary>
        public event Action OnComplete
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                if (behavior is null)
                    return;
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Unregister()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnComplete)),
                        value
                    );
                }

                void Release()
                {
                    behavior.OnComplete -= value;
                    property.Changing -= Unregister;
                }

                behavior.OnComplete += value;
                property.Changing += Unregister;

                registry.RegisterSubscription(
                    property.Owner,
                    EventKey(property.Name, nameof(OnComplete)),
                    value,
                    Release
                );
            }
            remove
            {
                FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscription(
                    _ref.Owner,
                    EventKey(_ref.Name, nameof(OnComplete)),
                    value
                );
            }
        }

        /// <summary>Subscribes to the current behavior's reset event.</summary>
        public event Action OnReset
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                if (behavior is null)
                    return;
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Unregister()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnReset)),
                        value
                    );
                }

                void Release()
                {
                    behavior.OnReset -= value;
                    property.Changing -= Unregister;
                }

                behavior.OnReset += value;
                property.Changing += Unregister;

                registry.RegisterSubscription(
                    property.Owner,
                    EventKey(property.Name, nameof(OnReset)),
                    value,
                    Release
                );
            }
            remove
            {
                FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscription(
                    _ref.Owner,
                    EventKey(_ref.Name, nameof(OnReset)),
                    value
                );
            }
        }
    }
}
