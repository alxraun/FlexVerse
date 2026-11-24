// parents: @.frostline/graph/m-pickle.xml

using System;
using System.Threading.Tasks;
using FlexVerse;
using RimWorks.Pickle;

namespace FlexVerse.Test.Pickle
{
    [PickleSteps]
    public static class TweenBehaviorSteps
    {
        private static TweenBehavior<float> _tween;
        private static int _completionCount;
        private static bool _subscribed;

        [BeforeScenario]
        public static void Reset(PickleContext ctx)
        {
            Unsubscribe();
            _tween = null;
            _completionCount = 0;
            ScenarioMarkers.Begin("M-TWEEN-BEHAVIOR");
        }

        [AfterScenario]
        public static void End(PickleContext ctx)
        {
            Unsubscribe();
            ScenarioMarkers.End("M-TWEEN-BEHAVIOR");
        }

        private static void Unsubscribe()
        {
            if (_subscribed && _tween != null)
            {
                FlexVerseRoot.Instance.BehaviorUpdater.Unsubscribe(_tween);
                _subscribed = false;
            }
        }

        [Given("an active tween behavior from {float} to {float} over {float} seconds")]
        public static void GivenTween(PickleContext ctx, float from, float to, float duration)
        {
            _tween = new TweenBehavior<float>
            {
                Value = from,
                Target = to,
                Duration = duration,
                Easing = FlexVerse.Easing.Linear,
            };
            _tween.OnComplete += () => _completionCount++;
            _tween.Activate();
        }

        [Given(
            "an active quad-ease-in tween behavior from {float} to {float} over {float} seconds"
        )]
        public static void GivenQuadEaseInTween(
            PickleContext ctx,
            float from,
            float to,
            float duration
        )
        {
            _tween = new TweenBehavior<float>
            {
                Value = from,
                Target = to,
                Duration = duration,
                Easing = FlexVerse.Easing.QuadEaseIn,
            };
            _tween.OnComplete += () => _completionCount++;
            _tween.Activate();
        }

        [Given(
            "an active tween behavior from {float} to {float} over {float} second subscribed to the behavior updater"
        )]
        public static void GivenSubscribedTween(
            PickleContext ctx,
            float from,
            float to,
            float duration
        )
        {
            GivenTween(ctx, from, to, duration);
            FlexVerseRoot.Instance.BehaviorUpdater.Subscribe(_tween);
            _subscribed = true;
        }

        [When("it advances for a total of {float} seconds in {float} second steps")]
        public static void AdvanceTotal(PickleContext ctx, float totalSeconds, float stepSeconds)
        {
            ctx.Require(_tween != null, "no tween behavior was created");
            var remaining = totalSeconds;
            while (remaining > 0f)
            {
                var step = Math.Min(stepSeconds, remaining);
                _tween.Update(step);
                remaining -= step;
            }
            Log.Info("M-TWEEN-BEHAVIOR", "tween.value", ("value", _tween.Value));
        }

        [When("it advances for {float} seconds in {float} second steps")]
        public static void Advance(PickleContext ctx, float totalSeconds, float stepSeconds)
        {
            AdvanceTotal(ctx, totalSeconds, stepSeconds);
        }

        [When("its target changes to {float}")]
        public static void Retarget(PickleContext ctx, float target)
        {
            ctx.Require(_tween != null, "no tween behavior was created");
            _tween.Target = target;
            Log.Info(
                "M-TWEEN-BEHAVIOR",
                "tween.retargeted",
                ("target", target),
                ("value", _tween.Value)
            );
        }

        [When("I wait for its completion within {int} frames")]
        public static async Task WaitForCompletion(PickleContext ctx, int maxFrames)
        {
            ctx.Require(_tween != null, "no tween behavior was created");
            var waited = 0;
            while (_completionCount == 0 && waited < maxFrames)
            {
                await ctx.WaitFrames(10);
                waited += 10;
            }
        }

        [Then("its value is {float} within {float}")]
        public static void ValueIs(PickleContext ctx, float expected, float tolerance)
        {
            ctx.Require(_tween != null, "no tween behavior was created");
            var diff = Math.Abs(_tween.Value - expected);

            Log.Info(
                "M-TWEEN-BEHAVIOR",
                "verify:value",
                ("expected", expected),
                ("actual", _tween.Value)
            );
            ctx.Require(
                diff <= tolerance,
                $"expected value {expected} +/- {tolerance}, got {_tween.Value}"
            );
        }

        [Then("it reports completion exactly once")]
        public static void CompletedOnce(PickleContext ctx)
        {
            Log.Info("M-TWEEN-BEHAVIOR", "verify:complete", ("count", _completionCount));
            ctx.Require(
                _completionCount == 1,
                $"expected exactly one completion, got {_completionCount}"
            );
        }

        [Then("it does not report completion yet")]
        public static void NotCompletedYet(PickleContext ctx)
        {
            Log.Info("M-TWEEN-BEHAVIOR", "verify:not-complete", ("count", _completionCount));
            ctx.Require(
                _completionCount == 0,
                $"expected no completion yet, got {_completionCount}"
            );
        }
    }
}
