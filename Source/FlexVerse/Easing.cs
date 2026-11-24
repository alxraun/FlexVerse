// parents: @.frostline/graph/m-tween-behavior.xml

using UnityEngine;

namespace FlexVerse
{
    /// <summary>Maps normalized progress onto an eased value.</summary>
    public delegate float EasingFunction(float progress);

    /// <summary>Supplies the built-in easing curve library. (M-TWEEN-BEHAVIOR)</summary>
    public static class Easing
    {
        /// <summary>Passes progress through unchanged.</summary>
        public static readonly EasingFunction Linear = progress => progress;

        /// <summary>Accelerates quadratically from rest.</summary>
        public static readonly EasingFunction QuadEaseIn = progress => progress * progress;

        /// <summary>Decelerates quadratically into rest.</summary>
        public static readonly EasingFunction QuadEaseOut = progress =>
            1f - (1f - progress) * (1f - progress);

        /// <summary>Combines quadratic acceleration and deceleration.</summary>
        public static readonly EasingFunction QuadEaseInOut = progress =>
            progress < 0.5f
                ? 2f * progress * progress
                : 1f - Mathf.Pow(-2f * progress + 2f, 2f) / 2f;

        /// <summary>Accelerates cubically from rest.</summary>
        public static readonly EasingFunction CubicEaseIn = progress =>
            progress * progress * progress;

        /// <summary>Decelerates cubically into rest.</summary>
        public static readonly EasingFunction CubicEaseOut = progress =>
            1f - Mathf.Pow(1f - progress, 3f);

        /// <summary>Combines cubic acceleration and deceleration.</summary>
        public static readonly EasingFunction CubicEaseInOut = progress =>
            progress < 0.5f
                ? 4f * progress * progress * progress
                : 1f - Mathf.Pow(-2f * progress + 2f, 3f) / 2f;

        /// <summary>Accelerates along a sine half-wave from rest.</summary>
        public static readonly EasingFunction SineEaseIn = progress =>
            1f - Mathf.Cos(progress * Mathf.PI / 2f);

        /// <summary>Decelerates along a sine half-wave into rest.</summary>
        public static readonly EasingFunction SineEaseOut = progress =>
            Mathf.Sin(progress * Mathf.PI / 2f);

        /// <summary>Combines sine acceleration and deceleration.</summary>
        public static readonly EasingFunction SineEaseInOut = progress =>
            -(Mathf.Cos(Mathf.PI * progress) - 1f) / 2f;

        private const float S_BACK = 1.70158f;
        private const float S_BACK_INOUT = S_BACK * 1.525f;

        /// <summary>Pulls back slightly before accelerating forward.</summary>
        public static readonly EasingFunction BackEaseIn = progress =>
            progress * progress * ((S_BACK + 1f) * progress - S_BACK);

        /// <summary>Overshoots the target before settling.</summary>
        public static readonly EasingFunction BackEaseOut = progress =>
        {
            var t = progress - 1f;
            return Mathf.Pow(t, 2f) * ((S_BACK + 1f) * t + S_BACK) + 1f;
        };

        /// <summary>Combines back pull-in and overshoot around the midpoint.</summary>
        public static readonly EasingFunction BackEaseInOut = progress =>
        {
            var progress2 = progress * 2f;
            if (progress2 < 1f)
            {
                return 0.5f
                    * progress2
                    * progress2
                    * ((S_BACK_INOUT + 1f) * progress2 - S_BACK_INOUT);
            }
            var tInOut = progress2 - 2f;
            return 0.5f * (tInOut * tInOut * ((S_BACK_INOUT + 1f) * tInOut + S_BACK_INOUT) + 2f);
        };

        /// <summary>Accelerates along a circular arc from rest.</summary>
        public static readonly EasingFunction CircEaseIn = progress =>
            1f - Mathf.Sqrt(1f - Mathf.Pow(progress, 2f));

        /// <summary>Decelerates along a circular arc into rest.</summary>
        public static readonly EasingFunction CircEaseOut = progress =>
            Mathf.Sqrt(1f - Mathf.Pow(progress - 1f, 2f));

        /// <summary>Combines circular acceleration and deceleration.</summary>
        public static readonly EasingFunction CircEaseInOut = progress =>
            progress < 0.5f
                ? (1f - Mathf.Sqrt(1f - Mathf.Pow(2f * progress, 2f))) / 2f
                : (Mathf.Sqrt(1f - Mathf.Pow(-2f * progress + 2f, 2f)) + 1f) / 2f;

        /// <summary>Accelerates exponentially from rest.</summary>
        public static readonly EasingFunction ExpoEaseIn = progress =>
            progress == 0f ? 0f : Mathf.Pow(2f, 10f * (progress - 1f));

