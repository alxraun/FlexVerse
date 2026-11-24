// parents: @.frostline/graph/m-xunit.xml

using System;
using UnityEngine;

namespace Verse
{
    public struct FloatRange
    {
        public float min;
        public float max;

        public FloatRange(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
    }

    public enum GameFont : byte
    {
        Tiny,
        Small,
        Medium,
    }

    public static class Text
    {
        public static GUIStyle CurTextFieldStyle { get; } = new GUIStyle();
        public static GameFont Font { get; set; } = GameFont.Small;
        public static TextAnchor Anchor { get; set; }
        public static bool WordWrap { get; set; }

        public static Vector2 CalcSize(string text) => new Vector2((text?.Length ?? 0) * 7f, 20f);
    }

    public struct TipSignal
    {
        public static implicit operator TipSignal(string text) => default;
    }

    public static class TooltipHandler
    {
        public static void TipRegion(Rect rect, TipSignal tip) { }
    }

    public static class Widgets
    {
        public static bool ButtonInvisible(Rect butRect, bool doMouseoverSound = true) => false;

        public static int ButtonTextCalls;
        public static Matrix4x4 LastButtonMatrix;
        public static Rect LastButtonRect;

        public static bool ButtonText(
            Rect rect,
            string label,
            bool drawBackground = true,
            bool doMouseoverSound = true,
            bool active = true,
            TextAnchor? overrideTextAnchor = null
        )
        {
            ButtonTextCalls++;
            LastButtonMatrix = GUI.matrix;
            LastButtonRect = rect;
            return false;
        }

        public static void DrawBoxSolid(Rect rect, Color color) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public class StaticConstructorOnStartupAttribute : Attribute { }

    public static class Log
    {
        public static void Error(string msg) { }

        public static void ErrorOnce(string msg, int id) { }

        public static void Warning(string msg) { }

        public static void WarningOnce(string msg, int id) { }

        public static void Message(string msg) { }
    }
}
