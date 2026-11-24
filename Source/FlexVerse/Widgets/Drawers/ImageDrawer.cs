// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;

namespace FlexVerse
{
    internal static class ImageDrawer
    {
        internal static void Draw(
            Rect bounds,
            Texture texture,
            BackgroundSize sizing,
            Vector2 position,
            Color tint,
            CornerRadii radii
        )
        {
            if (
                texture == null
                || bounds.width <= 0f
                || bounds.height <= 0f
                || texture.width <= 0
                || texture.height <= 0
                || tint.a <= 0f
            )
                return;

            var placement = ResolveImagePlacement(
                bounds,
                texture.width,
                texture.height,
                sizing,
                position
            );
            DrawClippedTexture(bounds, placement.Destination, texture, placement.Uv, tint, radii);
        }

        internal static void DrawContent(
            Rect bounds,
            Texture texture,
            ImageResizeMode resizeMode,
            Color tint,
            CornerRadii radii
        )
        {
            BackgroundSize size = resizeMode switch
            {
                ImageResizeMode.ScaleAndCrop => BackgroundSize.Cover,
                ImageResizeMode.StretchToFill => BackgroundSize.Stretch,
                _ => BackgroundSize.Contain,
            };
            Draw(bounds, texture, size, new Vector2(0.5f, 0.5f), tint, radii);
        }

        internal static void DrawTint(Rect rect, Texture texture, Color color)
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

        internal static (Rect Destination, Rect Uv) ResolveImagePlacement(
            Rect bounds,
            float imageWidth,
            float imageHeight,
            BackgroundSize sizing,
            Vector2 position
        )
        {
            float x = Mathf.Clamp01(position.x);
            float y = Mathf.Clamp01(position.y);
            float iw = imageWidth;
            float ih = imageHeight;
            float bw = bounds.width;
            float bh = bounds.height;
            var destination = bounds;
            var uv = new Rect(0f, 0f, 1f, 1f);

            switch (sizing)
            {
                case BackgroundSize.Cover:
                {
                    float ratio = Mathf.Max(bw / iw, bh / ih);
                    float croppedW = Mathf.Min(1f, bw / (iw * ratio));
                    float croppedH = Mathf.Min(1f, bh / (ih * ratio));
                    uv = new Rect(
                        (1f - croppedW) * x,
                        (1f - croppedH) * (1f - y),
                        croppedW,
                        croppedH
                    );
                    break;
                }
                case BackgroundSize.Contain:
                {
                    float ratio = Mathf.Min(bw / iw, bh / ih);
                    float w = iw * ratio,
                        h = ih * ratio;
                    destination = new Rect(bounds.x + (bw - w) * x, bounds.y + (bh - h) * y, w, h);
                    break;
                }
                case BackgroundSize.OriginalSize:
                    destination = new Rect(
                        bounds.x + (bw - iw) * x,
                        bounds.y + (bh - ih) * y,
                        iw,
                        ih
                    );
                    break;
                case BackgroundSize.Stretch:
                default:
                    break;
            }

            return (destination, uv);
        }

        private static void DrawClippedTexture(
            Rect bounds,
            Rect destination,
            Texture texture,
            Rect uv,
            Color tint,
            CornerRadii radii
        )
        {
            var oldColor = GUI.color;
            GUI.color = tint;
            try
            {
                if (radii.IsSquare)
                {
                    DrawStrip(bounds, destination, texture, uv);
                    return;
                }

                float top = Mathf.Max(radii.TL, radii.TR);
                float bottom = Mathf.Max(radii.BL, radii.BR);
                float y = 0f;
                float end = bounds.height;
                while (y < end)
                {
                    float step;
                    float insetL = 0f,
                        insetR = 0f;
                    bool curved = y < top || y >= end - bottom;
                    if (curved)
                    {
                        step = Mathf.Min(1f, end - y);
                        float mid = y + step * 0.5f;
                        insetL = Mathf.Max(InsetAt(mid, end, radii.TL, radii.BL), 0f);
                        insetR = Mathf.Max(InsetAt(mid, end, radii.TR, radii.BR), 0f);
                    }
                    else
                    {
                        step = Mathf.Min(end - bottom - y, end - y);
                        if (step <= 0f)
                            step = Mathf.Min(1f, end - y);
                    }

                    var stripe = new Rect(
                        bounds.x + insetL,
                        bounds.y + y,
                        Mathf.Max(0f, bounds.width - insetL - insetR),
                        step
                    );
                    if (stripe.width > 0f && stripe.height > 0f)
                        DrawStrip(stripe, destination, texture, uv);
                    y += step;
                }
            }
            finally
            {
                GUI.color = oldColor;
            }
        }

        private static float InsetAt(float y, float h, float top, float bottom)
        {
            if (top > 0f && y < top)
            {
                float d = top - y;
                return top - Mathf.Sqrt(Mathf.Max(0f, top * top - d * d));
            }
            if (bottom > 0f && y > h - bottom)
            {
                float d = y - (h - bottom);
                return bottom - Mathf.Sqrt(Mathf.Max(0f, bottom * bottom - d * d));
            }
            return 0f;
        }

        private static void DrawStrip(Rect clip, Rect dest, Texture texture, Rect uv)
        {
            GUI.BeginGroup(clip);
            try
            {
                var local = new Rect(dest.x - clip.x, dest.y - clip.y, dest.width, dest.height);
                GUI.DrawTextureWithTexCoords(local, texture, uv);
            }
            finally
            {
                GUI.EndGroup();
            }
        }
    }
}
