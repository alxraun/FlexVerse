// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;

namespace FlexVerse
{
    internal static class ShadowDrawer
    {
        internal static void Draw(Rect bounds, CornerRadii radii, Shadow shadow)
        {
            if (bounds.width <= 0f || bounds.height <= 0f || shadow.Color.a <= 0f)
                return;

            float spread = shadow.SpreadRadius;
            float blur = Mathf.Max(0f, shadow.BlurRadius);
            float pad = blur * 3f + 1f;
            var shape = new Rect(
                bounds.x + shadow.Offset.x - spread,
                bounds.y + shadow.Offset.y - spread,
                bounds.width + spread * 2f,
                bounds.height + spread * 2f
            );

            if (shape.width <= 0f || shape.height <= 0f)
                return;
            var shadowRadii = CornerRadii.Resolve(
                shape,
                0f,
                radii.Outset(spread).TL,
                radii.Outset(spread).TR,
                radii.Outset(spread).BR,
                radii.Outset(spread).BL
            );
            var draw = new Rect(
                shape.x - pad,
                shape.y - pad,
                shape.width + 2f * pad,
                shape.height + 2f * pad
            );
            var relativeShape = new Rect(pad, pad, shape.width, shape.height);
            var originalSurface = new Rect(
                bounds.x - draw.x,
                bounds.y - draw.y,
                bounds.width,
                bounds.height
            );
            var mask = AppearanceMasks.GetShadow(
                new Rect(0f, 0f, draw.width, draw.height),
                relativeShape,
                shadowRadii,
                originalSurface,
                radii,
                blur
            );
            ImageDrawer.DrawTint(draw, mask, shadow.Color);
        }
    }
}
