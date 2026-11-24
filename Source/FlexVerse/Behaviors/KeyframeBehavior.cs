// parents: @.frostline/graph/m-keyframe-behavior.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Defines one keyed time/value stop with an optional easing. (M-KEYFRAME-BEHAVIOR)</summary>
    public struct Keyframe<T> : IEquatable<Keyframe<T>>
    {
        /// <summary>Gets or sets the normalized time of the keyframe.</summary>
        public float Time { get; set; }

        /// <summary>Gets or sets the value reached at this keyframe.</summary>
        public T Value { get; set; }

        /// <summary>Gets or sets the easing applied inside the outgoing segment.</summary>
        public EasingFunction Easing { get; set; }

        /// <summary>Compares time, value and easing for equality.</summary>
        public readonly bool Equals(Keyframe<T> other)
        {
            return Time.Equals(other.Time)
                && EqualityComparer<T>.Default.Equals(Value, other.Value)
                && EqualityComparer<EasingFunction>.Default.Equals(Easing, other.Easing);
        }

        /// <summary>Compares against another object for equality.</summary>
        public override readonly bool Equals(object obj)
        {
            return obj is Keyframe<T> other && Equals(other);
        }

        /// <summary>Hashes time, value and easing.</summary>
        public override readonly int GetHashCode()
        {
            return HashCode.Combine(Time, Value, Easing);
        }

        /// <summary>Reports equality of two keyframes.</summary>
        public static bool operator ==(Keyframe<T> left, Keyframe<T> right)
        {
            return left.Equals(right);
        }

        /// <summary>Reports inequality of two keyframes.</summary>
        public static bool operator !=(Keyframe<T> left, Keyframe<T> right)
        {
            return !left.Equals(right);
        }
    }

    /// <summary>Interpolates piecewise-linearly over an ordered keyframe list. (M-KEYFRAME-BEHAVIOR)</summary>
    public class KeyframeBehavior<T> : Behavior<T>
    {
        private float _duration = 1f;
        private List<Keyframe<T>> _keyframes = [];
        private float _currentTime;
        private int _currentSegmentIndex;
        private T _initialValue = default!;

        /// <summary>Gets or sets the total playback duration in seconds.</summary>
        public float Duration
        {
            get => _duration;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change Duration while behavior is active."
                    );
                }
                _duration = Mathf.Max(0f, value);
            }
        }

        /// <summary>Gets or sets the ordered keyframe list.</summary>
        public IReadOnlyList<Keyframe<T>> Keyframes
        {
            get => _keyframes;
            set
            {
                if (State == Lifecycle.Active)
                {
                    throw new InvalidOperationException(
                        "Cannot change Keyframes while behavior is active."
                    );
                }
                _keyframes = value == null ? [] : [.. value];
                _keyframes.Sort((a, b) => a.Time.CompareTo(b.Time));
                if (_isTargetSetByUser)
                    SetLastTarget(_target);
            }
        }

        /// <summary>Replaces the last keyframe value with the assigned target. (REQ-004)</summary>
        public override T Target
        {
            get => _keyframes.Count > 0 ? _keyframes[^1].Value : _target;
            set
            {
                bool changed = !EqualityComparer<T>.Default.Equals(Target, value);
                _isTargetSetByUser = true;
                _target = value;
                SetLastTarget(value);

                if (changed && State == Lifecycle.Active && _keyframes.Count > 0)
                    StartUpdating();
            }
        }

        private void SetLastTarget(T value)
        {
            if (_keyframes.Count == 0)
                return;

            var last = _keyframes[^1];
            last.Value = value;
            _keyframes[^1] = last;
        }

        /// <summary>Activates playback from the current value.</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;
            _currentTime = 0f;
            _currentSegmentIndex = 0;
            _initialValue = Value;

            _onActivate?.Invoke();
            StartUpdating();
        }

        /// <summary>Evaluates the segment at the current time and reports progress. (REQ-001, REQ-002)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating)
                return;

            _currentTime += deltaTime;
            float normalized = Duration > 0f ? Mathf.Clamp01(_currentTime / Duration) : 1f;

            if (_currentTime >= Duration)
            {
                if (_keyframes.Count > 0)
                {
                    Value = _keyframes[^1].Value;
                }
                StopUpdating();
                return;
            }

            if (_keyframes.Count == 0)
            {
                _onUpdate?.Invoke(normalized);
                return;
            }

            while (
                _currentSegmentIndex < _keyframes.Count
                && _currentTime > _keyframes[_currentSegmentIndex].Time
            )
            {
                _currentSegmentIndex++;
            }

            if (_currentSegmentIndex == 0)
            {
                float endTime = _keyframes[0].Time;
                if (endTime <= 0f)
                {
                    Value = _keyframes[0].Value;
                }
                else
                {
                    float localProgress = Mathf.Clamp01(_currentTime / endTime);
                    float eased = localProgress;
                    Value = TypeUtils.Interpolate(_initialValue, _keyframes[0].Value, eased);
                }
            }
            else if (_currentSegmentIndex < _keyframes.Count)
            {
                var start = _keyframes[_currentSegmentIndex - 1];
                var end = _keyframes[_currentSegmentIndex];
                float segmentDuration = Mathf.Max(0.000001f, end.Time - start.Time);
                float localProgress = Mathf.Clamp01((_currentTime - start.Time) / segmentDuration);
                float eased = (end.Easing ?? Easing.Linear)(localProgress);
                Value = TypeUtils.Interpolate(start.Value, end.Value, eased);
            }
            else
            {
                Value = _keyframes[^1].Value;
            }

            _onUpdate?.Invoke(normalized);
        }

        /// <summary>Clears playback position state for pooling.</summary>
        public override void Reset()
        {
            _currentTime = 0f;
            _currentSegmentIndex = 0;
            _initialValue = default!;

            base.Reset();
        }

        /// <summary>Accepts the value from a predecessor behavior.</summary>
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
        }

        /// <summary>Compares duration and keyframe semantics.</summary>
        public override bool SemEquals(Behavior other)
        {
            if (other is not KeyframeBehavior<T> otherKeyframe)
            {
                return false;
            }

            if (!base.SemEquals(other))
                return false;

            if (!EqualityComparer<float>.Default.Equals(Duration, otherKeyframe.Duration))
                return false;

            if (!Keyframes.SequenceEqual(otherKeyframe.Keyframes))
                return false;

            return true;
        }

        /// <summary>Creates an independent clone preserving target intent. (REQ-005)</summary>
        public override Behavior<T> Clone()
        {
            var clone = new KeyframeBehavior<T> { Duration = Duration };
            clone.Keyframes = _keyframes;
            if (_isTargetSetByUser)
                clone.Target = _target;
            if (_isValueSetByUser)
            {
                clone.Value = Value;
            }
            return clone;
        }
    }
}
