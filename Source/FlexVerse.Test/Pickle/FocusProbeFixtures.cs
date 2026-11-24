// parents: @.frostline/graph/m-pickle.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Pickle
{
    public static class FocusProbe
    {
        private static readonly List<string> Facts = new();

        public static bool ProgrammaticFocusArmed;

        public static KeyCode? ProgrammaticKeyArmed;

        public static void Clear()
        {
            Facts.Clear();
            ProgrammaticFocusArmed = false;
            ProgrammaticKeyArmed = null;
        }

        public static void Record(string fact)
        {
            Facts.Add(fact);
            Log.Debug("M-PICKLE-FOCUS", "fact.recorded", ("fact", fact));
        }

        public static bool Seen(string fact) => Facts.Contains(fact);
    }

    public partial class FocusProbeScreen : Widget
    {
        public FocusProbeScreen()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            JustifyContent = Justify.FlexStart;
            PaddingAll = 10f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new TextField
            {
                Placeholder = "focus probe",
                FlexShrink = 0f,
                OnSubmit = (System.Action)(() => FocusProbe.Record("key:submit")),
                OnCancel = (System.Action)(() => FocusProbe.Record("key:cancel")),
                OnValueChange = (System.Action<string>)(v => FocusProbe.Record("value:" + v)),
            };
        }
    }

    public class FocusProbeWindow : Window
    {
        private const string EngineKey = "focus-pickle";

        private FocusProbeScreen _screen;

        public FocusProbeWindow()
        {
            doCloseX = true;

            closeOnAccept = false;
            closeOnCancel = false;
            draggable = true;
            resizeable = false;
            absorbInputAroundWindow = false;
            optionalTitle = "Focus Probe";
        }

        public override Vector2 InitialSize => new(400f, 160f);

        public static void ReleaseEngine()
        {
            FVUI.Release(EngineKey);
        }

        public override void DoWindowContents(Rect inRect)
        {
            _screen ??= new FocusProbeScreen();
            FVUI.Render(EngineKey, inRect, _screen);

            if (FocusProbe.ProgrammaticFocusArmed)
            {
                FocusProbe.ProgrammaticFocusArmed = false;
                EmulateClickOnTextField();
            }

            if (FocusProbe.ProgrammaticKeyArmed is { } keyCode)
            {
                FocusProbe.ProgrammaticKeyArmed = null;
                EmulateKeyInFvui(inRect, keyCode);
            }

            FocusProbe.Record("gui:frame");
        }

        private void EmulateClickOnTextField()
        {
            var engine = FVUI.GetEngine(EngineKey);
            var root = engine?.GetRoot();
            if (root == null)
            {
                return;
            }

            foreach (var child in engine.GetChildren(root))
            {
                if (child is TextField)
                {
                    var click = new FlexEvent(new Event { type = EventType.MouseDown }, child);
                    child.OnMouseDown(click);
                    FocusProbe.Record("click:emulated");
                    return;
                }
            }
        }

        private void EmulateKeyInFvui(Rect inRect, KeyCode keyCode)
        {
            var keyEvent = new Event { type = EventType.KeyDown, keyCode = keyCode };
            var original = Event.current;
            Event.current = keyEvent;
            try
            {
                FVUI.Render(EngineKey, inRect, _screen);
            }
            finally
            {
                Event.current = original;
            }

            FocusProbe.Record($"key:injected:{keyCode}");
        }
    }
}
