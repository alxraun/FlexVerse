// parents: @.frostline/graph/m-static-behavior.xml

using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Assigns a target value instantly or after a delay window. (M-STATIC-BEHAVIOR)</summary>
    public class StaticBehavior<T> : Behavior<T>
    {
        private float _duration;
        private float _elapsedTime;

        /// <summary>Gets or sets the delay window before the target assignment completes.</summary>
        public float Duration
        {
            get => _duration;
            set
            {
                _duration = value;

                if (State != Lifecycle.Active)
                    return;

                if (_duration > 0f)
                {
                    _elapsedTime = 0f;
                    StartUpdating();
                }
                else
                {
                    StopUpdating();
                }
            }
        }

        /// <summary>Captures the assigned target as the behavior value. (REQ-001)</summary>
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
                    "M-STATIC-BEHAVIOR",
                    "target.assigned",
                    ("behavior", this),
                    ("target", _target)
                );

                if (State == Lifecycle.Active)
                {
                    _value = _target;

                    if (_duration <= 0f)
                    {
                        _onUpdate?.Invoke(1f);
                    }
                }
            }
        }

        /// <summary>Assigns the target and starts the delay window once. (REQ-001)</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;

            _value = _target;
            _elapsedTime = 0f;

            _onActivate?.Invoke();

            if (_duration > 0f)
            {
                StartUpdating();
            }
        }

        /// <summary>Reports delay progress and completes after the duration elapses. (REQ-002)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating)
                return;

            _elapsedTime += deltaTime;
            var progress = _duration > 0 ? _elapsedTime / _duration : 1f;
            _onUpdate?.Invoke(progress);

            if (_elapsedTime >= _duration)
            {
                StopUpdating();
            }
        }

        /// <summary>Clears the duration and elapsed state for pooling. (REQ-004)</summary>
        public override void Reset()
        {
            _duration = 0;
            _elapsedTime = 0;

            base.Reset();
        }

        /// <summary>Accepts the target from a predecessor behavior.</summary>
        public override void UnionWith(Behavior source)
        {
            if (source is not Behavior<T> typedSource)
                return;

            if (!_isTargetSetByUser)
            {
                Target = typedSource.Target;
            }
        }

        /// <summary>Compares target and duration semantics. (REQ-005)</summary>
        public override bool SemEquals(Behavior other)
        {
            if (other is not StaticBehavior<T> otherStatic)
                return false;

            if (!base.SemEquals(otherStatic))
                return false;

            if (_duration != otherStatic._duration)
                return false;

            return true;
        }

        /// <summary>Creates an independent equal instance. (REQ-005)</summary>
        public override Behavior<T> Clone()
        {
            var clone = new StaticBehavior<T> { Duration = Duration };

            if (_isTargetSetByUser)
                clone.Target = Target;

            return clone;
        }
    }
}
