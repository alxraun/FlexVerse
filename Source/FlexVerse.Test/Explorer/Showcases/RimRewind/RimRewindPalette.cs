// parents: @.frostline/graph/m-explorer.xml
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    internal static class RimRewindPalette
    {
        public static readonly Color Secondary = Hex(0xdbc3a1);
        public static readonly Color Primary = Hex(0xefbf6d);
        public static readonly Color OnPrimary = Hex(0x422c00);
        public static readonly Color PrimaryContainer = Hex(0x5f4100);
        public static readonly Color OnPrimaryContainer = Hex(0xffdeaa);
        public static readonly Color SecondaryContainer = Hex(0x54442a);
        public static readonly Color OnSecondaryContainer = Hex(0xf8dfbb);
        public static readonly Color Error = Hex(0xffb4ab);
        public static readonly Color Surface = Hex(0x101418);
        public static readonly Color OnSurface = Hex(0xe0e2e8);
        public static readonly Color OnSurfaceVariant = Hex(0xc2c7cf);
        public static readonly Color SurfaceLowest = Hex(0x0b0e12);
        public static readonly Color SurfaceLow = Hex(0x191c20);
        public static readonly Color SurfaceContainer = Hex(0x1d2024);
        public static readonly Color SurfaceHigh = Hex(0x272a2f);
        public static readonly Color SurfaceHighest = Hex(0x32353a);
        public static readonly Color Outline = Hex(0x8c9199);
        public static readonly Color OutlineVariant = Hex(0x42474e);

        private static Color Hex(uint rgb) =>
            new Color32((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb, 255);
    }
}
