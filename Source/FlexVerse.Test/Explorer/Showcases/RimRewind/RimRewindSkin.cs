// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    [StaticConstructorOnStartup]
    internal static class RimRewindSkin
    {
        public const float CornerRadius = 2f;

        /// <summary>Shared spring presets: hover snaps quickly so sweeps leave no lingering trail; press dips fast with a springy return.</summary>
        public const float HoverTension = 400f;
        public const float HoverFriction = 28f;
        public const float PressTension = 400f;
        public const float PressFriction = 24f;
        public const float PressOffset = 1.5f;

        public static readonly Color Background = RimRewindPalette.Surface;
        public static readonly Color Sidebar = RimRewindPalette.SurfaceLow;
        public static readonly Color Surface = RimRewindPalette.SurfaceContainer;
        public static readonly Color Raised = RimRewindPalette.SurfaceHigh;
        public static readonly Color Line = RimRewindPalette.OutlineVariant;
        public static readonly Color Text = RimRewindPalette.OnSurface;
        public static readonly Color Muted = RimRewindPalette.OnSurfaceVariant;
        public static readonly Color Accent = RimRewindPalette.Primary;
        public static readonly Color Amber = RimRewindPalette.Primary;
        public static readonly Color Rewind = new(.58f, .75f, .81f);
        public static readonly Color GoldDark = RimRewindPalette.SecondaryContainer;
        private static Texture2D _disc;

        public static void LoadMaterials() =>
            _disc = GradientTextures.Radial(
                64,
                64,
                new Vector2(.5f, .5f),
                .5f,
                new GradientStop(0f, Color.white),
                new GradientStop(.94f, Color.white),
                new GradientStop(1f, Color.clear)
            );

        public static Label BuildCaption(Behavior<string> text, Color? color = null) =>
            new()
            {
                Content = text,
                Color = color ?? Muted,
                Font = GameFont.Tiny,
                TextAlign = TextAnchor.MiddleLeft,
                TextWrap = false,
                TextOverflow = TextOverflowMode.Clip,
                Height = 24f.Point(),
            };

        public static Label BuildHeading(Behavior<string> text) =>
            new()
            {
                Content = text,
                Color = Text,
                Font = GameFont.Medium,
                TextAlign = TextAnchor.MiddleLeft,
                TextWrap = false,
                TextOverflow = TextOverflowMode.Clip,
                Height = 34f.Point(),
            };

        public static Color Alpha(Color color, float alpha) =>
            new(color.r, color.g, color.b, alpha);

        /// <summary>Native primitives reuse a circle mask prepared at mount; no drawing-time resource creation.</summary>
        public static void DrawRounded(Rect rect, Color color, float radius)
        {
            if (rect.width <= 0f || rect.height <= 0f)
                return;
            radius = Mathf.Min(radius, Mathf.Min(rect.width, rect.height) * .5f);
            var original = GUI.color;
            try
            {
                Widgets.DrawBoxSolid(
                    new Rect(rect.x + radius, rect.y, rect.width - radius * 2f, rect.height),
                    color * original
                );
                Widgets.DrawBoxSolid(
                    new Rect(rect.x, rect.y + radius, rect.width, rect.height - radius * 2f),
                    color * original
                );
                GUI.color = original * color;
                GUI.DrawTextureWithTexCoords(
                    new Rect(rect.x, rect.y, radius, radius),
                    _disc,
                    new Rect(0f, .5f, .5f, .5f)
                );
                GUI.DrawTextureWithTexCoords(
                    new Rect(rect.xMax - radius, rect.y, radius, radius),
                    _disc,
                    new Rect(.5f, .5f, .5f, .5f)
                );
                GUI.DrawTextureWithTexCoords(
                    new Rect(rect.x, rect.yMax - radius, radius, radius),
                    _disc,
                    new Rect(0f, 0f, .5f, .5f)
                );
                GUI.DrawTextureWithTexCoords(
                    new Rect(rect.xMax - radius, rect.yMax - radius, radius, radius),
                    _disc,
                    new Rect(.5f, 0f, .5f, .5f)
                );
            }
            finally
            {
                GUI.color = original;
            }
        }
    }
}
