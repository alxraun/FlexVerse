// parents: @.frostline/graph/m-explorer.xml

using System;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public sealed class TrafficSkin : IDisposable
    {
        public static readonly Color Background = new(0.055f, 0.059f, 0.071f);
        public static readonly Color Caption = new(0.655f, 0.671f, 0.698f);
        public static readonly Color HeroCaption = new(0.898f, 0.902f, 0.914f);
        public static readonly Color Value = new(0.945f, 0.949f, 0.953f);
        public GUIStyle Normal { get; private set; }
        public GUIStyle Hero { get; private set; }
        public GUIStyle Transit { get; private set; }
        private Texture2D _transit;
        private Texture2D _normal;
        private Texture2D _hero;

        public static TrafficSkin Create()
        {
            var skin = new TrafficSkin();
            skin._normal = CreateTexture(false);
            skin._transit = CreateTexture(false, true);
            skin.Transit = CreateStyle(skin._transit);
            skin._hero = CreateTexture(true);
            skin.Normal = CreateStyle(skin._normal);
            skin.Hero = CreateStyle(skin._hero);
            return skin;
        }

        private static GUIStyle CreateStyle(Texture2D texture) =>
            new() { normal = { background = texture }, border = new RectOffset(14, 14, 14, 14) };

        private static Texture2D CreateTexture(bool hero, bool transit = false)
        {
            const int size = 64;
            const float radius = 14f;
            var pixels = new Color[size * size];
            var border = transit
                ? new Color(0.43f, 0.46f, 0.51f)
                : new Color(0.18f, 0.188f, 0.208f);
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = Mathf.Abs(x + 0.5f - size * 0.5f) - (size * 0.5f - radius);
                var dy = Mathf.Abs(y + 0.5f - size * 0.5f) - (size * 0.5f - radius);
                var outsideX = Mathf.Max(dx, 0f);
                var outsideY = Mathf.Max(dy, 0f);
                var distance =
                    Mathf.Sqrt(outsideX * outsideX + outsideY * outsideY)
                    + Mathf.Min(Mathf.Max(dx, dy), 0f)
                    - radius;
                var fill =
                    hero
                        ? Color.Lerp(
                            new Color(0.224f, 0.235f, 0.255f),
                            new Color(0.137f, 0.145f, 0.161f),
                            (x + size - 1f - y) / (2f * (size - 1f))
                        )
                    : transit ? new Color(0.16f, 0.17f, 0.19f)
                    : new Color(0.102f, 0.11f, 0.122f);
                var color = Color.Lerp(border, fill, Mathf.Clamp01(-0.5f - distance));
                color.a = Mathf.Clamp01(0.5f - distance);
                pixels[y * size + x] = color;
            }
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name =
                    hero ? "Traffic hero"
                    : transit ? "Traffic transit"
                    : "Traffic tile",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public void Dispose()
        {
            Transit.normal.background = null;
            UnityEngine.Object.Destroy(_transit);
            Normal.normal.background = null;
            Hero.normal.background = null;
            UnityEngine.Object.Destroy(_normal);
            UnityEngine.Object.Destroy(_hero);
        }
    }
}
