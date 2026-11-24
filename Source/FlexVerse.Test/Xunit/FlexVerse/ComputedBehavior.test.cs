// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    public class ComputedBehaviorTests
    {
        private static StaticBehavior<int> Source(int value = 1)
        {
            return new StaticBehavior<int> { Target = value };
        }

        [Fact]
        public void ReadsAreLazyMemoizedAndRecomputedAfterInvalidation()
        {
            var source = Source(2);
            int calls = 0;
            var computed = new ComputedBehavior<int>(
                () =>
                {
                    calls++;
                    return source.Value * 3;
                },
                source
            );

            Assert.Equal(0, calls);
            source.Activate();
            computed.Activate();

            Assert.Equal(6, computed.Value);
            Assert.Equal(6, computed.Value);
            Assert.Equal(1, calls);

            source.Target = 4;

            Assert.Equal(1, calls);
            Assert.Equal(12, computed.Value);
            Assert.Equal(2, calls);
        }

        [Fact]
        public void ActivationEmitsOnlyOnActivateAndDiscardsPreActivationCache()
        {
            var source = Source(2);
            source.Activate();
            var computed = source.Select(x => x + 1);
            Assert.Equal(3, computed.Value);

            var events = new List<string>();
            computed.OnActivate += () => events.Add("activate");
            computed.OnStart += () => events.Add("start");
            computed.OnUpdate += _ => events.Add("update");
            computed.OnComplete += () => events.Add("complete");

            source.Target = 5;
            computed.Activate();
            computed.Activate();

            Assert.Equal(new[] { "activate" }, events);
            Assert.False(computed.IsUpdating);
            Assert.Equal(6, computed.Value);
        }

        [Fact]
        public void ActiveInvalidationEmitsOneCompleteCycleWithoutEvaluating()
        {
            var source = Source();
            int calls = 0;
            var computed = new ComputedBehavior<int>(
                () =>
                {
                    calls++;
                    return source.Value * 2;
                },
                source
            );

            source.Activate();
            computed.Activate();
            Assert.Equal(2, computed.Value);

            var events = new List<string>();
            computed.OnStart += () =>
            {
                Assert.True(computed.IsUpdating);
                events.Add("start");
            };
            computed.OnUpdate += progress =>
            {
                Assert.Equal(1f, progress);
                Assert.True(computed.IsUpdating);
                events.Add("update");
            };
            computed.OnComplete += () =>
            {
                Assert.False(computed.IsUpdating);
                events.Add("complete");
            };

            source.Target = 3;
            source.Target = 4;

            Assert.Equal(new[] { "start", "update", "complete" }, events);
            Assert.Equal(1, calls);
            Assert.Equal(8, computed.Value);
            Assert.Equal(2, calls);
        }

        [Fact]
        public void ReadInsideUpdateThenAnotherInvalidationQueuesASecondCycle()
        {
            var source = Source();
            source.Activate();
            var computed = source.Select(x => x * 2);
            computed.Activate();
            Assert.Equal(2, computed.Value);

            var events = new List<string>();
            bool changedAgain = false;
            computed.OnStart += () => events.Add("start");
            computed.OnUpdate += _ =>
            {
                events.Add("update");
                if (!changedAgain)
                {
                    changedAgain = true;
                    Assert.Equal(4, computed.Value);
                    source.Target = 3;
                }
            };
            computed.OnComplete += () => events.Add("complete");

            source.Target = 2;

            Assert.Equal(
                new[] { "start", "update", "complete", "start", "update", "complete" },
                events
            );
            Assert.False(computed.IsUpdating);
            Assert.Equal(6, computed.Value);
        }

        [Fact]
        public void RepeatedUpdatesWhileDirtyAreIgnoredAcrossChainedComputed()
        {
            var source = Source();
            source.Activate();
            var first = source.Select(x => x + 1);
            var second = first.Select(x => x * 2);
            first.Activate();
            second.Activate();
            Assert.Equal(4, second.Value);

            int firstUpdates = 0,
                secondUpdates = 0;
            first.OnUpdate += _ => firstUpdates++;
            second.OnUpdate += _ => secondUpdates++;

            source.Target = 2;
            source.Target = 3;

            Assert.Equal(1, firstUpdates);
            Assert.Equal(1, secondUpdates);
            Assert.Equal(8, second.Value);

            source.Target = 4;
            Assert.Equal(2, firstUpdates);
            Assert.Equal(2, secondUpdates);
            Assert.Equal(10, second.Value);
        }

        [Fact]
        public void ComputedBorrowsDependenciesAndResetDetachesSubscriptions()
        {
            var source = Source();
            var computed = source.Select(x => x * 2);

            computed.Activate();
            Assert.Equal(Behavior.Lifecycle.Inactive, source.State);

            source.Activate();
            Assert.Equal(2, computed.Value);
            int updates = 0;
            computed.OnUpdate += _ => updates++;

            computed.Reset();
            source.Target = 5;

            Assert.Equal(0, updates);
            Assert.Equal(Behavior.Lifecycle.Active, source.State);
            Assert.Equal(Behavior.Lifecycle.Inactive, computed.State);
            Assert.Equal(10, computed.Value);
        }

        [Fact]
        public void ValueAssignmentIsAnOverrideUntilInvalidationAndTargetIsPassive()
        {
            var source = Source();
            source.Activate();
            var computed = source.Select(x => x * 10);
            computed.Activate();
            Assert.Equal(10, computed.Value);

            computed.Value = 91;
            computed.Target = 200;
            Assert.Equal(91, computed.Value);
            Assert.Equal(200, computed.Target);
            Assert.False(computed.IsUpdating);

            source.Target = 2;
            Assert.Equal(20, computed.Value);
            Assert.Equal(200, computed.Target);
        }

        [Fact]
        public void GroupCombineAndSelectUseExistingBehaviorInstances()
        {
            var a = Source(2);
            var b = Source(3);
            var c = Source(4);
            var d = Source(5);
            a.Activate();
            b.Activate();
            c.Activate();
            d.Activate();

            var two = Group.Combine(a, b, (x, y) => x + y);
            var three = Group.Combine(a, b, c, (x, y, z) => x + y + z);
            var four = Group.Combine(a, b, c, d, (w, x, y, z) => w + x + y + z);
            var arbitrary = Group.Combine(() => a.Value + b.Value, a, b);

            Assert.Equal(5, two.Value);
            Assert.Equal(9, three.Value);
            Assert.Equal(14, four.Value);
            Assert.Equal(5, arbitrary.Value);
            Assert.Same(a, two.Dependencies[0]);
            Assert.Same(b, two.Dependencies[1]);
        }

        [Fact]
        public void RepeatedDependencyReferenceSubscribesOnlyOnce()
        {
            var source = Source();
            source.Activate();
            var computed = new ComputedBehavior<int>(() => source.Value, source, source);
            computed.Activate();
            Assert.Single(computed.Dependencies);
            Assert.Equal(1, computed.Value);

            int updates = 0;
            computed.OnUpdate += _ => updates++;
            source.Target = 2;
            Assert.Equal(1, updates);
        }

        [Fact]
        public void SemEqualsAndCloneShareDependenciesButNotCache()
        {
            var source = Source();
            source.Activate();
            Func<int> compute = () => source.Value + 1;
            var first = new ComputedBehavior<int>(compute, source) { Target = 9 };
            var second = new ComputedBehavior<int>(compute, source) { Target = 9 };

            Assert.True(first.SemEquals(second));
            var clone = (ComputedBehavior<int>)first.Clone();
            Assert.NotSame(first, clone);
            Assert.True(first.SemEquals(clone));
            Assert.Same(source, clone.Dependencies[0]);
            Assert.Equal(2, clone.Value);

            var different = new ComputedBehavior<int>(compute, Source());
            Assert.False(first.SemEquals(different));
        }

        [Fact]
        public void SemEqualsDoesNotDependOnWhichCacheWasEvaluated()
        {
            var source = Source();
            source.Activate();
            Func<int> computer = () => source.Value + 1;
            var first = new ComputedBehavior<int>(computer, source);
            var second = new ComputedBehavior<int>(computer, source);

            first.Value = 100;
            second.Value = 100;
            first.Activate();
            second.Activate();

            Assert.True(first.SemEquals(second));
            Assert.Equal(2, first.Value);
            Assert.True(first.SemEquals(second));

            source.Target = 5;
            Assert.True(first.SemEquals(second));
            Assert.Equal(6, second.Value);
            Assert.True(first.SemEquals(second));
        }

        [Fact]
        public void UnionWithTransfersTargetButNeverUsesForeignValueAsDerivedResult()
        {
            var source = Source(5);
            source.Activate();
            var previous = new StaticBehavior<int> { Target = 99 };
            previous.Activate();
            var computed = source.Select(x => x * 3);

            computed.UnionWith(previous);

            Assert.Equal(99, computed.Target);
            Assert.Equal(15, computed.Value);
        }

        [Fact]
        public void FailedEvaluationRemainsDirtyAndCanBeRetried()
        {
            int calls = 0;
            var computed = new ComputedBehavior<int>(() =>
            {
                if (++calls == 1)
                    throw new InvalidOperationException("compute failed");
                return 42;
            });

            Assert.Throws<InvalidOperationException>(() => _ = computed.Value);
            Assert.Equal(42, computed.Value);
            Assert.Equal(2, calls);
            Assert.Equal(42, computed.Value);
            Assert.Equal(2, calls);
        }

        [Fact]
        public void RecursiveEvaluationThrowsInsteadOfOverflowingStack()
        {
            ComputedBehavior<int> computed = null!;
            computed = new ComputedBehavior<int>(() => computed.Value);

            Assert.Throws<InvalidOperationException>(() => _ = computed.Value);
        }

        [Fact]
        public void ThrowingEventObserverDoesNotLeaveIsUpdatingSet()
        {
            var source = Source();
            source.Activate();
            var computed = source.Select(x => x);
            computed.Activate();
            Assert.Equal(1, computed.Value);

            computed.OnStart += () => throw new InvalidOperationException("observer failure");

            Assert.Throws<InvalidOperationException>(() => source.Target = 2);
            Assert.False(computed.IsUpdating);
            Assert.Equal(Behavior.Lifecycle.Active, computed.State);
        }

        [Fact]
        public void TickNeverComputesOrEmitsLifecycleEvents()
        {
            int calls = 0;
            var computed = new ComputedBehavior<int>(() => ++calls);
            var events = new List<string>();
            computed.OnStart += () => events.Add("start");
            computed.OnUpdate += _ => events.Add("update");
            computed.OnComplete += () => events.Add("complete");

            computed.Activate();
            computed.Update(100f);

            Assert.Empty(events);
            Assert.Equal(0, calls);
            Assert.Equal(1, computed.Value);
        }
    }
}