        /// <summary>Decelerates exponentially into rest.</summary>
        public static readonly EasingFunction ExpoEaseOut = progress =>
            progress == 1f ? 1f : 1f - Mathf.Pow(2f, -10f * progress);

        /// <summary>Combines exponential acceleration and deceleration.</summary>
        public static readonly EasingFunction ExpoEaseInOut = progress =>
        {
            if (progress == 0f)
                return 0f;
            if (progress == 1f)
                return 1f;
            var progress2 = progress * 2f;
            if (progress2 < 1f)
            {
                return 0.5f * Mathf.Pow(2f, 10f * (progress2 - 1f));
            }
            return 0.5f * (2f - Mathf.Pow(2f, -10f * (progress2 - 1f)));
        };

        private const float ELASTIC_PERIOD_DEFAULT = 0.3f;
        private const float ELASTIC_AMPLITUDE_DEFAULT = 1f;
        private const float ELASTIC_S_DEFAULT = ELASTIC_PERIOD_DEFAULT / 4f;

        /// <summary>Oscillates with growing amplitude from rest.</summary>
        public static readonly EasingFunction ElasticEaseIn = progress =>
        {
            if (progress == 0f)
                return 0f;
            if (progress == 1f)
                return 1f;
            var t = progress - 1f;
            return -(
                ELASTIC_AMPLITUDE_DEFAULT
                * Mathf.Pow(2f, 10f * t)
                * Mathf.Sin((t - ELASTIC_S_DEFAULT) * (2f * Mathf.PI) / ELASTIC_PERIOD_DEFAULT)
            );
        };

        /// <summary>Oscillates with decaying amplitude into rest.</summary>
        public static readonly EasingFunction ElasticEaseOut = progress =>
        {
            if (progress == 0f)
                return 0f;
            if (progress == 1f)
                return 1f;
            return ELASTIC_AMPLITUDE_DEFAULT
                    * Mathf.Pow(2f, -10f * progress)
                    * Mathf.Sin(
                        (progress - ELASTIC_S_DEFAULT) * (2f * Mathf.PI) / ELASTIC_PERIOD_DEFAULT
                    )
                + 1f;
        };

        private const float ELASTIC_PERIOD_INOUT = ELASTIC_PERIOD_DEFAULT * 1.5f;
        private const float ELASTIC_S_INOUT = ELASTIC_PERIOD_INOUT / 4f;

        /// <summary>Oscillates with mirrored decay around the midpoint.</summary>
        public static readonly EasingFunction ElasticEaseInOut = progress =>
        {
            if (progress == 0f)
                return 0f;
            var progress2 = progress * 2f;
            if (progress2 == 2f)
                return 1f;

            var t = progress2 - 1f;

            if (progress2 < 1f)
            {
                return -0.5f
                    * (
                        ELASTIC_AMPLITUDE_DEFAULT
                        * Mathf.Pow(2f, 10f * t)
                        * Mathf.Sin((t - ELASTIC_S_INOUT) * (2f * Mathf.PI) / ELASTIC_PERIOD_INOUT)
                    );
            }
            else
            {
                return ELASTIC_AMPLITUDE_DEFAULT
                        * Mathf.Pow(2f, -10f * t)
                        * Mathf.Sin((t - ELASTIC_S_INOUT) * (2f * Mathf.PI) / ELASTIC_PERIOD_INOUT)
                        * 0.5f
                    + 1f;
            }
        };

        /// <summary>Accelerates quartically from rest.</summary>
        public static readonly EasingFunction QuartEaseIn = progress => Mathf.Pow(progress, 4f);

        /// <summary>Decelerates quartically into rest.</summary>
        public static readonly EasingFunction QuartEaseOut = progress =>
            1f - Mathf.Pow(1f - progress, 4f);

        /// <summary>Combines quartic acceleration and deceleration.</summary>
        public static readonly EasingFunction QuartEaseInOut = progress =>
            progress < 0.5f
                ? 8f * Mathf.Pow(progress, 4f)
                : 1f - Mathf.Pow(-2f * progress + 2f, 4f) / 2f;

        /// <summary>Accelerates quintically from rest.</summary>
        public static readonly EasingFunction QuintEaseIn = progress => Mathf.Pow(progress, 5f);

        /// <summary>Decelerates quintically into rest.</summary>
        public static readonly EasingFunction QuintEaseOut = progress =>
            1f + Mathf.Pow(progress - 1f, 5f);

        /// <summary>Combines quintic acceleration and deceleration.</summary>
        public static readonly EasingFunction QuintEaseInOut = progress =>
            progress < 0.5f
                ? 16f * Mathf.Pow(progress, 5f)
                : 1f + Mathf.Pow(2f * progress - 2f, 5f) / 2f;
    }
}
