// parents: @.frostline/graph/m-tween-behavior.xml

using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Interpolates a value toward a target over a duration through an easing curve. (M-TWEEN-BEHAVIOR)</summary>
    public class TweenBehavior<T> : Behavior<T>
    {
        private float _duration = 1f;
        private float _minDuration = 1f;
        private float _maxDuration = 1f;

        private EasingFunction _easing = FlexVerse.Easing.Linear;

        private float _progress;
        private T _initialValue = default!;

        private float _effectiveDuration;
        private float _calibratedSpeed;
        private bool _isCalibrated;

        private bool _isMinDurationSetByUser;
        private bool _isMaxDurationSetByUser;

        /// <summary>Gets or sets the base duration of the tween in seconds.</summary>
        public float Duration
        {
            get => _duration;
            set
            {
                _duration = value;
                if (!_isMinDurationSetByUser)
                    MinDuration = value;
                if (!_isMaxDurationSetByUser)
                    MaxDuration = value;

                if (State == Lifecycle.Active)
                {
                    HandleNewMovement();
                }
            }
        }

        /// <summary>Gets or sets the lower clamp for the effective duration.</summary>
        public float MinDuration
        {
            get => _minDuration;
            set
            {
                _isMinDurationSetByUser = true;
                _minDuration = value;
            }
        }

        /// <summary>Gets or sets the upper clamp for the effective duration.</summary>
        public float MaxDuration
        {
            get => _maxDuration;
            set
            {
                _isMaxDurationSetByUser = true;
                _maxDuration = value;
            }
        }

        /// <summary>Gets or sets the easing curve applied to progress.</summary>
        public EasingFunction Easing
        {
            get => _easing;
            set { _easing = value ?? FlexVerse.Easing.Linear; }
        }

        /// <summary>Starts a new tween from the current value toward the assigned target. (REQ-001)</summary>
        public override T Target
        {
            get => _target;
            set
            {
                _isTargetSetByUser = true;

                if (EqualityComparer<T>.Default.Equals(_target, value))
                    return;

                _target = value;

                Log.Debug(
                    "M-TWEEN-BEHAVIOR",
                    "target.assigned",
                    ("behavior", this),
                    ("target", _target)
                );

                if (State == Lifecycle.Active)
                {
                    HandleNewMovement();
                }
            }
        }

        /// <summary>Starts a new tween from the assigned value toward the current target.</summary>
        public override T Value
        {
            get => _value;
            set
            {
                _isValueSetByUser = true;
                _value = value;

                if (State == Lifecycle.Active)
                {
                    HandleNewMovement();
                }
            }
        }

        /// <summary>Activates the tween and begins movement toward the target.</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;
            _onActivate?.Invoke();

            HandleNewMovement();
        }

        /// <summary>Advances the eased interpolation and reports progress each tick. (REQ-002)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating)
                return;

            _progress += _effectiveDuration > 0f ? deltaTime / _effectiveDuration : 1f;
            _progress = Mathf.Clamp01(_progress);

            var progress = _easing(_progress);

            _value = TypeUtils.Interpolate(_initialValue, Target, progress);

            _onUpdate?.Invoke(progress);

            if (_progress >= 1f)
            {
                StopUpdating();
            }
        }

        /// <summary>Restores default duration, easing and calibration state for pooling.</summary>
        public override void Reset()
        {
            _duration = 1f;
            _minDuration = 1f;
            _maxDuration = 1f;
            _easing = FlexVerse.Easing.Linear;

            _progress = 0f;
            _initialValue = default;

            _calibratedSpeed = 0f;
            _isCalibrated = false;

            _isMinDurationSetByUser = false;
            _isMaxDurationSetByUser = false;

            base.Reset();
        }

        /// <summary>Accepts value, target and calibration state from a predecessor.</summary>
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
                _target = typedSource.Target;
            }

            if (source is TweenBehavior<T> other)
            {
                if (!_isCalibrated && other._isCalibrated)
                {
                    _calibratedSpeed = other._calibratedSpeed;
                    _isCalibrated = true;
                }
            }
        }

        /// <summary>Compares easing, durations and value/target semantics.</summary>
        public override bool SemEquals(Behavior other)
        {
            if (other is not TweenBehavior<T> otherTween)
                return false;

            if (!base.SemEquals(other))
                return false;

            if (!EqualityComparer<EasingFunction>.Default.Equals(Easing, otherTween.Easing))
                return false;

            if (Duration != otherTween.Duration)
                return false;
            if (MinDuration != otherTween.MinDuration)
                return false;
            if (MaxDuration != otherTween.MaxDuration)
                return false;

            return true;
        }

        /// <summary>Creates an independent clone with the same configuration.</summary>
        public override Behavior<T> Clone()
        {
            var clone = new TweenBehavior<T>
            {
                Duration = Duration,
                MinDuration = MinDuration,
                MaxDuration = MaxDuration,
                Easing = Easing,
            };
            if (_isValueSetByUser)
            {
                clone.Value = Value;
            }
            if (_isTargetSetByUser)
            {
                clone.Target = Target;
            }
            return clone;
        }

        private void HandleNewMovement()
        {
            _initialValue = _value;
            _progress = 0f;

            if (EqualityComparer<T>.Default.Equals(_initialValue, _target))
            {
                _progress = 1f;
                _value = _target;
                _onUpdate?.Invoke(1f);
                if (IsUpdating)
                    StopUpdating();
                return;
            }

            if (!IsUpdating)
            {
                _isCalibrated = false;
            }

            SetEffectiveDurationByTimeConstraints();

            StartUpdating();
        }

        private void SetEffectiveDurationByTimeConstraints()
        {
            var distance = TypeUtils.GetDistance(_initialValue, _target);

            var baseDuration = Mathf.Max(0f, _duration);
            var minDurationValue = Mathf.Max(0f, _minDuration);
            var maxDurationValue = Mathf.Max(minDurationValue, _maxDuration);

            if (!_isCalibrated)
            {
                _calibratedSpeed = baseDuration > 0f ? distance / baseDuration : 0f;
                _isCalibrated = true;
            }

            if (_calibratedSpeed > 0f)
                _effectiveDuration = distance / _calibratedSpeed;
            else if (distance == 0f)
                _effectiveDuration = 0f;
            else
                _effectiveDuration = baseDuration;

            _effectiveDuration = Mathf.Clamp(
                _effectiveDuration,
                minDurationValue,
                maxDurationValue
            );
        }

        protected override void StopUpdating()
        {
            base.StopUpdating();
            _isCalibrated = false;
            _calibratedSpeed = 0f;
        }
    }
}
