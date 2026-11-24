// parents: @.frostline/graph/m-image-widget.xml

using UnityEngine;
using Verse;

namespace FlexVerse
{
    public enum ImageResizeMode
    {
        ScaleToFit,
        ScaleAndCrop,
        StretchToFill,
    }

    public partial class Image : Widget
    {
        [Prop]
        public Behavior<string> TexturePath { get; set; }

        [Prop]
        public Behavior<Texture2D> TextureInstance { get; set; }

        [Prop]
        public Behavior<string> Tooltip { get; set; }

        [Prop]
        public Behavior<Color> TintColor { get; set; } = Color.white;

        [Prop]
        public Behavior<float> BorderRadius { get; set; } = 0f;

        [Prop]
        public Behavior<float> BorderTopLeftRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderTopRightRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderBottomRightRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<float> BorderBottomLeftRadius { get; set; } = float.NaN;

        [Prop]
        public Behavior<ImageResizeMode> ResizeMode { get; set; } = ImageResizeMode.ScaleToFit;

        [State]
        protected Behavior<Texture2D> ResolvedTexture { get; set; }

        [State]
        protected Behavior<string> LastResolvedPath { get; set; }

        [State]
        protected Behavior<Texture2D> LastResolvedInstance { get; set; }

        public Image()
        {
            FlexShrink = 0f;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                ResolveTexture();
                if (
                    ResolvedTexture.Value == null
                    || (ResolvedTexture.Value.width <= 0 && ResolvedTexture.Value.height <= 0)
                )
                {
                    return new Size(0, 0);
                }

                float intrinsicWidth = ResolvedTexture.Value.width;
                float intrinsicHeight = ResolvedTexture.Value.height;
                float intrinsicAspectRatio =
                    (intrinsicHeight > 0) ? intrinsicWidth / intrinsicHeight : 0;

                bool aspectRatioDefinedInStyle = AspectRatio.Value != default;
                float aspectRatio = aspectRatioDefinedInStyle
                    ? AspectRatio.Value
                    : intrinsicAspectRatio;

                float desiredWidth = -1;
                float desiredHeight = -1;

                bool styleWidthDefined =
                    Width.Value != null
                    && Width.Value.unit != Unit.Undefined
                    && Width.Value.unit == Unit.Point;
                bool styleHeightDefined =
                    Height.Value != null
                    && Height.Value.unit != Unit.Undefined
                    && Height.Value.unit == Unit.Point;

                if (styleWidthDefined)
                    desiredWidth = Width.Value.value;
                if (styleHeightDefined)
                    desiredHeight = Height.Value.value;

                float resultWidth = 0;
                float resultHeight = 0;

                if (styleWidthDefined && styleHeightDefined)
                {
                    resultWidth = desiredWidth;
                    resultHeight = desiredHeight;
                }
                else if (aspectRatio > 0)
                {
                    if (styleWidthDefined)
                    {
                        resultWidth = desiredWidth;
                        resultHeight = resultWidth / aspectRatio;
                    }
                    else if (styleHeightDefined)
                    {
                        resultHeight = desiredHeight;
                        resultWidth = resultHeight * aspectRatio;
                    }
                    else
                    {
                        resultWidth = intrinsicWidth;
                        resultHeight = intrinsicHeight;
                    }
                }
                else
                {
                    resultWidth = intrinsicWidth;
                    resultHeight = intrinsicHeight;
                }

                if (widthMode == MeasureMode.Exactly)
                {
                    resultWidth = width;
                    if (aspectRatio > 0)
                    {
                        resultHeight = resultWidth / aspectRatio;
                    }
                    if (heightMode == MeasureMode.AtMost)
                    {
                        resultHeight = Mathf.Min(resultHeight, height);
                    }
                }
                else if (heightMode == MeasureMode.Exactly)
                {
                    resultHeight = height;
                    if (aspectRatio > 0)
                    {
                        resultWidth = resultHeight * aspectRatio;
                    }
                    if (widthMode == MeasureMode.AtMost)
                    {
                        resultWidth = Mathf.Min(resultWidth, width);
                    }
                }
                else
                {
                    float potentialHeightFromWidth =
                        aspectRatio > 0 ? width / aspectRatio : float.MaxValue;
                    float potentialWidthFromHeight =
                        aspectRatio > 0 ? height * aspectRatio : float.MaxValue;

                    if (aspectRatio > 0)
                    {
                        if (potentialHeightFromWidth <= height)
                        {
                            resultWidth = width;
                            resultHeight = potentialHeightFromWidth;
                        }
                        else
                        {
                            resultHeight = height;
                            resultWidth = potentialWidthFromHeight;
                        }
                    }
                    else
                    {
                        if (widthMode == MeasureMode.AtMost)
                            resultWidth = width;
                        if (heightMode == MeasureMode.AtMost)
                            resultHeight = height;
                    }

                    if (
                        MinWidth.Value != null
                        && MinWidth.Value.unit != Unit.Undefined
                        && MinWidth.Value.unit == Unit.Point
                        && MinWidth.Value.value >= 0
                    )
                        resultWidth = Mathf.Max(resultWidth, MinWidth.Value.value);
                    if (
                        MinHeight.Value != null
                        && MinHeight.Value.unit != Unit.Undefined
                        && MinHeight.Value.unit == Unit.Point
                        && MinHeight.Value.value >= 0
                    )
                        resultHeight = Mathf.Max(resultHeight, MinHeight.Value.value);
                    if (
                        MaxWidth.Value != null
                        && MaxWidth.Value.unit != Unit.Undefined
                        && MaxWidth.Value.unit == Unit.Point
                        && MaxWidth.Value.value >= 0
                    )
                        resultWidth = Mathf.Min(resultWidth, MaxWidth.Value.value);
                    if (
                        MaxHeight.Value != null
                        && MaxHeight.Value.unit != Unit.Undefined
                        && MaxHeight.Value.unit == Unit.Point
                        && MaxHeight.Value.value >= 0
                    )
                        resultHeight = Mathf.Min(resultHeight, MaxHeight.Value.value);
                }

                resultWidth = Mathf.Max(0, resultWidth);
                resultHeight = Mathf.Max(0, resultHeight);

                return new Size(resultWidth, resultHeight);
            };

