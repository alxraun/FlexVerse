// parents: @.frostline/graph/m-xunit.xml

using System.Collections.Generic;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Lifecycle tests for StaticBehavior&lt;T&gt; using float targets.
    /// These paths never touch BindRef/WatchRef/FlexVerseRoot or any
    /// RimWorld/Verse/UnityEngine type.
    /// </summary>
    public class StaticBehaviorTests
    {
        private static StaticBehavior<float> NewBehavior(float target, float duration)
        {
            return new StaticBehavior<float> { Target = target, Duration = duration };
        }

        [Fact]
        public void Activate_SetsStateToActive_AndFiresOnActivateExactlyOnce()
        {
            var behavior = NewBehavior(target: 10f, duration: 1f);
            var activations = 0;
            behavior.OnActivate += () => activations++;

            Assert.Equal(Behavior.Lifecycle.Inactive, behavior.State);

            behavior.Activate();
            behavior.Activate();

            Assert.Equal(Behavior.Lifecycle.Active, behavior.State);
            Assert.Equal(1, activations);
        }

        [Fact]
        public void Update_ReportsProgressProportionallyViaOnUpdate_AndCompletesAfterDuration()
        {
            var behavior = NewBehavior(target: 5f, duration: 1f);
            var progressReports = new List<float>();
            var completions = 0;
            behavior.OnUpdate += p => progressReports.Add(p);
            behavior.OnComplete += () => completions++;

            behavior.Activate();
            behavior.Update(0.25f);
            behavior.Update(0.50f);
            behavior.Update(0.50f);

            Assert.Equal(new[] { 0.25f, 0.75f, 1.25f }, progressReports);
            Assert.Equal(1, completions);
            Assert.False(
                behavior.IsUpdating,
                "Behavior must stop updating after duration elapses."
            );
        }

        [Fact]
        public void Update_BeforeActivation_IsIgnored()
        {
            var behavior = NewBehavior(target: 1f, duration: 1f);
            var updates = 0;
            behavior.OnUpdate += _ => updates++;

            behavior.Update(10f);

            Assert.Equal(0, updates);
            Assert.False(behavior.IsUpdating);
        }

        [Fact]
        public void Activate_WithZeroDuration_CompletesImmediatelyWithoutOnUpdate()
        {
            var behavior = NewBehavior(target: 1f, duration: 0f);
            var updates = 0;
            var completions = 0;
            var starts = 0;
            behavior.OnUpdate += _ => updates++;
            behavior.OnComplete += () => completions++;
            behavior.OnStart += () => starts++;

            behavior.Activate();

            Assert.Equal(Behavior.Lifecycle.Active, behavior.State);
            Assert.False(behavior.IsUpdating, "Zero duration must never enter the updating state.");
            Assert.Equal(0, starts);
            Assert.Equal(0, updates);
            Assert.Equal(0, completions);

            behavior.Update(1f);
            Assert.Equal(0, updates);
        }

        [Fact]
        public void Activate_WithNegativeDuration_CompletesImmediately()
        {
            var behavior = NewBehavior(target: 1f, duration: -5f);
            behavior.Activate();

            Assert.False(behavior.IsUpdating);
        }

        [Fact]
        public void TargetSetWhileActive_WithZeroDuration_FiresOnUpdateWithOne()
        {
            var behavior = new StaticBehavior<float> { Duration = 0f };
            var updates = new List<float>();
            behavior.OnUpdate += p => updates.Add(p);

            behavior.Activate();
            behavior.Target = 42f;

            Assert.Equal(new[] { 1f }, updates);
        }

        [Fact]
        public void DurationSetWhileActive_FromZeroToPositive_RestartsUpdatingFromZero()
        {
            var behavior = NewBehavior(target: 1f, duration: 0f);
            var starts = 0;
            behavior.OnStart += () => starts++;

            behavior.Activate();
            behavior.Duration = 2f;

            Assert.True(behavior.IsUpdating);
            Assert.Equal(1, starts);

            behavior.Update(2f);
            Assert.False(behavior.IsUpdating);
        }

        [Fact]
        public void Reset_ReturnsToInactiveState_AndDetachesAllHandlers()
        {
            var behavior = NewBehavior(target: 7f, duration: 1f);
            var activations = 0;
            var updates = 0;
            behavior.OnActivate += () => activations++;
            behavior.OnUpdate += _ => updates++;

            behavior.Activate();
            behavior.Reset();

            Assert.Equal(Behavior.Lifecycle.Inactive, behavior.State);
            Assert.False(behavior.IsUpdating);
            Assert.Equal(0f, behavior.Duration, 5f);

            behavior.Activate();
            behavior.Update(5f);

            Assert.Equal(1, activations);
            Assert.Equal(0, updates);
        }

        [Fact]
        public void SemEquals_BehaviorsWithSameTargetAndDuration_AreEqual_AndDifferOnDuration()
        {
            var a = NewBehavior(target: 3f, duration: 1f);
            var b = NewBehavior(target: 3f, duration: 1f);
            var differentDuration = NewBehavior(target: 3f, duration: 2f);

            Assert.True(a.SemEquals(b));
            Assert.False(a.SemEquals(differentDuration));
        }

        [Fact]
        public void Clone_CopiesDurationAndUserTarget_ButIsIndependentInstance()
        {
            var original = NewBehavior(target: 9f, duration: 3f);
            var clone = (StaticBehavior<float>)original.Clone();

            Assert.NotSame(original, clone);
            Assert.Equal(original.Duration, clone.Duration, 5f);
            Assert.Equal(original.Target, clone.Target);
            Assert.True(original.SemEquals(clone));
        }
    }
}
