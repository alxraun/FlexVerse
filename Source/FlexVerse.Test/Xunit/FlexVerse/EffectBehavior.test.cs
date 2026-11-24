// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    public class EffectBehaviorTests
    {
        [Fact]
        public void ActivationTriggerExecutesOneSynchronousLifecycle()
        {
            var events = new List<string>();
            EffectBehavior? effect = null;
            effect = Effect.Create(() =>
            {
                Assert.NotNull(effect);
                Assert.True(effect.IsUpdating);
                events.Add("callback");
            });
            effect.OnActivate += () => events.Add("activate");
            effect.OnStart += () =>
            {
                Assert.True(effect.IsUpdating);
                events.Add("start");
            };
            effect.OnUpdate += progress =>
            {
                Assert.Equal(1f, progress);
                Assert.True(effect.IsUpdating);
                events.Add("update");
            };
            effect.OnComplete += () =>
            {
                Assert.False(effect.IsUpdating);
                events.Add("complete");
            };

            effect.Activate();
            effect.Activate();

            Assert.Equal(new[] { "activate", "start", "callback", "update", "complete" }, events);
            Assert.Equal(Behavior.Lifecycle.Active, effect.State);
            Assert.False(effect.IsUpdating);
        }

        [Fact]
        public void DependencyUpdateIsCoalescedToOncePerFrame()
        {
            var dependency = new StaticBehavior<int>();
            dependency.Activate();

            int calls = 0;
            var events = new List<string>();
            var effect = Effect.Create(
                () =>
                {
                    calls++;
                    events.Add("callback");
                },
                EffectTrigger.OnUpdate,
                dependency
            );
            effect.OnStart += () => events.Add("start");
            effect.OnUpdate += p =>
            {
                Assert.Equal(1f, p);
                events.Add("update");
            };
            effect.OnComplete += () => events.Add("complete");
            effect.Activate();

            dependency.Target = 1;
            dependency.Target = 2;

            Assert.Equal(1, calls);
            Assert.Equal(new[] { "start", "callback", "update", "complete" }, events);
            Assert.False(effect.IsUpdating);

            effect.Update(10f);
            Assert.Equal(1, calls);
        }

        [Fact]
        public void CompletionTriggerUsesDependencyOnComplete()
        {
            var dependency = new StaticBehavior<float> { Duration = 1f, Target = 5f };
            dependency.Activate();

            int calls = 0;
            var effect = Effect.Create(() => calls++, EffectTrigger.OnComplete, dependency);
            effect.Activate();
            dependency.Update(1f);

            Assert.Equal(1, calls);
        }

        [Fact]
        public void ResetDetachesDependenciesWithoutResettingThem()
        {
            var dependency = new StaticBehavior<int>();
            dependency.Activate();

            int calls = 0;
            var effect = Effect.Create(() => calls++, EffectTrigger.OnUpdate, dependency);
            effect.Activate();
            effect.Reset();
            dependency.Target = 2;

            Assert.Equal(0, calls);
            Assert.Equal(Behavior.Lifecycle.Active, dependency.State);
            Assert.Equal(Behavior.Lifecycle.Inactive, effect.State);
        }

        [Fact]
        public void CallbackExceptionStillClosesLifecycle()
        {
            var effect = Effect.Create(() => throw new InvalidOperationException("fail"));
            int completions = 0;
            effect.OnComplete += () => completions++;

            Assert.Throws<InvalidOperationException>(() => effect.Activate());
            Assert.False(effect.IsUpdating);
            Assert.Equal(1, completions);
        }

        [Fact]
        public void RecursiveDependencyTriggerDoesNotExecuteEffectTwice()
        {
            var dependency = new StaticBehavior<int>();
            dependency.Activate();
            int calls = 0;
            var effect = Effect.Create(
                () =>
                {
                    calls++;
                    dependency.Target++;
                },
                EffectTrigger.OnUpdate,
                dependency
            );
            effect.Activate();

            dependency.Target = 1;

            Assert.Equal(1, calls);
            Assert.False(effect.IsUpdating);
        }
    }
}
