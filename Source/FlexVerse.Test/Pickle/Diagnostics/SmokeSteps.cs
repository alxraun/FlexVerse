// parents: @.frostline/graph/m-pickle.xml

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using RimWorks.Pickle;

namespace FlexVerse.Test.Pickle
{
    [PickleSteps]
    public static class FlexVerseSmokeSteps
    {
        private static readonly List<string> Markers = new();
        private static SmokeWidget _widget;
        private static StaticBehavior<float> _behavior;
        private static bool _behaviorCompleted;

        [BeforeScenario]
        public static void ScenarioBegin(PickleContext ctx)
        {
            ScenarioMarkers.Begin("M-PICKLE-SMOKE");
        }

        [AfterScenario]
        public static void ScenarioEnd(PickleContext ctx)
        {
            ScenarioMarkers.End("M-PICKLE-SMOKE");
        }

        [When("I wait {int} frames")]
        public static async Task WaitFrames(PickleContext ctx, int frames)
        {
            await ctx.WaitFrames(frames);
        }

        [When("I emit a flexverse verify marker {string}")]
        public static void EmitVerifyMarker(PickleContext ctx, string marker)
        {
            Log.Info("M-PICKLE-SMOKE", marker);
            Markers.Add(marker);
        }

        [Then("the flexverse verify marker {string} was recorded")]
        public static void VerifyMarkerRecorded(PickleContext ctx, string marker)
        {
            ctx.Require(
                Markers.Contains(marker),
                $"expected FlexVerse verify marker '{marker}' to have been emitted"
            );
        }

        [Given("a smoke widget with prop {string} set to {float}")]
        public static void CreateSmokeWidget(PickleContext ctx, string prop, float value)
        {
            ctx.Require(prop == "Speed", $"this smoke suite only wires prop 'Speed', got '{prop}'");
            _widget = new SmokeWidget { Speed = value };
        }

        [Then("the smoke widget prop {string} is {float}")]
        public static void SmokeWidgetPropIs(PickleContext ctx, string prop, float value)
        {
            ctx.Require(_widget != null, "no smoke widget was created");
            ctx.Require(
                _widget.Speed != null && System.Math.Abs(_widget.Speed.Target - value) < 0.0001f,
                $"expected Speed target={value}, got {(_widget.Speed != null ? _widget.Speed.Target.ToString() : "<null>")}"
            );
        }

        [Given("an active static behavior with duration {float}")]
        public static void CreateStaticBehavior(PickleContext ctx, float duration)
        {
            _behaviorCompleted = false;
            _behavior = new StaticBehavior<float> { Duration = duration, Target = 1f };
            _behavior.OnComplete += () => _behaviorCompleted = true;
            _behavior.Activate();
        }

        [When("it updates for a total of {float} seconds")]
        public static void UpdateBehavior(PickleContext ctx, float seconds)
        {
            _behavior.Update(seconds);
        }

        [Then("it reports completion")]
        public static void BehaviorCompleted(PickleContext ctx)
        {
            ctx.Require(
                _behaviorCompleted,
                "expected StaticBehavior to raise OnComplete after its duration elapsed"
            );
        }
    }
}
