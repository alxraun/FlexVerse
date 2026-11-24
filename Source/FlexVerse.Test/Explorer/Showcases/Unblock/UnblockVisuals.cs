// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    internal static class UnblockVisuals
    {
        public static readonly Color Background = Rgb(0x07090e);
        public static readonly Color Board = Rgb(0x090d14);
        public static readonly Color Frame = new(0.2f, 0.255f, 0.333f, 0.8f);
        public static readonly Color Red = Rgb(0xef4444);
        public static readonly Color RedBorder = Rgb(0xfca5a5);
        public static readonly Color Cyan = new(0.22f, 0.74f, 0.97f, 0.8f);
        public static readonly Color Grid = new(1f, 1f, 1f, 0.012f);
        public static readonly Color Grip = new(0.392f, 0.455f, 0.545f, 0.35f);
        private static readonly Color[] Palette =
        [
            Rgb(0xffffff),
            Rgb(0xf8fafc),
            Rgb(0xf1f5f9),
            Rgb(0xe2e8f0),
            Rgb(0xcbd5e1),
            Rgb(0xf3f4f6),
        ];

        public static Color Shade(string id, int index)
        {
            var hash = index;
            foreach (var character in id)
                hash += character;
            return Palette[hash % Palette.Length];
        }

        private static Color Rgb(int rgb) =>
            new(((rgb >> 16) & 255) / 255f, ((rgb >> 8) & 255) / 255f, (rgb & 255) / 255f);

        public static Texture2D CreateGlow()
        {
            const int size = 128;
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = "Unblock glow",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear,
            };
            var pixels = new Color[size * size];
            for (var y = 0; y < size; y++)
            for (var x = 0; x < size; x++)
            {
                var dx = (x + 0.5f) / size * 2f - 1f;
                var dy = (y + 0.5f) / size * 2f - 1f;
                var falloff = Mathf.Max(0f, 1f - dx * dx - dy * dy);
                pixels[y * size + x] = new Color(1f, 1f, 1f, falloff * falloff * falloff);
            }
            texture.SetPixels(pixels);
            texture.Apply(false, true);
            return texture;
        }

        public static void DrawGlow(Rect rect, Texture2D texture, Color color)
        {
            var previous = GUI.color;
            try
            {
                GUI.color = color;
                GUI.DrawTexture(rect, texture);
            }
            finally
            {
                GUI.color = previous;
            }
        }
    }
}
