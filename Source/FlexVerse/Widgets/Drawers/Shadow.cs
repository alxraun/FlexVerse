// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;

namespace FlexVerse
{
    public readonly struct Shadow
    {
        public static Shadow None => default;

        public readonly Vector2 Offset;
        public readonly float BlurRadius;
        public readonly float SpreadRadius;
        public readonly Color Color;

        public Shadow(Vector2 offset, float blurRadius, float spreadRadius, Color color)
        {
            Offset = offset;
            BlurRadius = blurRadius;
            SpreadRadius = spreadRadius;
            Color = color;
        }
    }
}
