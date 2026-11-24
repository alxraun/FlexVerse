// parents: @.frostline/graph/m-xunit.xml

using System;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// R08: tests for immutable painter geometry and cached Unity textures.
    /// Authored for the independent verification agent; not run here.
    /// </summary>
    [Collection("Renderer GUI")]
    public class AppearanceTests
    {
        [Fact]
        public void CornerRadii_NormalizesOversizeAndHonorsExplicitZero()
        {
            var rect = new Rect(0, 0, 100, 50);
            var round = CornerRadii.Resolve(rect, 80f, float.NaN, float.NaN, float.NaN, float.NaN);
            Assert.Equal(25f, round.TL);
            Assert.Equal(25f, round.TR);
            Assert.Equal(25f, round.BL);
            Assert.Equal(25f, round.BR);

            var overridden = CornerRadii.Resolve(rect, 12f, 0f, float.NaN, float.NaN, float.NaN);
            Assert.Equal(0f, overridden.TL);
            Assert.Equal(12f, overridden.TR);
            Assert.Equal(12f, overridden.BL);
            Assert.Equal(12f, overridden.BR);
        }

        [Fact]
        public void RoundedMask_AssignsTopLeftAndBottomLeftIndependently()
        {
            var rect = new Rect(0, 0, 32, 32);
            var radii = CornerRadii.Resolve(rect, 12f, 0f, 12f, 12f, 12f);
            var texture = AppearanceMasks.GetFill(rect, radii);

            Assert.True(texture.GetPixel(0, texture.height - 1).a > 0.9f);
            Assert.True(texture.GetPixel(0, 0).a < 0.2f);
            Assert.True(texture.GetPixel(16, 16).a > 0.9f);
        }

        [Fact]
        public void RoundedBorder_UsesEachGeometricEdgeThickness()
        {
            var rect = new Rect(0, 0, 40, 40);
            var radii = CornerRadii.Resolve(rect, 10f, float.NaN, float.NaN, float.NaN, float.NaN);
            var border = AppearanceMasks.GetBorder(rect, radii, new BorderEdges(3f, 5f, 7f, 9f));

            Assert.True(border.GetPixel(20, 39).a > 0.5f);
            Assert.True(border.GetPixel(20, 0).a > 0.5f);
            Assert.True(border.GetPixel(0, 20).a > 0.5f);
            Assert.True(border.GetPixel(39, 20).a > 0.5f);
            Assert.True(border.GetPixel(20, 20).a < 0.1f);
        }

        [Fact]
        public void SurfaceMasks_ShareTextureAcrossColorsButNotGeometry()
        {
            var rect = new Rect(0, 0, 48, 32);
            var rad = CornerRadii.Resolve(rect, 6f, float.NaN, float.NaN, float.NaN, float.NaN);
            var a = AppearanceMasks.GetFill(rect, rad);
            var b = AppearanceMasks.GetFill(rect, rad);

            Assert.Same(a, b);
            Assert.NotSame(
                a,
                AppearanceMasks.GetFill(
                    rect,
                    CornerRadii.Resolve(rect, 8f, float.NaN, float.NaN, float.NaN, float.NaN)
                )
            );
        }

        [Fact]
        public void OuterShadow_BlursOutsideButNeverCoversTranslucentSurfaceInterior()
        {
            var textureRect = new Rect(0, 0, 60, 50);
            var shadowShape = new Rect(10, 10, 40, 30);
            var surface = new Rect(12, 12, 36, 26);
            var radii = CornerRadii.Resolve(
                shadowShape,
                8f,
                float.NaN,
                float.NaN,
                float.NaN,
                float.NaN
            );
            var ownRadii = CornerRadii.Resolve(
                surface,
                8f,
                float.NaN,
                float.NaN,
                float.NaN,
                float.NaN
            );
            var texture = AppearanceMasks.GetShadow(
                textureRect,
                shadowShape,
                radii,
                surface,
                ownRadii,
                4f
            );

            Assert.True(texture.GetPixel(30, 25).a < 0.01f);
            Assert.True(texture.GetPixel(10, 25).a > 0.5f);
            Assert.True(texture.GetPixel(9, 25).a > 0f);
            Assert.True(texture.GetPixel(0, 0).a < 0.05f);
        }

        [Fact]
        public void GradientFactory_LinearStopsAreSortedAndCached()
        {
            var stops = new[]
            {
                new GradientStop(1f, Color.blue),
                new GradientStop(0.5f, Color.green),
                new GradientStop(0f, Color.red),
            };
            var gradient = GradientTextures.Linear(9, 1, 0f, stops);
            var same = GradientTextures.Linear(9, 1, 0f, stops);

            Assert.Same(gradient, same);
            Assert.Equal(9, gradient.width);
            Assert.Equal(1, gradient.height);
            Assert.True(gradient.GetPixel(0, 0).r > gradient.GetPixel(0, 0).b);
            Assert.True(gradient.GetPixel(4, 0).g > 0.95f);
            Assert.True(gradient.GetPixel(8, 0).b > gradient.GetPixel(8, 0).r);
        }

        [Fact]
        public void GradientFactory_RadialAndConicHaveIndependentStopCurves()
        {
            var radial = GradientTextures.Radial(
                33,
                33,
                new Vector2(0.5f, 0.5f),
                0.5f,
                new GradientStop(0f, Color.white),
                new GradientStop(1f, Color.black)
            );
            var conic = GradientTextures.Conic(
                33,
                33,
                new Vector2(0.5f, 0.5f),
                0f,
                new GradientStop(0f, Color.red),
                new GradientStop(1f, Color.blue)
            );

            Assert.True(radial.GetPixel(16, 16).r > 0.98f);
            Assert.True(radial.GetPixel(0, 16).r < 0.2f);
            Assert.NotEqual(conic.GetPixel(28, 16), conic.GetPixel(16, 28));
        }

        [Fact]
        public void GradientFactory_DistinguishesTransparentStopsAndRejectsInvalidInput()
        {
            var clearGradient = GradientTextures.Linear(new Color(1f, 1f, 1f, 0f), Color.white, 8);
            var opaqueGradient = GradientTextures.Linear(Color.white, Color.white, 8);

            Assert.NotSame(clearGradient, opaqueGradient);
            Assert.True(clearGradient.GetPixel(0, 0).a < 1f);
            Assert.Equal(1f, opaqueGradient.GetPixel(0, 0).a);

            Assert.Throws<ArgumentException>(() =>
                GradientTextures.Linear(10, 10, 0f, Array.Empty<GradientStop>())
            );
        }

        [Fact]
        public void BackgroundImageModes_ResolveCoverContainStretchAndOriginalSize()
        {
            var bounds = new Rect(10f, 20f, 100f, 100f);
            var center = new Vector2(0.5f, 0.5f);

            var cover = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.Cover,
                center
            );
            Assert.Equal(bounds, cover.Destination);
            Assert.Equal(new Rect(0.25f, 0f, 0.5f, 1f), cover.Uv);

            var contain = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.Contain,
                center
            );
            Assert.Equal(new Rect(10f, 45f, 100f, 50f), contain.Destination);
            Assert.Equal(new Rect(0f, 0f, 1f, 1f), contain.Uv);

            var original = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.OriginalSize,
                center
            );
            Assert.Equal(new Rect(-40f, 20f, 200f, 100f), original.Destination);

            var stretch = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.Stretch,
                center
            );
            Assert.Equal(bounds, stretch.Destination);
            Assert.Equal(new Rect(0f, 0f, 1f, 1f), stretch.Uv);
        }

        [Fact]
        public void BackgroundPosition_SelectsStartOrEndOfCoverCrop()
        {
            var bounds = new Rect(0f, 0f, 50f, 100f);
            var left = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.Cover,
                Vector2.zero
            );
            var right = ImageDrawer.ResolveImagePlacement(
                bounds,
                200f,
                100f,
                BackgroundSize.Cover,
                Vector2.one
            );

            Assert.Equal(0f, left.Uv.x);
            Assert.Equal(0.75f, right.Uv.x);
            Assert.Equal(left.Uv.width, right.Uv.width);
        }

        [Fact]
        public void StyleOnlyProps_DontModifyFlexboxIntrinsicMeasurement()
        {
            var engine = new WidgetEngine("r08_view_layout");
            var gradient = GradientTextures.Linear(Color.red, Color.blue, 8);
            var definition = new View
            {
                Width = 120f.Point(),
                Height = 80f.Point(),
                BackgroundImage = gradient,
                BackgroundColor = new Color(0f, 0f, 0f, 0.5f),
                BorderRadius = 14f,
                BorderTopLeftRadius = 0f,
                BoxShadow = new Shadow(new Vector2(6f, 7f), 5f, 2f, new Color(0f, 0f, 0f, 0.6f)),
            };

            engine.ProcessUI(definition, new Rect(0f, 0f, 120f, 80f));
            var widget = engine.GetRoot();
            var rect = engine.GetLayoutResults()[widget];

            Assert.Equal(120f, rect.width);
            Assert.Equal(80f, rect.height);
            Assert.Equal(Matrix4x4.identity, GUI.matrix);
        }
    }
}
