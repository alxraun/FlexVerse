// parents: @.frostline/graph/m-pickle.xml

using System;
using System.Threading.Tasks;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Pickle
{
    public partial class LayoutPropsProbe : Widget
    {
        [Prop]
        public Behavior<Color> BorderColor { get; set; }
    }

    [PickleSteps]
    public static class LayoutPropsSteps
    {
        private sealed class ProbeWindow : Window
        {
            public bool Done;
            public Exception Failure;

            public override void DoWindowContents(Rect rect)
            {
                if (Done)
                    return;
                try
                {
                    Check();
                }
                catch (Exception e)
                {
                    Failure = e;
                }
                finally
                {
                    Done = true;
                }
            }

            private static void Require(bool condition, string message)
            {
                if (!condition)
                    throw new InvalidOperationException(message);
            }

            private static void Check()
            {
                var engine = new WidgetEngine();
                var definition = new LayoutPropsProbe
                {
                    Width = 80.Point(),
                    Height = 20.Point(),
                    BorderColor = Color.white,
                };
                var bounds = new Rect(0, 0, 300, 200);
                try
                {
                    engine.ProcessUI(definition, bounds);
                    var instance = (LayoutPropsProbe)engine.GetRoot();
                    Require(
                        engine.GetNode(instance).StyleGetWidth().value == 80,
                        "initial layout Prop was not applied"
                    );
                    var tween = new TweenBehavior<Length> { Target = 160.Point(), Duration = 1f };
                    instance.Width = tween;
                    tween.Update(0.5f);
                    engine.ProcessUI(definition, bounds);
                    float width = engine.GetNode(instance).StyleGetWidth().value;
                    Require(width > 80 && width < 160, "Tween OnUpdate did not reach Flexbox");
                    instance.BorderColor = Color.red;
                    engine.ProcessUI(definition, bounds);
                    Require(
                        !engine.DidLayoutCalculationHappenThisCycle,
                        "draw-only Prop triggered layout"
                    );
                    instance.Width = 42.Point();
                    engine.ProcessUI(definition, bounds);
                    Require(
                        engine.GetNode(instance).StyleGetWidth().value == 42,
                        "layout replacement was not applied"
                    );
                    Require(instance.GetProperty("Width") != null, "layout Prop missing from Refs");
                }
                finally
                {
                    engine.Shutdown();
                }
            }
        }

        [BeforeScenario]
        public static void Begin() => ScenarioMarkers.Begin("M-PICKLE-LAYOUT");

        [AfterScenario]
        public static void End() => ScenarioMarkers.End("M-PICKLE-LAYOUT");

        [Then("layout Props preserve reactive Flexbox updates in IMGUI")]
        public static async Task CheckLayout(PickleContext ctx)
        {
            var window = new ProbeWindow();
            Find.WindowStack.Add(window);
            try
            {
                for (int frame = 0; frame < 60 && !window.Done; frame++)
                    await ctx.WaitFrames(1);
                ctx.Require(window.Done, "layout probe did not render");
                ctx.Require(
                    window.Failure == null,
                    window.Failure?.ToString() ?? "layout probe failed"
                );
            }
            finally
            {
                Find.WindowStack.TryRemove(window, doCloseSound: false);
            }
        }
    }
}
