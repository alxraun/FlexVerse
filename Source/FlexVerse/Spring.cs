// parents: @.frostline/graph/m-spring-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Authors spring behaviors with tension, friction and mass parameters. (M-SPRING-BEHAVIOR)</summary>
    public static class Spring
    {
        /// <summary>Creates a spring toward a target with full configuration.</summary>
        public static Behavior<T> To<T>(
            T targetValue,
            float tension = 300f,
            float friction = 26f,
            float mass = 1f,
            float restThreshold = 0.001f,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new SpringBehavior<T>
            {
                Target = targetValue,
                Tension = tension,
                Friction = friction,
                Mass = mass,
                RestThreshold = restThreshold,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a spring from a start value with full configuration.</summary>
        public static Behavior<T> From<T>(
            T startValue,
            float tension = 300f,
            float friction = 26f,
            float mass = 1f,
            float restThreshold = 0.001f,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new SpringBehavior<T>
            {
                Value = startValue,
                Tension = tension,
                Friction = friction,
                Mass = mass,
                RestThreshold = restThreshold,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a spring between a start value and a target with initial velocity.</summary>
        public static Behavior<T> FromTo<T>(
            T startValue,
            T targetValue,
            T initialVelocity,
            float tension = 300f,
            float friction = 26f,
            float mass = 1f,
            float restThreshold = 0.001f,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new SpringBehavior<T>
            {
                Value = startValue,
                Target = targetValue,
                InitialVelocity = initialVelocity,
                Tension = tension,
                Friction = friction,
                Mass = mass,
                RestThreshold = restThreshold,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a spring holding a state value as both value and target.</summary>
        public static Behavior<T> State<T>(
            T value,
            float tension = 300f,
            float friction = 26f,
            float mass = 1f,
            float restThreshold = 0.001f,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var behavior = new SpringBehavior<T>
            {
                Value = value,
                Target = value,
                Tension = tension,
                Friction = friction,
                Mass = mass,
                RestThreshold = restThreshold,
            };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }
    }
}
