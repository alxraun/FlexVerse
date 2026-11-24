// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Comprehensive lifecycle, sequencing, strategy-geometry and mutation
    /// tests for SequenceBehavior&lt;T&gt;. Deterministic: children are
    /// StaticBehavior&lt;float&gt; instances with explicit durations, so the
    /// whole chain is driven by hand-issued Update calls. No RimWorld/Verse/
    /// UnityEngine code paths are touched.
    /// </summary>
    public class SequenceBehaviorTests
    {
        private static StaticBehavior<float> Stage(float target, float duration = 1f)
        {
            return new StaticBehavior<float> { Target = target, Duration = duration };
        }

        [Fact]
        public void Constructor_FiltersNullChildren_AndPreservesOrder()
        {
            var first = Stage(1f);
            var second = Stage(2f);

            var sequence = new SequenceBehavior<float>(null, first, null, second, null);

            Assert.Equal(2, sequence.Children.Count);
            Assert.Same(first, sequence.Children[0]);
            Assert.Same(second, sequence.Children[1]);
        }

        [Fact]
        public void EmptySequence_Activate_CompletesImmediatelyWithStartAndCompleteOnce()
        {
            var sequence = new SequenceBehavior<float>();
            var starts = 0;
            var completions = 0;
            sequence.OnStart += () => starts++;
            sequence.OnComplete += () => completions++;

            sequence.Activate();
            sequence.Update(10f);

            Assert.False(sequence.IsUpdating);
            Assert.Equal(1, starts);
            Assert.Equal(1, completions);
        }

        [Fact]
        public void NullOnlyChildren_Activate_CompletesImmediately()
        {
            var sequence = new SequenceBehavior<float>(null, null);
            var completions = 0;
            sequence.OnComplete += () => completions++;

            sequence.Activate();
            sequence.Update(1f);

            Assert.Equal(1, completions);
            Assert.False(sequence.IsUpdating);
        }

        [Fact]
        public void Children_ActivateInOrder_EachAfterPreviousCompletes()
        {
            var c0 = Stage(10f, 0.5f);
            var c1 = Stage(20f, 0.5f);
            var c2 = Stage(30f, 0.5f);
            var sequence = new SequenceBehavior<float>(c0, c1, c2);

            var order = new List<string>();
            c0.OnActivate += () => order.Add("c0");
            c1.OnActivate += () => order.Add("c1");
            c2.OnActivate += () => order.Add("c2");

            sequence.Activate();
            Assert.Equal(new[] { "c0" }, order);

            sequence.Update(0.5f);
            Assert.Equal(new[] { "c0", "c1" }, order);

            sequence.Update(0.5f);
            Assert.Equal(new[] { "c0", "c1", "c2" }, order);

            sequence.Update(0.5f);
            Assert.False(sequence.IsUpdating);
        }

        [Fact]
        public void NextChild_StaysInactive_UntilCurrentChildCompletes()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1);

            sequence.Activate();
            sequence.Update(0.25f);

            Assert.Equal(Behavior.Lifecycle.Active, c0.State);
            Assert.Equal(Behavior.Lifecycle.Inactive, c1.State);
            Assert.True(sequence.IsUpdating);
        }

        [Fact]
        public void SequenceValue_TracksCurrentChildValue_DuringPlayback()
        {
            var c0 = Stage(10f, 0.5f);
            var c1 = Stage(20f, 0.5f);
            var sequence = new SequenceBehavior<float>(c0, c1);

            sequence.Activate();
            sequence.Update(0.1f);
            Assert.Equal(10f, sequence.Value, 5f);

            sequence.Update(0.5f);
            sequence.Update(0.1f);
            Assert.Equal(20f, sequence.Value, 5f);
        }

        [Fact]
        public void SequenceCompletes_AfterLastChild_OnCompleteFiresExactlyOnce()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 0.5f), Stage(2f, 0.5f));
            var completions = 0;
            sequence.OnComplete += () => completions++;

            sequence.Activate();
            sequence.Update(0.5f);
            sequence.Update(0.5f);

            Assert.False(sequence.IsUpdating);
            Assert.Equal(1, completions);

            sequence.Update(10f);
            Assert.Equal(1, completions);
        }

        [Fact]
        public void OnStartFires_BeforeFirstChildActivates_ResetDetachesObservers()
        {
            var c0 = Stage(1f, 1f);
            var sequence = new SequenceBehavior<float>(c0);
            var order = new List<string>();
            sequence.OnStart += () => order.Add("start");
            c0.OnActivate += () => order.Add("c0");

            sequence.Activate();
            Assert.Equal(new[] { "start", "c0" }, order);

            sequence.Reset();
            sequence.Activate();
            Assert.Equal(new[] { "start", "c0" }, order);

            sequence.Reset();
            sequence.OnStart += () => order.Add("start");
            c0.OnActivate += () => order.Add("c0");
            sequence.Activate();
            Assert.Equal(new[] { "start", "c0", "start", "c0" }, order);
        }

        [Fact]
        public void AggregatedProgress_IsProportionalAcrossChildren()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f), Stage(3f, 1f));
            var reports = new List<float>();
            sequence.OnUpdate += p => reports.Add(p);

            sequence.Activate();
            sequence.Update(0.25f);
            sequence.Update(0.75f);
            sequence.Update(1f);

            Assert.Equal(0.25f / 3f, reports[0], 5f);
            Assert.Equal(1f / 3f, reports[1], 5f);
            Assert.Equal(2f / 3f, reports[2], 5f);
        }

        [Fact]
        public void AggregatedProgress_NeverExceedsOne_AndEndsExactlyAtOne()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f), Stage(3f, 1f));
            var reports = new List<float>();
            sequence.OnUpdate += p => reports.Add(p);

            sequence.Activate();
            sequence.Update(0.5f);
            sequence.Update(0.5f);
            sequence.Update(0.5f);
            sequence.Update(0.5f);
            sequence.Update(0.5f);
            sequence.Update(0.5f);

            Assert.NotEmpty(reports);
            foreach (var report in reports)
            {
                Assert.True(
                    report >= 0f && report <= 1f,
                    $"Aggregated progress {report} escaped [0,1]."
                );
            }
            Assert.Equal(1f, reports[^1], 5f);
        }

        [Fact]
        public void Update_BeforeActivation_DoesNothing()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f));
            var updates = 0;
            sequence.OnUpdate += _ => updates++;

            sequence.Update(10f);

            Assert.Equal(0, updates);
            Assert.False(sequence.IsUpdating);
        }

        [Fact]
        public void ZeroDurationChild_DoesNotStallTheChain()
        {
            var c0 = Stage(10f, 0f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1);

            sequence.Activate();
            sequence.Update(1f);

            Assert.Equal(Behavior.Lifecycle.Active, c1.State);
        }

        [Fact]
        public void NullBehaviorChild_DoesNotStallTheChain()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, NullBehavior.Instance, c1);

            sequence.Activate();
            sequence.Update(1f);
            sequence.Update(1f);

            Assert.Equal(Behavior.Lifecycle.Active, c1.State);
        }

        /// <summary>
        /// Children [10, 20], retarget to 30 while child0 is active and the
        /// tracked origin is still 0 (no child tick yet). Proportional
        /// stretches each segment toward the new global target by its
        /// distance ratio: [0->10 at 1/3? no: 10/20 = 0.5] => [15, 30].
        /// </summary>
        [Fact]
        public void Retarget_Proportional_ResolvesStretchedSegmentTargets()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1);

            sequence.Activate();
            sequence.Target = 30f;

            Assert.Equal(15f, c0.Target, 5f);

            sequence.Update(1f);
            Assert.Equal(30f, c1.Target, 5f);

            sequence.Update(1f);
            Assert.Equal(30f, sequence.Value, 5f);
        }

        [Fact]
        public void Retarget_MidFlight_WaypointsLieOnCorridorFromLiveValue()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var c2 = Stage(30f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1, c2)
            {
                Strategy = new SequenceStrategy
                {
                    Elasticity = 1f,
                    Anchor = 0f,
                    Restart = false,
                },
            };

            sequence.Activate();
            sequence.Update(1.2f);
            sequence.Update(0.1f);

            sequence.Target = 40f;

            Assert.Equal(33.333f, c1.Target, 3f);

            sequence.Update(1f);
            Assert.Equal(40f, c2.Target, 5f);
            sequence.Update(1f);

            Assert.Equal(40f, sequence.Value, 5f);
        }

        [Fact]
        public void Retarget_MidFlightWithRestart_FirstCorridorPointIsForward()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var c2 = Stage(30f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1, c2);

            sequence.Activate();
            sequence.Update(1.2f);
            sequence.Update(0.1f);

            sequence.Target = 40f;

            Assert.Equal(26.667f, c0.Target, 3f);
        }

        [Fact]
        public void Retarget_FixedOffset_ShiftsEverySegmentByTheDelta()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1)
            {
                Strategy = SequenceStrategy.FixedOffset,
            };

            sequence.Activate();
            sequence.Target = 30f;

            Assert.Equal(20f, c0.Target, 5f);

            sequence.Update(1f);
            Assert.Equal(30f, c1.Target, 5f);

            sequence.Update(1f);
            Assert.Equal(30f, sequence.Value, 5f);
        }

        [Fact]
        public void Retarget_LastOnly_LastSegmentMustReachTheNewTarget()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1)
            {
                Strategy = SequenceStrategy.LastOnly,
            };

            sequence.Activate();
            sequence.Target = 30f;
            sequence.Update(1f);
            sequence.Update(1f);

            Assert.Equal(10f, c0.Target, 5f);
            Assert.Equal(30f, sequence.Value, 5f);
        }

        [Theory]
        [InlineData(0f, 0f)]
        [InlineData(0.000001f, 0f)]
        [InlineData(0f, 0.000001f)]
        [InlineData(0.25f, 0.75f)]
        [InlineData(0.5f, 0.5f)]
        [InlineData(1f, 0f)]
        [InlineData(0f, 1f)]
        [InlineData(1f, 1f)]
        public void Retarget_AllParameters_ReachCompositeTarget(float elasticity, float anchor)
        {
            foreach (bool restart in new[] { false, true })
            {
                var first = Stage(10f);
                var last = Stage(20f);
                var sequence = new SequenceBehavior<float>(first, last)
                {
                    Strategy = new SequenceStrategy
                    {
                        Elasticity = elasticity,
                        Anchor = anchor,
                        Restart = restart,
                    },
                };
                sequence.Activate();
                sequence.Target = 40f;
                sequence.Update(1f);
                Assert.Equal(40f, last.Target);
                sequence.Update(1f);
                Assert.Equal(40f, sequence.Value);
            }
        }

        [Fact]
        public void Retarget_NearLastOnly_IntermediateTargetHasNoBoundaryJump()
        {
            foreach (bool varyElasticity in new[] { false, true })
            {
                var first = Stage(10f);
                var last = Stage(20f);
                var sequence = new SequenceBehavior<float>(first, last)
                {
                    Strategy = new SequenceStrategy
                    {
                        Elasticity = varyElasticity ? 0.000001f : 0f,
                        Anchor = varyElasticity ? 0f : 0.000001f,
                        Restart = true,
                    },
                };
                sequence.Activate();
                sequence.Target = 40f;
                Assert.InRange(first.Target, 10f, 10.0001f);
                sequence.Update(1f);
                Assert.Equal(40f, last.Target);
            }
        }

        [Fact]
        public void Retarget_RestartFalse_ContinuesFromCurrentChild()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var c2 = Stage(30f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1, c2);
            var c0Activations = 0;
            c0.OnActivate += () => c0Activations++;

            sequence.Activate();
            sequence.Update(1.2f);

            sequence.Strategy = new SequenceStrategy
            {
                Elasticity = 1f,
                Anchor = 0f,
                Restart = false,
            };
            sequence.Target = 40f;

            sequence.Update(1f);
            sequence.Update(1f);

            Assert.False(sequence.IsUpdating);
            Assert.Equal(1, c0Activations);
        }

        [Fact]
        public void Retarget_RestartTrue_ReplaysTheChainViaLiveAdaptation()
        {
            var t0 = new TweenBehavior<float> { Target = 10f, Duration = 1f };
            var t1 = new TweenBehavior<float> { Target = 20f, Duration = 1f };
            var sequence = new SequenceBehavior<float>(t0, t1);
            var completions = new List<string>();
            t0.OnComplete += () => completions.Add("t0");
            t1.OnComplete += () => completions.Add("t1");

            sequence.Activate();
            sequence.Update(1.2f);
            sequence.Update(0.1f);
            Assert.Equal(new[] { "t0" }, completions);

            sequence.Target = 40f;

            sequence.Update(1f);
            sequence.Update(1f);

            Assert.Equal(new[] { "t0", "t0", "t1" }, completions);
            Assert.False(sequence.IsUpdating);
            Assert.Equal(40f, sequence.Value, 5f);
        }

        [Fact]
        public void TargetSet_WhileInactive_DoesNotActivate()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f));

            sequence.Target = 5f;

            Assert.Equal(Behavior.Lifecycle.Inactive, sequence.State);
            Assert.False(sequence.IsUpdating);
        }

        [Fact]
        public void TargetSet_EqualToCurrentLastChildTarget_IsIgnored()
        {
            var c0 = Stage(10f, 1f);
            var c1 = Stage(20f, 1f);
            var sequence = new SequenceBehavior<float>(c0, c1);
            var c0Activations = 0;
            c0.OnActivate += () => c0Activations++;

            sequence.Activate();
            sequence.Update(0.5f);
            sequence.Target = 20f;

            Assert.Equal(1, c0Activations);
            Assert.True(sequence.IsUpdating);
        }

        [Fact]
        public void BehaviorsSetter_WhileInactive_ReplacesTheChain()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f));
            var replacement = Stage(9f, 0.5f);

            sequence.Behaviors = new Behavior[] { replacement };
            sequence.Activate();
            sequence.Update(0.5f);

            Assert.Same(replacement, sequence.Children[0]);
            Assert.False(sequence.IsUpdating);
        }

        [Fact]
        public void BehaviorsSetter_WithIdenticalList_DoesNotRestartCurrentChild()
        {
            var c0 = Stage(10f, 1f);
            var sequence = new SequenceBehavior<float>(c0);
            var activations = 0;
            c0.OnActivate += () => activations++;

            sequence.Activate();
            sequence.Update(0.5f);
            sequence.Behaviors = new Behavior[] { c0 };

            Assert.Equal(1, activations);
            Assert.True(sequence.IsUpdating);
        }

        [Fact]
        public void BehaviorsSetter_WhileActive_WithoutPreviousChild_AdvancesToFirstNewChild()
        {
            var old = Stage(1f, 5f);
            var sequence = new SequenceBehavior<float>(old);
            var d0 = Stage(7f, 1f);
            var d1 = Stage(8f, 1f);

            sequence.Activate();
            sequence.Behaviors = new Behavior[] { d0, d1 };

            Assert.Equal(Behavior.Lifecycle.Active, d0.State);
            Assert.Equal(Behavior.Lifecycle.Inactive, d1.State);

            sequence.Update(1f);
            Assert.Equal(Behavior.Lifecycle.Active, d1.State);
        }

        [Fact]
        public void ValueSetter_ForwardsToCurrentChild()
        {
            var c0 = Stage(10f, 1f);
            var sequence = new SequenceBehavior<float>(c0);

            sequence.Activate();
            sequence.Value = 4f;

            Assert.Equal(4f, c0.Value, 5f);
        }

        [Fact]
        public void UnionWith_TypedSource_MergesTargetIntoFirstGenericChild()
        {
            var c0 = new StaticBehavior<float> { Duration = 1f };
            var sequence = new SequenceBehavior<float>(c0, Stage(2f, 1f));
            var source = new StaticBehavior<float> { Target = 42f, Value = 42f };

            sequence.UnionWith(source);

            Assert.Equal(42f, c0.Target, 5f);
            Assert.Equal(42f, sequence.Value, 5f);
        }

        [Fact]
        public void UnionWith_UntypedSource_IsNoOp()
        {
            var c0 = Stage(10f, 1f);
            var sequence = new SequenceBehavior<float>(c0);

            sequence.UnionWith(NullBehavior.Instance);

            Assert.Equal(10f, c0.Target, 5f);
        }

        [Fact]
        public void Reset_ReturnsSequenceAndChildrenToPooledInactiveState()
        {
            var c0 = Stage(10f, 0.5f);
            var c1 = Stage(20f, 0.5f);
            var sequence = new SequenceBehavior<float>(c0, c1);
            var completions = 0;
            sequence.OnComplete += () => completions++;

            sequence.Activate();
            sequence.Update(0.5f);
            sequence.Update(0.5f);
            Assert.Equal(1, completions);

            sequence.Reset();

            Assert.Equal(Behavior.Lifecycle.Inactive, sequence.State);
            Assert.False(sequence.IsUpdating);
            Assert.Equal(Behavior.Lifecycle.Inactive, c0.State);
            Assert.Equal(Behavior.Lifecycle.Inactive, c1.State);

            sequence.Activate();
            sequence.Update(10f);
            Assert.Equal(1, completions);
        }

        [Fact]
        public void Reset_DetachesSequenceUserHandlers()
        {
            var sequence = new SequenceBehavior<float>(Stage(1f, 1f));
            var updates = 0;
            sequence.OnUpdate += _ => updates++;

            sequence.Activate();
            sequence.Reset();
            sequence.Activate();
            sequence.Update(0.5f);

            Assert.Equal(0, updates);
        }

        [Fact]
        public void SemEquals_IdenticalChainsAndStrategy_AreEqual()
        {
            var a = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));
            var b = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));

            Assert.True(a.SemEquals(b));
            Assert.True(b.SemEquals(a));
        }

        [Fact]
        public void SemEquals_DiffersOnChildTarget_Count_AndStrategy()
        {
            var baseChildren = new[] { Stage(1f, 1f), Stage(2f, 1f) };
            var a = new SequenceBehavior<float>(baseChildren);

            var differentTarget = new SequenceBehavior<float>(Stage(1f, 1f), Stage(99f, 1f));
            var differentCount = new SequenceBehavior<float>(Stage(1f, 1f));
            var differentStrategy = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f))
            {
                Strategy = SequenceStrategy.LastOnly,
            };

            Assert.False(a.SemEquals(differentTarget));
            Assert.False(a.SemEquals(differentCount));
            Assert.False(a.SemEquals(differentStrategy));
            Assert.False(a.SemEquals(Stage(1f, 1f)));
        }

        [Fact]
        public void SemEquals_DifferingUserTargets_MustNotBeEqual()
        {
            var a = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));
            var b = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));

            a.Target = 30f;
            b.Target = 40f;

            Assert.False(
                a.SemEquals(b),
                "Sequences retargeted to different global targets are semantically distinct."
            );
        }

        [Fact]
        public void Clone_ProducesIndependentEqualInstance_WithClonedChildren()
        {
            var original = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));

            var clone = (SequenceBehavior<float>)original.Clone();

            Assert.NotSame(original, clone);
            Assert.True(original.SemEquals(clone));
            Assert.Equal(2, clone.Children.Count);
            Assert.NotSame(original.Children[0], clone.Children[0]);
            Assert.NotSame(original.Children[1], clone.Children[1]);
        }

        [Fact]
        public void Clone_CopiesUserTarget_AndStrategy()
        {
            var original = new SequenceBehavior<float>(Stage(1f, 1f))
            {
                Strategy = SequenceStrategy.FixedOffset,
            };
            original.Target = 30f;

            var clone = (SequenceBehavior<float>)original.Clone();

            Assert.Equal(30f, clone.Target, 5f);
            Assert.Equal(original.Strategy.Elasticity, clone.Strategy.Elasticity, 5f);
            Assert.Equal(original.Strategy.Anchor, clone.Strategy.Anchor, 5f);
            Assert.Equal(original.Strategy.Restart, clone.Strategy.Restart);
        }

        [Fact]
        public void Clone_MutationOfCloneChild_DoesNotAffectOriginal()
        {
            var original = new SequenceBehavior<float>(Stage(1f, 1f), Stage(2f, 1f));
            var clone = (SequenceBehavior<float>)original.Clone();

            ((StaticBehavior<float>)clone.Children[1]).Duration = 9f;

            Assert.Equal(1f, ((StaticBehavior<float>)original.Children[1]).Duration, 5f);
            Assert.False(original.SemEquals(clone));
        }

        [Fact]
        public void Clone_PlaysIndependentlyFromOriginal()
        {
            var original = new SequenceBehavior<float>(Stage(1f, 0.5f), Stage(2f, 0.5f));
            var clone = (SequenceBehavior<float>)original.Clone();

            original.Activate();
            original.Update(0.5f);
            original.Update(0.5f);
            Assert.False(original.IsUpdating);
            Assert.False(clone.IsUpdating);

            clone.Activate();
            clone.Update(0.5f);
            Assert.True(clone.IsUpdating, "Clone must run its own chain independently.");
        }

        [Fact]
        public void StrategyPresets_HaveDocumentedParameters()
        {
            Assert.Equal(1f, SequenceStrategy.Proportional.Elasticity, 5f);
            Assert.Equal(0f, SequenceStrategy.Proportional.Anchor, 5f);
            Assert.True(SequenceStrategy.Proportional.Restart);

            Assert.Equal(0f, SequenceStrategy.FixedOffset.Elasticity, 5f);
            Assert.Equal(1f, SequenceStrategy.FixedOffset.Anchor, 5f);
            Assert.True(SequenceStrategy.FixedOffset.Restart);

            Assert.Equal(0f, SequenceStrategy.LastOnly.Elasticity, 5f);
            Assert.Equal(0f, SequenceStrategy.LastOnly.Anchor, 5f);
            Assert.True(SequenceStrategy.LastOnly.Restart);
        }

        [Fact]
        public void Retarget_MidFlight_Vector2WaypointsLieOnCorridorFromLiveValue()
        {
            var c0 = Stage2D(new Vector2(2f, 0f), 1f);
            var c1 = Stage2D(new Vector2(7f, 0f), 1f);
            var c2 = Stage2D(new Vector2(13f, 0f), 1f);
            var sequence = new SequenceBehavior<Vector2>(c0, c1, c2)
            {
                Strategy = new SequenceStrategy
                {
                    Elasticity = 1f,
                    Anchor = 0f,
                    Restart = false,
                },
            };

            sequence.Activate();
            sequence.Update(1.2f);
            sequence.Update(0.1f);

            sequence.Target = new Vector2(11f, 22f);

            var corridorEnd = new Vector2(11f, 22f);
            var corridorStart = new Vector2(7f, 0f);
            Assert.Equal(
                0f,
                Vector2.Distance(
                    Vector2.LerpUnclamped(corridorStart, corridorEnd, 7f / 13f),
                    ((Behavior<Vector2>)sequence.Children[1]).Target
                ),
                3
            );

            sequence.Update(1f);
            Assert.Equal(
                0f,
                Vector2.Distance(corridorEnd, ((Behavior<Vector2>)sequence.Children[2]).Target),
                3
            );
            sequence.Update(1f);

            Assert.Equal(0f, Vector2.Distance(corridorEnd, sequence.Value), 5f);
        }

        private static StaticBehavior<Vector2> Stage2D(Vector2 target, float duration)
        {
            return new StaticBehavior<Vector2> { Target = target, Duration = duration };
        }
    }
}
