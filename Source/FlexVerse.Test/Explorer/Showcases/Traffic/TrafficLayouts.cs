// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    internal static class TrafficLayouts
    {
        public const int Count = 16;
        public const int Columns = 6;
        public const int Rows = 6;
        public const float Gap = 10f;
        public static readonly EasingFunction Ease = EaseInOut;

        public static Rect Frame(int column, int row) =>
            new(column / (float)Columns, (row + 1) / (float)Rows, 1f / Columns, 1f / Rows);

        private static float EaseInOut(float progress)
        {
            if (progress <= 0f || progress >= 1f)
                return Mathf.Clamp01(progress);
            var low = 0f;
            var high = 1f;
            for (var iteration = 0; iteration < 12; iteration++)
            {
                var t = (low + high) * 0.5f;
                var inverse = 1f - t;
                var x =
                    3f * inverse * inverse * t * 0.65f + 3f * inverse * t * t * 0.35f + t * t * t;
                if (x < progress)
                    low = t;
                else
                    high = t;
            }
            var parameter = (low + high) * 0.5f;
            return parameter * parameter * (3f - 2f * parameter);
        }
    }
}
