// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    public class TimelineTests
    {
        private sealed class ManualBehavior : Behavior<float>
        {
            public override void Activate()
            {
                if (State == Lifecycle.Active)
                    return;
                State = Lifecycle.Active;
                _onActivate?.Invoke();
                StartUpdating();
            }

            public void Finish() => StopUpdating();

            public override void Update(float deltaTime) { }

            public override void UnionWith(Behavior source) { }

            public override bool SemEquals(Behavior other) => ReferenceEquals(this, other);

            public override Behavior<float> Clone() => new ManualBehavior();
        }

        private sealed class InstantBehavior : Behavior<float>
        {
            public override void Activate()
            {
                if (State == Lifecycle.Active)
                    return;
                State = Lifecycle.Active;
                _onActivate?.Invoke();
                StartUpdating();
                StopUpdating();
            }

            public override void Update(float deltaTime) { }

            public override void UnionWith(Behavior source) { }

            public override bool SemEquals(Behavior other) => ReferenceEquals(this, other);

            public override Behavior<float> Clone() => new InstantBehavior();
        }

        private sealed class EquivalentBehavior : Behavior<float>
        {
            public override void Activate()
            {
                if (State == Lifecycle.Active)
                    return;
                State = Lifecycle.Active;
                _onActivate?.Invoke();
                StartUpdating();
            }

            public void Finish() => StopUpdating();

            public override void Update(float deltaTime) { }

            public override void UnionWith(Behavior source) { }

            public override bool SemEquals(Behavior other) => other is EquivalentBehavior;

            public override Behavior<float> Clone() => new EquivalentBehavior();
        }

        private sealed class RefSlot : IPropertyRef<float>
        {
            private Behavior<float>? _current;

            public RefSlot(Behavior<float>? current = null) => _current = current;

            public string Name => "Value";
            public object Owner => this;
            public event Action? Changing;
            public event Action? Changed;

            public Behavior<float> Get() => _current!;

            Behavior IPropertyRef.Get() => Get();

            public void Set(Behavior<float> value)
            {
                var old = _current;
                if (old != null && old.SemEquals(value))
                    return;

                Changing?.Invoke();
                _current = value;
                Changed?.Invoke();
                value?.Activate();
                old?.Reset();
            }

            void IPropertyRef.Set(Behavior value) => Set((Behavior<float>)value);
        }

        [Fact]
        public void TimelineIsInertUntilActivatedAndCompletesThroughBehaviorLifecycle()
        {
            int calls = 0,
                starts = 0,
                completes = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Call(() => calls++),
                    Timeline.Wait(1f),
                    Timeline.Call(() => calls++)
                )
            );
            timeline.OnStart += () => starts++;
            timeline.OnComplete += () => completes++;

            Assert.Equal(0, calls);
            Assert.False(timeline.IsUpdating);

            timeline.Activate();
            Assert.Equal(1, calls);
            Assert.Equal(1, starts);
            Assert.True(timeline.IsUpdating);

            timeline.Update(1f);
            Assert.Equal(2, calls);
            Assert.False(timeline.IsUpdating);
            Assert.Equal(1, completes);

            timeline.Activate();
            timeline.Stop();
            Assert.Equal(2, calls);
            Assert.Equal(1, completes);
        }

        [Fact]
        public void ParallelWaitsForAllChildren()
        {
            int complete = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Parallel(Timeline.Wait(2f), Timeline.Wait(1f)),
                    Timeline.Call(() => complete++)
                )
            );

            timeline.Activate();
            timeline.Update(1f);
            Assert.Equal(0, complete);
            timeline.Update(1f);
            Assert.Equal(1, complete);
            Assert.False(timeline.IsUpdating);
        }

        [Fact]
        public void SetWaitsForInstalledBehaviorOnComplete()
        {
            var slot = new RefSlot();
            int next = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Set(slot, new ManualBehavior()),
                    Timeline.Call(() => next++)
                )
            );

            timeline.Activate();
            Assert.IsType<ManualBehavior>(slot.Get());
            Assert.True(timeline.IsUpdating);
            Assert.Equal(0, next);

            ((ManualBehavior)slot.Get()).Finish();
            timeline.Update(0f);
            Assert.Equal(1, next);
            Assert.False(timeline.IsUpdating);
        }

        [Fact]
        public void SetCatchesSynchronousOnCompleteDuringAssignment()
        {
            var slot = new RefSlot();
            int next = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Set(slot, new InstantBehavior()),
                    Timeline.Call(() => next++)
                )
            );

            timeline.Activate();
            Assert.Equal(1, next);
            Assert.False(timeline.IsUpdating);
        }

        [Fact]
        public void SemanticallyEqualAssignmentWaitsForRetainedInstance()
        {
            var old = new EquivalentBehavior();
            old.Activate();
            var slot = new RefSlot(old);
            int next = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Set(slot, new EquivalentBehavior()),
                    Timeline.Call(() => next++)
                )
            );

            timeline.Activate();
            Assert.Same(old, slot.Get());
            Assert.Equal(0, next);

            old.Finish();
            timeline.Update(0f);
            Assert.Equal(1, next);
        }

        [Fact]
        public void ReplacementInterruptsTimelineWithoutCompletingDependentActions()
        {
            var slot = new RefSlot();
            int next = 0,
                completes = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Set(slot, new ManualBehavior()),
                    Timeline.Call(() => next++)
                )
            );
            timeline.OnComplete += () => completes++;

            timeline.Activate();
            var replacement = new ManualBehavior();
            slot.Set(replacement);
            timeline.Update(0f);

            Assert.Equal(0, next);
            Assert.False(timeline.IsUpdating);
            Assert.Equal(1, completes);
            Assert.True(replacement.IsUpdating);
        }

        [Fact]
        public void StopUnsubscribesTimelineButDoesNotResetAssignedBehavior()
        {
            var slot = new RefSlot();
            int next = 0,
                completes = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Set(slot, new ManualBehavior()),
                    Timeline.Call(() => next++)
                )
            );
            timeline.OnComplete += () => completes++;

            timeline.Activate();
            var assigned = (ManualBehavior)slot.Get();
            timeline.Stop();
            timeline.Update(0f);
            assigned.Finish();

            Assert.False(timeline.IsUpdating);
            Assert.Equal(0, next);
            Assert.Equal(1, completes);
            Assert.Equal(Behavior.Lifecycle.Active, assigned.State);
        }

        [Fact]
        public void ResetStopsOrchestrationAndNotifiesUpdaterOnce()
        {
            int next = 0,
                completes = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(Timeline.Wait(10f), Timeline.Call(() => next++))
            );
            timeline.OnComplete += () => completes++;

            timeline.Activate();
            timeline.Reset();

            Assert.Equal(0, next);
            Assert.Equal(1, completes);
            Assert.Equal(Behavior.Lifecycle.Inactive, timeline.State);
        }

        [Fact]
        public void IfWaitUntilAndWhileComposeWithoutUnboundedInstantLoop()
        {
            bool ready = false;
            int selected = 0,
                iterations = 0;
            var timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.WaitUntil(() => ready),
                    Timeline.If(
                        () => true,
                        Timeline.Call(() => selected++),
                        Timeline.Call(() => selected += 100)
                    ),
                    Timeline.While(() => iterations < 3, Timeline.Call(() => iterations++))
                )
            );

            timeline.Activate();
            timeline.Update(0f);
            Assert.Equal(0, selected);

            ready = true;
            timeline.Update(0f);
            Assert.Equal(1, selected);
            Assert.Equal(1, iterations);

            timeline.Update(0f);
            Assert.Equal(2, iterations);
            timeline.Update(0f);
            Assert.Equal(3, iterations);
            timeline.Update(0f);
            Assert.False(timeline.IsUpdating);
        }

        [Fact]
        public void CallbackCanStopOwnTimelineWithoutRunningNextAction()
        {
            TimelineBehavior? timeline = null;
            int next = 0;
            timeline = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Call(() => timeline!.Stop()),
                    Timeline.Call(() => next++)
                )
            );

            timeline!.Activate();
            Assert.Equal(0, next);
            Assert.False(timeline.IsUpdating);
        }

        [Fact]
        public void CloneDoesNotShareExecutionState()
        {
            int starts = 0;
            var original = Timeline.Create(
                Timeline.Sequence(Timeline.Call(() => starts++), Timeline.Wait(1f))
            );
            var clone = (TimelineBehavior)original.CloneUntyped();

            original.Activate();
            clone.Activate();
            Assert.Equal(2, starts);
            original.Update(1f);
            Assert.False(original.IsUpdating);
            Assert.True(clone.IsUpdating);
            clone.Update(1f);
            Assert.False(clone.IsUpdating);
        }
    }
}
