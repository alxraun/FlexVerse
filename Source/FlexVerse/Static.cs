// parents: @.frostline/graph/m-static-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Authors immediate or delayed static value behaviors. (M-STATIC-BEHAVIOR)</summary>
    public static class Static
    {
        /// <summary>Creates an immediate static value behavior.</summary>
        public static Behavior<T> Value<T>(
            T value,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new StaticBehavior<T> { Target = value };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a delayed static value behavior.</summary>
        public static Behavior<T> Value<T>(
            T value,
            float duration,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new StaticBehavior<T> { Target = value, Duration = duration };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a delay-only static behavior without a target.</summary>
        public static Behavior<T> Wait<T>(
            float duration,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new StaticBehavior<T> { Duration = duration };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }
    }
}
