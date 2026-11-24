// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Globalization;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Equal world units on both axes. Min/Max define the minimum visible extent;
    /// the wider axis expands around its centre to fill the available viewport.
    /// Tick strings are rebuilt only on viewport changes, outside OnGUI.
    /// </summary>
    internal sealed class SequencePlot
    {
        internal readonly struct Tick(float value, string label)
        {
            internal float Value { get; } = value;
            internal string Label { get; } = label;
        }

        internal Rect Bounds { get; private set; }
        internal Vector2 Min { get; private set; }
        internal Vector2 Max { get; private set; }
        internal Tick[] X { get; private set; } = [];
        internal Tick[] Y { get; private set; } = [];
        internal bool Ready => Bounds.width > 0f && Bounds.height > 0f;
        private Rect _canvas;
        private Vector2 _requestedMin;
        private Vector2 _requestedMax;
        private float _scale;

        internal void Update(Rect canvas, Vector2 min, Vector2 max)
        {
            if (_canvas == canvas && _requestedMin == min && _requestedMax == max)
                return;
            _canvas = canvas;
            _requestedMin = min;
            _requestedMax = max;
            Bounds = new Rect(
                canvas.x + 40f,
                canvas.y + 42f,
                Mathf.Max(1f, canvas.width - 58f),
                Mathf.Max(1f, canvas.height - 104f)
            );
            var size = new Vector2(
                Mathf.Max(0.01f, max.x - min.x),
                Mathf.Max(0.01f, max.y - min.y)
            );
            size *= 1.12f;
            _scale = Mathf.Min(Bounds.width / size.x, Bounds.height / size.y);
            var visible = new Vector2(Bounds.width / _scale, Bounds.height / _scale);
            Min = (min + max - visible) * 0.5f;
            Max = Min + visible;
            var step = Mathf.Pow(2f, Mathf.Ceil(Mathf.Log(48f / _scale, 2f)));
            X = BuildTicks(Min.x, Max.x, step);
            Y = BuildTicks(Min.y, Max.y, step);
        }

        private static Tick[] BuildTicks(float min, float max, float step)
        {
            var first = Mathf.Ceil(min / step) * step;
            var count = Math.Max(0, Mathf.FloorToInt((max - first) / step) + 1);
            var ticks = new Tick[count];
            for (int i = 0; i < count; i++)
            {
                var value = first + i * step;
                ticks[i] = new Tick(value, value.ToString("0.##", CultureInfo.InvariantCulture));
            }
            return ticks;
        }

        internal Vector2 ToLocal(Vector2 world) =>
            new((world.x - Min.x) * _scale, Bounds.height - (world.y - Min.y) * _scale);

        internal Vector2 FromScreen(Vector2 screen) =>
            new(
                Mathf.Round((Min.x + (screen.x - Bounds.x) / _scale) * 2f) * 0.5f,
                Mathf.Round((Max.y - (screen.y - Bounds.y) / _scale) * 2f) * 0.5f
            );
    }
}
