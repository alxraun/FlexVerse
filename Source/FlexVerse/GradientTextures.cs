// parents: @.frostline/graph/m-gradient-textures.xml

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Defines one color stop of a gradient texture. (M-GRADIENT-TEXTURES)</summary>
    public readonly struct GradientStop
    {
        /// <summary>Holds the normalized position of the stop.</summary>
        public readonly float Position;

        /// <summary>Holds the color blended at the stop.</summary>
        public readonly Color Color;

        /// <summary>Creates a gradient stop from a position and color.</summary>
        public GradientStop(float position, Color color)
        {
            Position = position;
            Color = color;
        }
    }

    /// <summary>Generates and caches linear, radial and conic gradient textures. (M-GRADIENT-TEXTURES, REQ-004)</summary>
    public static class GradientTextures
    {
        private const int MaxDimension = 4096;
        private const int MaxPixels = 4194304;

        private static readonly Dictionary<string, Texture2D> Cache = new();

        private enum GradientKind
        {
            Linear,
            Radial,
            Conic,
        }

        /// <summary>Creates a cached square linear gradient between two colors.</summary>
        public static Texture2D Linear(Color start, Color end, int size = 256) =>
            Linear(size, size, 0f, new GradientStop(0f, start), new GradientStop(1f, end));

        /// <summary>Creates a cached square radial gradient between two colors.</summary>
        public static Texture2D Radial(Color start, Color end, int size = 256) =>
            Radial(
                size,
                size,
                new Vector2(0.5f, 0.5f),
                0.5f,
                new GradientStop(0f, start),
                new GradientStop(1f, end)
            );

        /// <summary>Creates a cached square conic gradient between two colors.</summary>
        public static Texture2D Conic(Color start, Color end, int size = 256) =>
            Conic(
                size,
                size,
                new Vector2(0.5f, 0.5f),
                0f,
                new GradientStop(0f, start),
                new GradientStop(1f, end)
            );

        /// <summary>Creates a cached linear gradient at a given angle. (REQ-003)</summary>
        public static Texture2D Linear(
            int width,
            int height,
            float angleDegrees,
            params GradientStop[] stops
        ) => Create(GradientKind.Linear, width, height, angleDegrees, Vector2.zero, 0f, stops);

        /// <summary>Creates a cached radial gradient around a center. (REQ-003)</summary>
        public static Texture2D Radial(
            int width,
            int height,
            Vector2 center,
            float radius,
            params GradientStop[] stops
        ) => Create(GradientKind.Radial, width, height, 0f, center, radius, stops);

        /// <summary>Creates a cached conic gradient from a start angle. (REQ-003)</summary>
        public static Texture2D Conic(
            int width,
            int height,
            Vector2 center,
            float startAngleDegrees,
            params GradientStop[] stops
        ) => Create(GradientKind.Conic, width, height, startAngleDegrees, center, 0f, stops);

        private static Texture2D Create(
            GradientKind kind,
            int width,
            int height,
            float angle,
            Vector2 center,
            float radius,
            GradientStop[] input
        )
        {
            if (
                width <= 0
                || width > MaxDimension
                || height <= 0
                || height > MaxDimension
                || (long)width * height > MaxPixels
            )
                throw new ArgumentOutOfRangeException(
                    nameof(width),
                    "Gradient texture dimensions must be positive and within 4M pixels."
                );
            if (input == null || input.Length == 0)
                throw new ArgumentException(
                    "At least one gradient stop is required.",
                    nameof(input)
                );

            var stops = (GradientStop[])input.Clone();
            for (int i = 0; i < stops.Length; i++)
            {
                if (float.IsNaN(stops[i].Position) || float.IsInfinity(stops[i].Position))
                    throw new ArgumentException(
                        "Gradient stop position must be finite.",
                        nameof(input)
                    );
                stops[i] = new GradientStop(Mathf.Clamp01(stops[i].Position), stops[i].Color);
            }

            for (int i = 1; i < stops.Length; i++)
            {
                var current = stops[i];
                int j = i - 1;
                while (j >= 0 && stops[j].Position > current.Position)
                {
                    stops[j + 1] = stops[j];
                    j--;
                }
                stops[j + 1] = current;
            }

            var key = MakeKey(kind, width, height, angle, center, radius, stops);
            if (Cache.TryGetValue(key, out var existing) && existing != null)
                return existing;

            float angleRadians = angle * ((float)Math.PI / 180f);
            float dirX = Mathf.Cos(angleRadians);
            float dirY = Mathf.Sin(angleRadians);
            float extent = Mathf.Abs(dirX) + Mathf.Abs(dirY);
            float smaller = Mathf.Min(width, height);
            var pixels = new Color[width * height];

            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                float u = (x + 0.5f) / width;

                float v = 1f - (y + 0.5f) / height;
                float t;
                switch (kind)
                {
                    case GradientKind.Linear:
                        t =
                            0.5f
                            + ((u - 0.5f) * dirX + (v - 0.5f) * dirY)
                                / Mathf.Max(extent, 0.000001f);
                        break;

                    case GradientKind.Radial:
                        float dx = (u - center.x) * width / smaller;
                        float dy = (v - center.y) * height / smaller;
                        t = radius > 0f ? Mathf.Sqrt(dx * dx + dy * dy) / radius : 1f;
                        break;

                    case GradientKind.Conic:
                        float theta = (float)Math.Atan2(v - center.y, u - center.x) - angleRadians;
                        t = theta / (2f * (float)Math.PI);
                        t -= Mathf.Floor(t);
                        break;

                    default:
                        t = 0f;
                        break;
                }

                pixels[y * width + x] = Evaluate(stops, Mathf.Clamp01(t));
            }

            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = "FlexVerse " + kind + " gradient",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);

            Cache.Add(key, texture);
            return texture;
        }

        private static Color Evaluate(GradientStop[] stops, float t)
        {
            if (stops.Length == 1 || t <= stops[0].Position)
                return stops[0].Color;

            for (int i = 1; i < stops.Length; i++)
            {
                var end = stops[i];
                if (t <= end.Position)
                {
                    var start = stops[i - 1];
                    float span = end.Position - start.Position;
                    return span <= 0f
                        ? end.Color
                        : Color.Lerp(start.Color, end.Color, (t - start.Position) / span);
                }
            }

            return stops[stops.Length - 1].Color;
        }

        private static string MakeKey(
            GradientKind kind,
            int width,
            int height,
            float angle,
            Vector2 center,
            float radius,
            GradientStop[] stops
        )
        {
            var b = new StringBuilder();
            b.Append((int)kind).Append('|').Append(width).Append('x').Append(height);
            Append(b, angle);
            Append(b, center.x);
            Append(b, center.y);
            Append(b, radius);
            foreach (var stop in stops)
            {
                Append(b, stop.Position);
                Append(b, stop.Color.r);
                Append(b, stop.Color.g);
                Append(b, stop.Color.b);
                Append(b, stop.Color.a);
            }
            return b.ToString();
        }

        private static void Append(StringBuilder b, float v) =>
            b.Append('|').Append(v.ToString("R", CultureInfo.InvariantCulture));

        /// <summary>Destroys all cached gradient textures at framework teardown. (REQ-004)</summary>
        public static void ClearCache()
        {
            foreach (var pair in Cache)
            {
                if (pair.Value != null)
                    UnityEngine.Object.Destroy(pair.Value);
            }
            Cache.Clear();
        }
    }
}
