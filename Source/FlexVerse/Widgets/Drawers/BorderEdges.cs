// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;

namespace FlexVerse
{
    internal readonly struct BorderEdges
    {
        public readonly float L,
            T,
            R,
            B;

        public BorderEdges(float l, float t, float r, float b)
        {
            L = l;
            T = t;
            R = r;
            B = b;
        }

        public bool Any => L > 0f || T > 0f || R > 0f || B > 0f;
        public string CacheKey =>
            AppearanceMasks.F(L)
            + ","
            + AppearanceMasks.F(T)
            + ","
            + AppearanceMasks.F(R)
            + ","
            + AppearanceMasks.F(B);
    }
}
