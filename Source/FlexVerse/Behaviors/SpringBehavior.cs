// parents: @.frostline/graph/m-spring-behavior.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Integrates a physical spring toward its target until it settles. (M-SPRING-BEHAVIOR)</summary>
    public class SpringBehavior<T> : Behavior<T>
    {
        private float _tension = 300f;
        private float _friction = 26f;
        private float _mass = 1f;
        private float _restThreshold = 0.001f;
        private T _initialVelocity = default!;

        private T _velocity = default!;

        private bool _isInitialVelocitySetByUser;

        /// <summary>Gets or sets the spring tension while the spring is inactive.</summary>
        public float Tension
        {
            get => _tension;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change Tension while behavior is active."
                    );
                }
                _tension = value;
            }
        }

        /// <summary>Gets or sets the damping friction while the spring is inactive.</summary>
        public float Friction
        {
            get => _friction;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change Friction while behavior is active."
                    );
                }
                _friction = value;
            }
        }

        /// <summary>Gets or sets the spring mass while the spring is inactive.</summary>
        public float Mass
        {
            get => _mass;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change Mass while behavior is active."
                    );
                }
                _mass = value;
            }
        }

        /// <summary>Gets or sets the velocity applied when the spring activates.</summary>
        public T InitialVelocity
        {
            get => _initialVelocity;
            set
            {
                _initialVelocity = value;
                _isInitialVelocitySetByUser = true;
            }
        }

        /// <summary>Gets or sets the settle epsilon used by rest detection.</summary>
        public float RestThreshold
        {
            get => _restThreshold;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change RestThreshold while behavior is active."
                    );
                }
                _restThreshold = value;
            }
        }

        /// <summary>Activates the spring with its configured initial velocity.</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;
            _velocity = _isInitialVelocitySetByUser ? _initialVelocity : default!;

            _onActivate?.Invoke();
            StartUpdating();
        }

        /// <summary>Substep cap: keeps explicit integration stable for stiff springs on any frame length.</summary>
        internal const float MaxStep = 1f / 120f;

        /// <summary>Wall-clock clamp so a hitch (GC, scene load) cannot explode the spring.</summary>
        internal const float MaxDelta = 0.25f;

        /// <summary>Integrates the spring in stable substeps toward the target. (REQ-001)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating)
                return;

            var remaining = Mathf.Min(deltaTime, MaxDelta);
            while (remaining > 0f)
            {
                var step = Mathf.Min(remaining, MaxStep);
                remaining -= step;
                Integrate(step);
                if (!IsUpdating)
                    break;
            }
        }

        private void Integrate(float deltaTime)
        {
            var displacement = Subtract(Target, Value);

            if (IsAtRest(_velocity, displacement, _restThreshold))
            {
                Value = Target;
                _velocity = default!;
                StopUpdating();
                return;
            }

            var springForce = Multiply(displacement, _tension);
            var dampingForce = Multiply(_velocity, _friction);
            var acceleration = Divide(Subtract(springForce, dampingForce), _mass);

            _velocity = Add(_velocity, Multiply(acceleration, deltaTime));
            Value = Add(Value, Multiply(_velocity, deltaTime));

            _onUpdate?.Invoke(0f);
        }

        /// <summary>Clears velocity state for pooling.</summary>
        public override void Reset()
        {
            _velocity = default!;
            _initialVelocity = default!;
            _isInitialVelocitySetByUser = false;

            base.Reset();
        }

        /// <summary>Accepts value and target state from a predecessor.</summary>
        public override void UnionWith(Behavior source)
        {
            if (source is not Behavior<T> typedSource)
            {
                return;
            }

            if (!_isValueSetByUser)
            {
                _value = typedSource.Value;
            }

            if (!_isTargetSetByUser)
            {
                _target = typedSource.Value;
            }
        }

        /// <summary>Compares spring parameters and value/target semantics.</summary>
        public override bool SemEquals(Behavior other)
        {
            if (other is not SpringBehavior<T> otherSpring)
                return false;

            if (!base.SemEquals(other))
                return false;

            if (!EqualityComparer<float>.Default.Equals(Tension, otherSpring.Tension))
                return false;

            if (!EqualityComparer<float>.Default.Equals(Friction, otherSpring.Friction))
                return false;

            if (!EqualityComparer<float>.Default.Equals(Mass, otherSpring.Mass))
                return false;

            if (!EqualityComparer<T>.Default.Equals(InitialVelocity, otherSpring.InitialVelocity))
                return false;

            if (!EqualityComparer<float>.Default.Equals(RestThreshold, otherSpring.RestThreshold))
                return false;

            return true;
        }

        /// <summary>Creates an independent clone with the same spring configuration.</summary>
        public override Behavior<T> Clone()
        {
            var clone = new SpringBehavior<T>
            {
                Tension = Tension,
                Friction = Friction,
                Mass = Mass,
                RestThreshold = RestThreshold,
            };
            if (_isValueSetByUser)
            {
                clone.Value = Value;
            }
            if (_isTargetSetByUser)
            {
                clone.Target = Target;
            }
            if (_isInitialVelocitySetByUser)
            {
                clone.InitialVelocity = _initialVelocity;
            }
            return clone;
        }

        private static T Add(T a, T b) =>
            (a, b) switch
            {
                (float fa, float fb) => (T)(object)(fa + fb),
                (int ia, int ib) => (T)(object)(ia + ib),
                (Vector2 va, Vector2 vb) => (T)(object)(va + vb),
                (Color ca, Color cb) => (T)(object)(ca + cb),
                _ => throw new NotImplementedException(
                    $"Add for type {typeof(T)} is not implemented."
                ),
            };

        private static T Subtract(T a, T b) =>
            (a, b) switch
            {
                (float fa, float fb) => (T)(object)(fa - fb),
                (int ia, int ib) => (T)(object)(ia - ib),
                (Vector2 va, Vector2 vb) => (T)(object)(va - vb),
                (Color ca, Color cb) => (T)(object)(ca - cb),
                _ => throw new NotImplementedException(
                    $"Subtract for type {typeof(T)} is not implemented."
                ),
            };

        private static T Multiply(T a, float scalar) =>
            a switch
            {
                float fa => (T)(object)(fa * scalar),
                int ia => (T)(object)Mathf.RoundToInt(ia * scalar),
                Vector2 va => (T)(object)(va * scalar),
                Color ca => (T)(object)(ca * scalar),
                _ => throw new NotImplementedException(
                    $"Multiply for type {typeof(T)} is not implemented."
                ),
            };

        private static T Divide(T a, float scalar)
        {
            if (scalar == 0)
            {
                throw new DivideByZeroException();
            }

            return a switch
            {
                float fa => (T)(object)(fa / scalar),
                int ia => (T)(object)Mathf.RoundToInt(ia / scalar),
                Vector2 va => (T)(object)(va / scalar),
                Color ca => (T)(object)(ca / scalar),
                _ => throw new NotImplementedException(
                    $"Divide for type {typeof(T)} is not implemented."
                ),
            };
        }

        private static float Magnitude(T value) =>
            value switch
            {
                float f => Mathf.Abs(f),
                int i => Mathf.Abs(i),
                Vector2 v => v.magnitude,
                Color c => ((Vector4)c).magnitude,
                _ => throw new NotImplementedException(
                    $"Magnitude for type {typeof(T)} is not implemented."
                ),
            };

        private static bool IsAtRest(T velocity, T displacement, float threshold)
        {
            float velocityMagnitude = Magnitude(velocity);
            float displacementMagnitude = Magnitude(displacement);
            return velocityMagnitude < threshold && displacementMagnitude < threshold;
        }
    }
}