        public override void OnGUI(Rect calculatedRect)
        {
            ResolveTexture();
            var radii = CornerRadii.Resolve(
                calculatedRect,
                BorderRadius.Value,
                BorderTopLeftRadius.Value,
                BorderTopRightRadius.Value,
                BorderBottomRightRadius.Value,
                BorderBottomLeftRadius.Value
            );

            if (ResolvedTexture.Value != null)
            {
                ImageDrawer.DrawContent(
                    calculatedRect,
                    ResolvedTexture.Value,
                    ResizeMode.Value,
                    TintColor.Value,
                    radii
                );
            }

            if (!string.IsNullOrEmpty(Tooltip.Value))
                TooltipHandler.TipRegion(calculatedRect, Tooltip.Value);
        }

        private void ResolveTexture()
        {
            if (
                TexturePath.Value == LastResolvedPath.Value
                && TextureInstance.Value == LastResolvedInstance.Value
            )
            {
                if (
                    ResolvedTexture.Value != null
                    || (TexturePath.Value == null && TextureInstance.Value == null)
                )
                {
                    return;
                }
            }

            ResolvedTexture.Target = null;

            if (TextureInstance.Value != null)
            {
                ResolvedTexture.Target = TextureInstance.Value;
            }
            else if (!string.IsNullOrEmpty(TexturePath.Value))
            {
                var texture = ContentFinder<Texture2D>.Get(TexturePath.Value, false);
                if (texture == null)
                {
                    Log.Warning("M-IMAGE-WIDGET", "texture.not_found", ("path", TexturePath.Value));
                }
                ResolvedTexture.Target = texture;
            }

            LastResolvedPath.Target = TexturePath.Value;
            LastResolvedInstance.Target = TextureInstance.Value;
        }
    }
}
