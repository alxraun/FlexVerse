// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ExpectedBox : Widget
    {
        [Prop]
        public Behavior<string> Label { get; set; }

        [Prop]
        public Behavior<Length> ExpectedTop { get; set; }

        [Prop]
        public Behavior<Length> ExpectedLeft { get; set; }

        [Prop]
        public Behavior<Length> ExpectedWidth { get; set; }

        [Prop]
        public Behavior<Length> ExpectedHeight { get; set; }

        [Prop]
        public Behavior<Color> BackgroundColor { get; set; } = new Color(0.5f, 0.5f, 0.5f);

        private static readonly Dictionary<Color, Texture2D> _hatchTextures = [];

        public ExpectedBox()
        {
            PositionType = PositionTypes.Absolute;
            FlexShrink = 0f;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                var w = ExpectedWidth.Value.unit == Unit.Point ? ExpectedWidth.Value.value : 0f;
                var h = ExpectedHeight.Value.unit == Unit.Point ? ExpectedHeight.Value.value : 0f;
                return new Size(w, h);
            };

        public override void OnMount()
        {
            base.OnMount();
            if (ExpectedTop != null)
                PositionTop = ExpectedTop;
            if (ExpectedLeft != null)
                PositionLeft = ExpectedLeft;
        }

        public override void OnGUI(Rect calculatedRect)
        {
            var expectationRect = calculatedRect;

            Color baseColor =
                (BackgroundColor.Value != default) ? BackgroundColor.Value : Color.gray;
            Texture2D hatchTexture = GetOrCreateHatchTexture(baseColor);
            GUI.DrawTexture(expectationRect, hatchTexture, ScaleMode.StretchToFill);

            var originalGUIColor = GUI.color;
            GUI.color = baseColor;
            Widgets.DrawBox(expectationRect, 1);
            GUI.color = originalGUIColor;

            if (!string.IsNullOrEmpty(Label.Value))
            {
                var originalAnchor = Text.Anchor;
                try
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(expectationRect, Label.Value);
                }
                finally
                {
                    Text.Anchor = originalAnchor;
                }
            }
        }

        private static Texture2D GetOrCreateHatchTexture(Color baseColor)
        {
            if (_hatchTextures.TryGetValue(baseColor, out Texture2D texture))
            {
                return texture;
            }

            int textureSize = 16;
            texture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false)
            {
                name = "HatchTexture_" + baseColor.ToString(),
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Point,
            };

            Color bgColor = new(baseColor.r, baseColor.g, baseColor.b, 0.1f);
            Color lineColor = new(baseColor.r, baseColor.g, baseColor.b, 0.3f);

            for (int y = 0; y < textureSize; y++)
            {
                for (int x = 0; x < textureSize; x++)
                {
                    if ((x + y) % 4 < 2)
                    {
                        texture.SetPixel(x, y, lineColor);
                    }
                    else
                    {
                        texture.SetPixel(x, y, bgColor);
                    }
                }
            }

            texture.Apply();
            _hatchTextures[baseColor] = texture;
            return texture;
        }
    }
}
