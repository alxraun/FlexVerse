// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;

namespace FlexVerse
{
    internal readonly struct CornerRadii
    {
        public readonly float TL;
        public readonly float TR;
        public readonly float BR;
        public readonly float BL;

        private CornerRadii(float tl, float tr, float br, float bl)
        {
            TL = tl;
            TR = tr;
            BR = br;
            BL = bl;
        }

        public bool IsSquare => TL <= 0f && TR <= 0f && BR <= 0f && BL <= 0f;

        public static CornerRadii Resolve(
            Rect rect,
            float all,
            float tl,
            float tr,
            float br,
            float bl
        )
        {
            all = Valid(all);
            var r = new CornerRadii(
                Valid(float.IsNaN(tl) ? all : tl),
                Valid(float.IsNaN(tr) ? all : tr),
                Valid(float.IsNaN(br) ? all : br),
                Valid(float.IsNaN(bl) ? all : bl)
            );

            float w = Mathf.Max(0f, rect.width);
            float h = Mathf.Max(0f, rect.height);
            float f = 1f;
            if (r.TL + r.TR > 0)
                f = Mathf.Min(f, w / (r.TL + r.TR));
            if (r.BL + r.BR > 0)
                f = Mathf.Min(f, w / (r.BL + r.BR));
            if (r.TL + r.BL > 0)
                f = Mathf.Min(f, h / (r.TL + r.BL));
            if (r.TR + r.BR > 0)
                f = Mathf.Min(f, h / (r.TR + r.BR));
            return new CornerRadii(r.TL * f, r.TR * f, r.BR * f, r.BL * f);
        }

        public CornerRadii Inset(float left, float top, float right, float bottom) =>
            new CornerRadii(
                Mathf.Max(0f, TL - Mathf.Max(left, top)),
                Mathf.Max(0f, TR - Mathf.Max(right, top)),
                Mathf.Max(0f, BR - Mathf.Max(right, bottom)),
                Mathf.Max(0f, BL - Mathf.Max(left, bottom))
            );

        public CornerRadii Outset(float value) =>
            new CornerRadii(
                Mathf.Max(0f, TL + value),
                Mathf.Max(0f, TR + value),
                Mathf.Max(0f, BR + value),
                Mathf.Max(0f, BL + value)
            );

        public string CacheKey =>
            AppearanceMasks.F(TL)
            + ","
            + AppearanceMasks.F(TR)
            + ","
            + AppearanceMasks.F(BR)
            + ","
            + AppearanceMasks.F(BL);

        private static float Valid(float radius) =>
            float.IsNaN(radius) || float.IsInfinity(radius) || radius < 0f ? 0f : radius;
    }
}
