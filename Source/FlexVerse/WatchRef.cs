// parents: @.frostline/graph/m-watch-ref.xml

using System;

namespace FlexVerse
{
    /// <summary>Watches a property slot and resubscribes on occupant replacement. (M-WATCH-REF)</summary>
    public readonly ref struct WatchRef(IPropertyRef propertyRef)
    {
        private readonly IPropertyRef _ref = propertyRef;

        private static object EventKey(string name, string eventName) =>
            (nameof(WatchRef), name, eventName);

        /// <summary>Watches update events across slot replacement. (REQ-001)</summary>
        public event Action<float> OnUpdate
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Rebind()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnUpdate)),
                        value
                    );
                    new WatchRef(property).OnUpdate += value;
                }

                void Release()
                {
                    if (behavior is not null)
                        behavior.OnUpdate -= value;
                    property.Changed -= Rebind;
                }

                if (behavior is not null)
                    behavior.OnUpdate += value;
                property.Changed += Rebind;
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

        /// <summary>Watches activation events across slot replacement.</summary>
        public event Action OnActivate
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Rebind()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnActivate)),
                        value
                    );
                    new WatchRef(property).OnActivate += value;
                }

                void Release()
                {
                    if (behavior is not null)
                        behavior.OnActivate -= value;
                    property.Changed -= Rebind;
                }

                if (behavior is not null)
                    behavior.OnActivate += value;
                property.Changed += Rebind;
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

        /// <summary>Watches start events across slot replacement.</summary>
        public event Action OnStart
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Rebind()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnStart)),
                        value
                    );
                    new WatchRef(property).OnStart += value;
                }

                void Release()
                {
                    if (behavior is not null)
                        behavior.OnStart -= value;
                    property.Changed -= Rebind;
                }

                if (behavior is not null)
                    behavior.OnStart += value;
                property.Changed += Rebind;
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

        /// <summary>Watches completion events across slot replacement.</summary>
        public event Action OnComplete
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Rebind()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnComplete)),
                        value
                    );
                    new WatchRef(property).OnComplete += value;
                }

                void Release()
                {
                    if (behavior is not null)
                        behavior.OnComplete -= value;
                    property.Changed -= Rebind;
                }

                if (behavior is not null)
                    behavior.OnComplete += value;
                property.Changed += Rebind;
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

        /// <summary>Watches reset events across slot replacement.</summary>
        public event Action OnReset
        {
            add
            {
                var property = _ref;
                var behavior = property.Get();
                var registry = FlexVerseRoot.Instance.BehaviorRegistry;

                void Rebind()
                {
                    registry.UnregisterSubscription(
                        property.Owner,
                        EventKey(property.Name, nameof(OnReset)),
                        value
                    );
                    new WatchRef(property).OnReset += value;
                }

                void Release()
                {
                    if (behavior is not null)
                        behavior.OnReset -= value;
                    property.Changed -= Rebind;
                }

                if (behavior is not null)
                    behavior.OnReset += value;
                property.Changed += Rebind;
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
