// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Runtime.CompilerServices;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    public class PropertySubscriptionTests
    {
        private sealed class PropertyOwner : IPropertyRef<int>
        {
            private Behavior<int> _current;
            public int Calls;

            public PropertyOwner(Behavior<int> current) => _current = current;

            public string Name => "Current";
            public object Owner => this;
            public event Action? Changing;
            public event Action? Changed;

            public Behavior<int> Get() => _current;

            Behavior IPropertyRef.Get() => _current;

            public void Set(Behavior<int> value)
            {
                Changing?.Invoke();
                _current = value;
                Changed?.Invoke();
            }

            void IPropertyRef.Set(Behavior value) => Set((Behavior<int>)value);

            public void OnUpdate(float progress) => Calls++;
        }

        private static StaticBehavior<int> Source()
        {
            var source = new StaticBehavior<int>();
            source.Activate();
            return source;
        }

        [Fact]
        public void WatchFollowsReplacementAndRemovesOldSource()
        {
            var oldSource = Source();
            var newSource = Source();
            var owner = new PropertyOwner(oldSource);

            new WatchRef(owner).OnUpdate += owner.OnUpdate;
            oldSource.Target = 1;
            Assert.Equal(1, owner.Calls);

            owner.Set(newSource);
            oldSource.Target = 2;
            newSource.Target = 3;

            Assert.Equal(2, owner.Calls);
        }

        [Fact]
        public void BindStopsOnReplacementAndDoesNotFollowNewSource()
        {
            var oldSource = Source();
            var newSource = Source();
            var owner = new PropertyOwner(oldSource);

            new BindRef(owner).OnUpdate += owner.OnUpdate;
            oldSource.Target = 1;
            owner.Set(newSource);
            oldSource.Target = 2;
            newSource.Target = 3;

            Assert.Equal(1, owner.Calls);
        }

        [Fact]
        public void ExplicitReleaseRemovesWatchSubscriptionImmediately()
        {
            var source = Source();
            var owner = new PropertyOwner(source);
            new WatchRef(owner).OnUpdate += owner.OnUpdate;

            FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscriptions(owner);
            source.Target = 1;

            Assert.Equal(0, owner.Calls);
        }

        [Fact]
        public void OwnerAnchorsDelegateDuringGcWhileStillAlive()
        {
            var source = Source();
            var owner = new PropertyOwner(source);
            new WatchRef(owner).OnUpdate += owner.OnUpdate;

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();
            source.Target = 1;

            Assert.Equal(1, owner.Calls);
            GC.KeepAlive(owner);
        }

        [Fact]
        public void StrongSubscriptionKeepsOwnerUntilExplicitRelease()
        {
            var source = Source();
            var weakOwner = SubscribeAndAbandonOwner(source);

            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            Assert.True(weakOwner.TryGetTarget(out var retained));
            source.Target = 1;
            Assert.Equal(1, retained.Calls);
            FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscriptions(retained);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static WeakReference<PropertyOwner> SubscribeAndAbandonOwner(
            StaticBehavior<int> source
        )
        {
            var owner = new PropertyOwner(source);
            new WatchRef(owner).OnUpdate += owner.OnUpdate;
            return new WeakReference<PropertyOwner>(owner);
        }

        [Fact]
        public void IndependentEventChannelsAndBindWatchCanCoexist()
        {
            var source = new StaticBehavior<int> { Duration = 1f };
            var owner = new PropertyOwner(source);
            int notifications = 0;
            Action callback = () => notifications++;

            new BindRef(owner).OnStart += callback;
            new BindRef(owner).OnComplete += callback;
            new WatchRef(owner).OnStart += callback;

            source.Activate();
            source.Update(1f);
            Assert.Equal(3, notifications);

            FlexVerseRoot.Instance.BehaviorRegistry.UnregisterSubscriptions(owner);
            source.Duration = 0f;
            source.Duration = 1f;
            source.Update(1f);

            Assert.Equal(3, notifications);
        }
    }
}
