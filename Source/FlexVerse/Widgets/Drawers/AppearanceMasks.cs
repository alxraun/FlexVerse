// parents: @.frostline/graph/m-appearance-drawers.xml

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace FlexVerse
{
    internal static class AppearanceMasks
    {
        private const int MaxEntries = 64;
        private const int MaxResolution = 1024;
        private static readonly Dictionary<string, Texture2D> Cache = new();
        private static readonly Queue<string> Order = new();

        internal static string F(float value) => value.ToString("R", CultureInfo.InvariantCulture);

        internal static Texture2D GetFill(Rect bounds, CornerRadii radii) =>
            Get(
                "fill|" + Key(bounds, radii),
                bounds,
                (x, y) => Coverage(Distance(x, y, bounds.width, bounds.height, radii))
            );

        internal static Texture2D GetBorder(Rect bounds, CornerRadii radii, BorderEdges edges)
        {
            var innerW = Mathf.Max(0f, bounds.width - edges.L - edges.R);
            var innerH = Mathf.Max(0f, bounds.height - edges.T - edges.B);
            var innerRect = new Rect(0f, 0f, innerW, innerH);
            var innerRadii = CornerRadii.Resolve(
                innerRect,
                0f,
                radii.Inset(edges.L, edges.T, edges.R, edges.B).TL,
                radii.Inset(edges.L, edges.T, edges.R, edges.B).TR,
                radii.Inset(edges.L, edges.T, edges.R, edges.B).BR,
                radii.Inset(edges.L, edges.T, edges.R, edges.B).BL
            );

            return Get(
                "border|" + Key(bounds, radii) + "|" + edges.CacheKey,
                bounds,
                (x, y) =>
                {
                    var outer = Coverage(Distance(x, y, bounds.width, bounds.height, radii));
                    if (innerW <= 0f || innerH <= 0f)
                        return outer;
                    var inner = Coverage(
                        Distance(x - edges.L, y - edges.T, innerW, innerH, innerRadii)
                    );
                    return Mathf.Clamp01(outer - inner);
                }
            );
        }

        internal static Texture2D GetShadow(
            Rect textureRect,
            Rect shadowShape,
            CornerRadii shadowRadii,
            Rect originalSurface,
            CornerRadii originalRadii,
            float blur
        )
        {
            string key =
                "shadow|"
                + Key(textureRect, shadowRadii)
                + "|"
                + F(shadowShape.x)
                + ","
                + F(shadowShape.y)
                + ","
                + F(shadowShape.width)
                + ","
                + F(shadowShape.height)
                + "|original="
                + F(originalSurface.x)
                + ","
                + F(originalSurface.y)
                + ","
                + Key(originalSurface, originalRadii)
                + "|blur="
                + F(blur);

            return Get(
                key,
                textureRect,
                (x, y) =>
                {
                    float d = Distance(
                        x - shadowShape.x,
                        y - shadowShape.y,
                        shadowShape.width,
                        shadowShape.height,
                        shadowRadii
                    );
                    float shadowAlpha;
                    if (blur <= 0f)
                    {
                        shadowAlpha = Coverage(d);
                    }
                    else if (d <= 0f)
                    {
                        shadowAlpha = 1f;
                    }
                    else
                    {
                        float sigma = Mathf.Max(0.01f, blur * 0.5f);
                        shadowAlpha = Mathf.Clamp01(
                            (float)Math.Exp(-0.5f * d * d / (sigma * sigma))
                        );
                    }

                    var ownCoverage = Coverage(
                        Distance(
                            x - originalSurface.x,
                            y - originalSurface.y,
                            originalSurface.width,
                            originalSurface.height,
                            originalRadii
                        )
                    );
                    return shadowAlpha * (1f - ownCoverage);
                }
            );
        }

        internal static void DisposeCaches()
        {
            foreach (var pair in Cache)
            {
                if (pair.Value != null)
                    UnityEngine.Object.Destroy(pair.Value);
            }
            Cache.Clear();
            Order.Clear();
        }

        private static string Key(Rect bounds, CornerRadii radii) =>
            F(bounds.width) + "x" + F(bounds.height) + "|" + radii.CacheKey;

        private static float Coverage(float distance) => Mathf.Clamp01(0.5f - distance);

        private static float Distance(float x, float y, float w, float h, CornerRadii r)
        {
            if (r.TL > 0f && x < r.TL && y < r.TL)
            {
                float dx = x - r.TL,
                    dy = y - r.TL;
                return Mathf.Sqrt(dx * dx + dy * dy) - r.TL;
            }
            if (r.TR > 0f && x > w - r.TR && y < r.TR)
            {
                float dx = x - (w - r.TR),
                    dy = y - r.TR;
                return Mathf.Sqrt(dx * dx + dy * dy) - r.TR;
            }
            if (r.BR > 0f && x > w - r.BR && y > h - r.BR)
            {
                float dx = x - (w - r.BR),
                    dy = y - (h - r.BR);
                return Mathf.Sqrt(dx * dx + dy * dy) - r.BR;
            }
            if (r.BL > 0f && x < r.BL && y > h - r.BL)
            {
                float dx = x - r.BL,
                    dy = y - (h - r.BL);
                return Mathf.Sqrt(dx * dx + dy * dy) - r.BL;
            }

            if (x < 0 || x > w || y < 0 || y > h)
            {
                float dx = Mathf.Max(Mathf.Max(-x, x - w), 0f);
                float dy = Mathf.Max(Mathf.Max(-y, y - h), 0f);
                return Mathf.Sqrt(dx * dx + dy * dy);
            }

            return -Mathf.Min(Mathf.Min(x, w - x), Mathf.Min(y, h - y));
        }

        private static Texture2D Get(string key, Rect bounds, Func<float, float, float> coverage)
        {
            if (Cache.TryGetValue(key, out var existing) && existing != null)
                return existing;

            int w = Math.Max(1, Math.Min(MaxResolution, Mathf.CeilToInt(bounds.width)));
            int h = Math.Max(1, Math.Min(MaxResolution, Mathf.CeilToInt(bounds.height)));
            var pixels = new Color[w * h];
            float stepX = bounds.width / w,
                stepY = bounds.height / h;
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float opacity = coverage((x + 0.5f) * stepX, bounds.height - (y + 0.5f) * stepY);
                pixels[y * w + x] = new Color(1f, 1f, 1f, opacity);
            }

            var texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                name = "FlexVerse native surface " + key,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);

            if (Cache.Count >= MaxEntries)
            {
                var oldKey = Order.Dequeue();
                if (Cache.TryGetValue(oldKey, out var old) && old != null)
                    UnityEngine.Object.Destroy(old);
                Cache.Remove(oldKey);
            }

            Cache.Add(key, texture);
            Order.Enqueue(key);
            return texture;
        }
    }
}
