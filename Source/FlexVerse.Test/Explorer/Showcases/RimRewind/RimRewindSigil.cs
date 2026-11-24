// parents: @.frostline/graph/m-explorer.xml
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSigil : Widget
    {
        [Prop]
        public Behavior<float> Pulse { get; set; } = 0f;

        private const int BakeSize = 80;
        private const float BakeScale = 2f;
        private const float ArcRadius = 17f;
        private const float ArcStroke = 1.5f;
        private const float ChevronStroke = 2f;
        private static readonly Vector2 ChevronA = new(3.5f, 0f);
        private static readonly Vector2 ChevronB = new(-3.5f, 0f);
        private static readonly Vector2 TopArm = new(3f, -5f);
        private static readonly Vector2 MidArm = new(-2f, 0f);
        private static readonly Vector2 BottomArm = new(3f, 5f);

        private Texture2D _texture;

        public override void OnMount()
        {
            var half = BakeSize * 0.5f;
            _texture = new Texture2D(BakeSize, BakeSize, TextureFormat.ARGB32, false);
            _texture.wrapMode = TextureWrapMode.Clamp;
            var ink = RimRewindPalette.OnPrimaryContainer;
            var pixels = new Color[BakeSize * BakeSize];
            for (var y = 0; y < BakeSize; y++)
            for (var x = 0; x < BakeSize; x++)
            {
                var p = (new Vector2(x + 0.5f, y + 0.5f) - new Vector2(half, half)) / BakeScale;
                var coverage = ArcCoverage(p);
                coverage = Mathf.Max(coverage, ChevronCoverage(p, ChevronA));
                coverage = Mathf.Max(coverage, ChevronCoverage(p, ChevronB));
                pixels[y * BakeSize + x] = new Color(ink.r, ink.g, ink.b, ink.a * coverage);
            }
            _texture.SetPixels(pixels);
            _texture.Apply(false, true);
        }

        public override void OnUnmount()
        {
            if (_texture != null)
                Object.Destroy(_texture);
            _texture = null;
        }

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint || _texture == null)
                return;
            var pulse = Mathf.Clamp01(Pulse.Value);
            var size = Mathf.Min(rect.width, rect.height) * (0.728f + pulse * 0.082f);
            var draw = new Rect(
                rect.center.x - size * 0.5f,
                rect.center.y - size * 0.5f,
                size,
                size
            );
            var color = GUI.color;
            GUI.color = RimRewindSkin.Alpha(color, color.a * (0.55f + 0.4f * pulse));
            try
            {
                GUI.DrawTexture(draw, _texture);
            }
            finally
            {
                GUI.color = color;
            }
        }

        private static float ArcCoverage(Vector2 p) =>
            EdgeCoverage(Mathf.Abs(p.magnitude - ArcRadius) - ArcStroke * 0.5f);

        private static float ChevronCoverage(Vector2 p, Vector2 apex) =>
            Mathf.Max(
                SegmentCoverage(p, apex + TopArm, apex + MidArm),
                SegmentCoverage(p, apex + MidArm, apex + BottomArm)
            );

        private static float SegmentCoverage(Vector2 p, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            var t = Mathf.Clamp01(Vector2.Dot(p - a, ab) / Mathf.Max(ab.sqrMagnitude, 1e-6f));
            var closest = a + ab * t;
            return EdgeCoverage((p - closest).magnitude - ChevronStroke * 0.5f);
        }

        private static float EdgeCoverage(float distance) => Mathf.Clamp01(0.5f - distance);
    }
}
