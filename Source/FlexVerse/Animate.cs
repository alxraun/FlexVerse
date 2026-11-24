// parents: @.frostline/graph/m-tween-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Authors tween and keyframe behaviors with optional event hooks. (M-TWEEN-BEHAVIOR)</summary>
    public static class Animate
    {
        /// <summary>Creates a tween toward a target with full configuration.</summary>
        public static TweenBehavior<T> To<T>(
            T target,
            float duration = 1f,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new TweenBehavior<T>
            {
                Target = target,
                Duration = duration,
                MinDuration = minDuration ?? duration,
                MaxDuration = maxDuration ?? duration,
                Easing = easing ?? Easing.Linear,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a one-second linear tween toward a target.</summary>
        public static TweenBehavior<T> To<T>(T target) => To(target, 1f, null, null, null);

        /// <summary>Creates a linear tween toward a target over a duration.</summary>
        public static TweenBehavior<T> To<T>(T target, float duration) =>
            To(target, duration, null, null, null);

        /// <summary>Creates an eased tween toward a target over a duration.</summary>
        public static TweenBehavior<T> To<T>(T target, float duration, EasingFunction easing) =>
            To(target, duration, null, null, easing);

        /// <summary>Creates a tween from a start value with full configuration.</summary>
        public static TweenBehavior<T> From<T>(
            T value,
            float duration = 1f,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new TweenBehavior<T>
            {
                Value = value,
                Duration = duration,
                MinDuration = minDuration ?? duration,
                MaxDuration = maxDuration ?? duration,
                Easing = easing ?? Easing.Linear,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a one-second linear tween from a start value.</summary>
        public static TweenBehavior<T> From<T>(T value) => From(value, 1f, null, null, null);

        /// <summary>Creates a linear tween from a start value over a duration.</summary>
        public static TweenBehavior<T> From<T>(T value, float duration) =>
            From(value, duration, null, null, null);

        /// <summary>Creates an eased tween from a start value over a duration.</summary>
        public static TweenBehavior<T> From<T>(T value, float duration, EasingFunction easing) =>
            From(value, duration, null, null, easing);

        /// <summary>Creates a tween between a start value and a target with full configuration.</summary>
        public static TweenBehavior<T> FromTo<T>(
            T value,
            T target,
            float duration = 1f,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new TweenBehavior<T>
            {
                Value = value,
                Target = target,
                Duration = duration,
                MinDuration = minDuration ?? duration,
                MaxDuration = maxDuration ?? duration,
                Easing = easing ?? Easing.Linear,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a one-second linear tween between a start value and a target.</summary>
        public static TweenBehavior<T> FromTo<T>(T value, T target) =>
            FromTo(value, target, 1f, null, null, null);

        /// <summary>Creates a linear tween between a start value and a target over a duration.</summary>
        public static TweenBehavior<T> FromTo<T>(T value, T target, float duration) =>
            FromTo(value, target, duration, null, null, null);

        /// <summary>Creates an eased tween between a start value and a target over a duration.</summary>
        public static TweenBehavior<T> FromTo<T>(
            T value,
            T target,
            float duration,
            EasingFunction easing
        ) => FromTo(value, target, duration, null, null, easing);

        /// <summary>Creates a tween holding a state value as both value and target.</summary>
        public static TweenBehavior<T> State<T>(
            T value,
            float duration = 1f,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new TweenBehavior<T>
            {
                Target = value,
                Value = value,
                Duration = duration,
                MinDuration = minDuration ?? duration,
                MaxDuration = maxDuration ?? duration,
                Easing = easing ?? Easing.Linear,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a one-second tween holding a state value.</summary>
        public static TweenBehavior<T> State<T>(T value) => State(value, 1f, null, null, null);

        /// <summary>Creates a tween holding a state value over a duration.</summary>
        public static TweenBehavior<T> State<T>(T value, float duration) =>
            State(value, duration, null, null, null);

        /// <summary>Creates an eased tween holding a state value over a duration.</summary>
        public static TweenBehavior<T> State<T>(T value, float duration, EasingFunction easing) =>
            State(value, duration, null, null, easing);

        /// <summary>Creates a keyframe behavior over the given keyframes. (M-KEYFRAME-BEHAVIOR)</summary>
        public static KeyframeBehavior<T> Keyframes<T>(
            float duration,
            params Keyframe<T>[] keyframes
        )
        {
            return new KeyframeBehavior<T> { Duration = duration, Keyframes = keyframes };
        }

        /// <summary>Creates a layout-direction state tween pending implementation.</summary>
        [Obsolete("Layout-aware animations are not yet implemented.")]
        public static Behavior<FlexDirections> State(
            FlexDirections value,
            float duration,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>Creates an alignment state tween pending implementation.</summary>
        [Obsolete("Layout-aware animations are not yet implemented.")]
        public static Behavior<Align> State(
            Align value,
            float duration,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>Creates a justification state tween pending implementation.</summary>
        [Obsolete("Layout-aware animations are not yet implemented.")]
        public static Behavior<Justify> State(
            Justify value,
            float duration,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            throw new NotImplementedException();
        }

        /// <summary>Creates a wrap state tween pending implementation.</summary>
        [Obsolete("Layout-aware animations are not yet implemented.")]
        public static Behavior<Wrap> State(
            Wrap value,
            float duration,
            float? minDuration = null,
            float? maxDuration = null,
            EasingFunction easing = null,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            throw new NotImplementedException();
        }
    }
}
