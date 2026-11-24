// parents: @.frostline/graph/m-pickle.xml

using System;
using RimWorks.Pickle;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Pickle
{
    public partial class LabelMeasurementProbe : Label
    {
        public Size Measure(
            float width,
            MeasureMode widthMode,
            float height,
            MeasureMode heightMode
        ) => MeasureFunc(null, width, widthMode, height, heightMode);
    }

    [PickleSteps]
    public static class LabelMeasurementSteps
    {
        [BeforeScenario]
        public static void Begin() => ScenarioMarkers.Begin("M-PICKLE-LABEL");

        [AfterScenario]
        public static void End() => ScenarioMarkers.End("M-PICKLE-LABEL");

        [Then("Label measurement respects exact and maximum content constraints")]
        public static void Constraints(PickleContext ctx)
        {
            foreach (bool wrap in new[] { false, true })
            foreach (
                string text in new[]
                {
                    "",
                    "A long sentence with several words and an explicit\nline break.",
                }
            )
            {
                var label = new LabelMeasurementProbe { Content = text, TextWrap = wrap };
                Size exact = label.Measure(80f, MeasureMode.Exactly, 10f, MeasureMode.Exactly);
                ctx.Require(
                    exact.Width == 80f && exact.Height == 10f,
                    $"exact content size must be 80x10: wrap={wrap}, text='{text}', got {exact.Width}x{exact.Height}"
                );
                Size maximum = label.Measure(80f, MeasureMode.AtMost, 10f, MeasureMode.AtMost);
                ctx.Require(
                    maximum.Width >= 0f
                        && maximum.Width <= 80f
                        && maximum.Height >= 0f
                        && maximum.Height <= 10f,
                    $"AtMost content size must fit 80x10: wrap={wrap}, got {maximum.Width}x{maximum.Height}"
                );
                Size zero = label.Measure(0f, MeasureMode.AtMost, 0f, MeasureMode.AtMost);
                ctx.Require(
                    zero.Width == 0f && zero.Height == 0f,
                    "zero bounds must produce zero content size"
                );
            }
        }

        [Then("Label measurement leaves box constraints to Flexbox and restores text globals")]
        public static void Ownership(PickleContext ctx)
        {
            GameFont font = Text.Font;
            var label = new LabelMeasurementProbe
            {
                Content = "x",
                TextWrap = false,
                Font = font,
            };
            TextAnchor anchor = Text.Anchor;
            bool wrap = Text.WordWrap;
            Size natural = label.Measure(
                float.NaN,
                MeasureMode.Undefined,
                float.NaN,
                MeasureMode.Undefined
            );
            label.MinWidth = 500f.Point();
            label.MinHeight = 500f.Point();
            label.PaddingAll = 20f.Point();
            label.Font = font == GameFont.Small ? GameFont.Medium : GameFont.Small;
            label.Measure(80f, MeasureMode.Exactly, 10f, MeasureMode.AtMost);
            ctx.Require(
                Text.Font == font && Text.Anchor == anchor && Text.WordWrap == wrap,
                "measurement must restore the caller's text globals"
            );
            label.Font = font;
            Size content = label.Measure(
                float.NaN,
                MeasureMode.Undefined,
                float.NaN,
                MeasureMode.Undefined
            );
            ctx.Require(
                Math.Abs(content.Width - natural.Width) < 0.01f
                    && Math.Abs(content.Height - natural.Height) < 0.01f,
                "box min sizes and padding must not be applied inside the content measurement callback"
            );
        }
    }
}
