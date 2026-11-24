// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    internal static class TetrisStyle
    {
        public static readonly Color Background = new(0.027f, 0.035f, 0.055f);
        public static readonly Color Surface = new(0.035f, 0.051f, 0.078f);
        public static readonly Color Frame = new(0.2f, 0.255f, 0.333f, 0.8f);
        public static readonly Color Red = new(0.937f, 0.267f, 0.267f);
        public static readonly Color Grid = new(1f, 1f, 1f, 0.025f);
        private static readonly Color[] Palette =
        [
            new(0.73f, 0.9f, 0.94f),
            new(0.85f, 0.82f, 0.94f),
            new(0.95f, 0.87f, 0.75f),
            new(0.78f, 0.84f, 0.95f),
            new(0.8f, 0.92f, 0.86f),
            new(0.95f, 0.81f, 0.81f),
            new(0.94f, 0.93f, 0.81f),
        ];

        public static Color Ink(TetrisKind kind) => Palette[(int)kind];

        public static Texture2D CreateGlow()
        {
            const int size = 64;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Tetris glow",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x + 0.5f) / size * 2f - 1f;
                var dy = (y + 0.5f) / size * 2f - 1f;
                var alpha = Mathf.Max(0f, 1f - dx * dx - dy * dy);
                pixels[y * size + x] = new Color(1f, 1f, 1f, alpha * alpha * alpha);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public static void Glow(Rect rect, Texture2D texture, Color tint)
        {
            if (texture == null)
                return;
            var previous = GUI.color;
            try
            {
                GUI.color = tint;
                GUI.DrawTexture(rect, texture);
            }
            finally
            {
                GUI.color = previous;
            }
        }
    }
}
