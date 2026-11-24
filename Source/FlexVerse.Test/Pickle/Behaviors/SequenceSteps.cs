// parents: @.frostline/graph/m-pickle.xml

using System;
using System.Collections.Generic;
using System.Text;
using FlexVerse;
using RimWorks.Pickle;

namespace FlexVerse.Test.Pickle
{
    [PickleSteps]
    public static class SequenceBehaviorSteps
    {
        private static SequenceBehavior<float> _sequence;
        private static int _sequenceCompletions;
        private static int[] _childCompletions;
        private static List<string> _playOrder;

        [BeforeScenario]
        public static void Reset(PickleContext ctx)
        {
            _sequence = null;
            _sequenceCompletions = 0;
            _childCompletions = null;
            _playOrder = null;
            ScenarioMarkers.Begin("M-SEQUENCE-BEHAVIOR");
        }

        [AfterScenario]
        public static void End(PickleContext ctx)
        {
            ScenarioMarkers.End("M-SEQUENCE-BEHAVIOR");
        }

        [Given(
            "an active sequence of 3 tweens toward {float}, {float} and {float} each {float} seconds"
        )]
        public static void GivenSequence(
            PickleContext ctx,
            float first,
            float second,
            float third,
            float duration
        )
        {
            var targets = new[] { first, second, third };
            var children = new TweenBehavior<float>[3];
            _childCompletions = new int[3];
            _playOrder = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                var index = i;
                children[index] = new TweenBehavior<float>
                {
                    Target = targets[index],
                    Duration = duration,
                    Easing = FlexVerse.Easing.Linear,
                };
                children[index].OnStart += () => _playOrder.Add($"T{index}:start");
                children[index].OnComplete += () =>
                {
                    _childCompletions[index]++;
                    _playOrder.Add($"T{index}:complete");
                };
            }

            _sequence = new SequenceBehavior<float>(children)
            {
                Strategy = SequenceStrategy.Proportional,
                Value = 0f,
            };
            _sequence.OnComplete += () => _sequenceCompletions++;
            _sequence.Activate();
        }

        [When("the sequence advances for a total of {float} seconds in {float} second steps")]
        public static void AdvanceTotal(PickleContext ctx, float totalSeconds, float stepSeconds)
        {
            ctx.Require(_sequence != null, "no sequence behavior was created");
            var remaining = totalSeconds;
            while (remaining > 0f)
            {
                var step = Math.Min(stepSeconds, remaining);
                _sequence.Update(step);
                remaining -= step;
            }
            Log.Info("M-SEQUENCE-BEHAVIOR", "sequence.value", ("value", _sequence.Value));
        }

        [When("the sequence target changes to {float}")]
        public static void Retarget(PickleContext ctx, float target)
        {
            ctx.Require(_sequence != null, "no sequence behavior was created");
            _sequence.Target = target;
            Log.Info(
                "M-SEQUENCE-BEHAVIOR",
                "sequence.retargeted",
                ("target", target),
                ("value", _sequence.Value)
            );
        }

        [Then("the sequence value is {float} within {float}")]
        public static void ValueIs(PickleContext ctx, float expected, float tolerance)
        {
            ctx.Require(_sequence != null, "no sequence behavior was created");
            Log.Info(
                "M-SEQUENCE-BEHAVIOR",
                "verify:value",
                ("expected", expected),
                ("actual", _sequence.Value)
            );
            ctx.Require(
                Math.Abs(_sequence.Value - expected) <= tolerance,
                $"expected sequence value {expected} +/- {tolerance}, got {_sequence.Value}"
            );
        }

        [Then("the sequence reports completion exactly once")]
        public static void CompletedOnce(PickleContext ctx)
        {
            Log.Info("M-SEQUENCE-BEHAVIOR", "verify:complete", ("count", _sequenceCompletions));
            ctx.Require(
                _sequenceCompletions == 1,
                $"expected exactly one sequence completion, got {_sequenceCompletions}"
            );
        }

        [Then("the sequence reports completion exactly twice")]
        public static void CompletedTwice(PickleContext ctx)
        {
            Log.Info("M-SEQUENCE-BEHAVIOR", "verify:complete", ("count", _sequenceCompletions));
            ctx.Require(
                _sequenceCompletions == 2,
                $"expected exactly two sequence completions, got {_sequenceCompletions}"
            );
        }

        [Then("the children played strictly in order")]
        public static void ChildrenInOrder(PickleContext ctx)
        {
            var expected = new[]
            {
                "T0:start",
                "T0:complete",
                "T1:start",
                "T1:complete",
                "T2:start",
                "T2:complete",
            };
            var actual = string.Join(",", _playOrder);
            Log.Info("M-SEQUENCE-BEHAVIOR", "verify:order", ("actual", actual));
            ctx.Require(
                _playOrder.Count == expected.Length && Matches(expected),
                $"expected strict play order [{string.Join(",", expected)}], got [{actual}]"
            );
        }

        [Then("the children completed exactly {int}, {int} and {int} times")]
        public static void ChildCompletions(PickleContext ctx, int first, int second, int third)
        {
            var expected = new[] { first, second, third };
            var builder = new StringBuilder("verify:children ");
            for (int i = 0; i < expected.Length; i++)
            {
                if (i > 0)
                    builder.Append(',');
                builder.Append($"T{i}={_childCompletions[i]}");
            }
            Log.Info("M-SEQUENCE-BEHAVIOR", "verify:children", ("counts", builder.ToString()));
            for (int i = 0; i < expected.Length; i++)
            {
                ctx.Require(
                    _childCompletions[i] == expected[i],
                    $"child T{i} completed {_childCompletions[i]} times, expected {expected[i]}"
                );
            }
        }

        private static bool Matches(string[] expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                if (_playOrder[i] != expected[i])
                    return false;
            }
            return true;
        }
    }
}
