// parents: @.frostline/graph/m-pickle.xml

using System.Threading.Tasks;
using RimWorks.Pickle;
using RimWorks.Pickle.Input;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Pickle
{
    [PickleSteps]
    public static class FocusSteps
    {
        private static FocusProbeWindow _window;

        [BeforeScenario]
        public static void ResetProbe(PickleContext ctx)
        {
            EventSynth.SuppressDebugLogAutoOpen();

            Find.WindowStack.TryRemoveAssignableFromType(
                typeof(Dialog_MessageBox),
                doCloseSound: false
            );
            FocusProbe.Clear();
            CloseWindow();
            ScenarioMarkers.Begin("M-FOCUS-MANAGER");
        }

        [AfterScenario]
        public static void Teardown(PickleContext ctx)
        {
            CloseWindow();
            ScenarioMarkers.End("M-FOCUS-MANAGER");
        }

        private static void CloseWindow()
        {
            if (_window != null && Find.WindowStack.IsOpen(_window))
            {
                Find.WindowStack.TryRemove(_window, doCloseSound: false);
            }
            _window = null;
            FocusProbeWindow.ReleaseEngine();
        }

        private static async Task FailOnSynthError(PickleContext ctx)
        {
            await ctx.WaitFrames(3);
            if (EventSynth.TryTakeFailure(out var failure) && failure != null)
            {
                ctx.Require(false, $"input synthesis failed: {failure}");
            }
        }

        [Given("a focus probe window with a text field")]
        public static async Task GivenFocusProbeWindow(PickleContext ctx)
        {
            _window = new FocusProbeWindow();
            Find.WindowStack.Add(_window);

            for (var i = 0; i < 30 && !FocusProbe.Seen("gui:frame"); i++)
            {
                await ctx.WaitFrames(2);
            }

            ctx.Require(FocusProbe.Seen("gui:frame"), "focus probe window never rendered a frame");

            ctx.Require(
                Find.WindowStack.GetsInput(null),
                "another open window absorbs input around itself"
            );
        }

        [When("I click the text field")]
        public static async Task WhenClickTextField(PickleContext ctx)
        {
            FocusProbe.ProgrammaticFocusArmed = true;

            for (var i = 0; i < 30 && !FocusProbe.Seen("click:emulated"); i++)
            {
                await ctx.WaitFrames(2);
            }

            ctx.Require(
                FocusProbe.Seen("click:emulated"),
                "programmatic click never reached the text field"
            );
        }

        [When("I press Enter")]
        public static async Task WhenPressEnter(PickleContext ctx)
        {
            await PressKey(ctx, KeyCode.Return);
        }

        [When("I press Escape")]
        public static async Task WhenPressEscape(PickleContext ctx)
        {
            await PressKey(ctx, KeyCode.Escape);
        }

        private static async Task PressKey(PickleContext ctx, KeyCode keyCode)
        {
            FocusProbe.ProgrammaticKeyArmed = keyCode;
            var expected = $"key:injected:{keyCode}";

            for (var i = 0; i < 30 && !FocusProbe.Seen(expected); i++)
            {
                await ctx.WaitFrames(2);
            }

            ctx.Require(
                FocusProbe.Seen(expected),
                $"synthetic key {keyCode} was never injected through FVUI"
            );
            await FailOnSynthError(ctx);
        }

        [Then("the focus probe recorded fact {string}")]
        public static void ThenRecorded(PickleContext ctx, string fact)
        {
            ctx.Require(FocusProbe.Seen(fact), $"expected focus fact '{fact}' recorded");
        }

        [Then("the focus probe did not record fact {string}")]
        public static void ThenNotRecorded(PickleContext ctx, string fact)
        {
            ctx.Require(!FocusProbe.Seen(fact), $"expected focus fact '{fact}' NOT recorded");
        }

        [Then("the focus probe window is still open")]
        public static void ThenWindowOpen(PickleContext ctx)
        {
            ctx.Require(
                _window != null && Find.WindowStack.IsOpen(_window),
                "expected the focus probe window to stay open"
            );
        }
    }
}
