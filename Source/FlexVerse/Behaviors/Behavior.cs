// parents: @.frostline/graph/m-behavior.xml

using System;
using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Defines the abstract base contract shared by all behaviors.</summary>
    public abstract class Behavior
    {
        /// <summary>Describes the lifecycle position of a behavior.</summary>
        public enum Lifecycle
        {
            Inactive,
            Active,
        }

        /// <summary>Reports the current lifecycle state of the behavior.</summary>
        public Lifecycle State { get; protected set; } = Lifecycle.Inactive;

        protected Action _onActivate;

        /// <summary>Raises callbacks when the behavior activates.</summary>
        public event Action OnActivate
        {
            add { _onActivate += value; }
            remove { _onActivate -= value; }
        }

        protected Action _onStart;

        /// <summary>Raises callbacks when the behavior starts updating.</summary>
        public event Action OnStart
        {
            add { _onStart += value; }
            remove { _onStart -= value; }
        }

        protected Action _onComplete;

        /// <summary>Raises callbacks when an active updating cycle ends.</summary>
        public event Action OnComplete
        {
            add { _onComplete += value; }
            remove { _onComplete -= value; }
        }

        protected Action<float> _onUpdate;

        /// <summary>Raises callbacks on each behavior update tick.</summary>
        public event Action<float> OnUpdate
        {
            add { _onUpdate += value; }
            remove { _onUpdate -= value; }
        }

        protected Action _onReset;

        /// <summary>Raises callbacks when the behavior resets.</summary>
        public event Action OnReset
        {
            add { _onReset += value; }
            remove { _onReset -= value; }
        }

        /// <summary>Indicates whether the behavior is currently updating.</summary>
        public bool IsUpdating { get; private set; }

        /// <summary>Activates the behavior.</summary>
        public abstract void Activate();

        /// <summary>Advances the behavior by one update tick.</summary>
        public abstract void Update(float deltaTime);

        /// <summary>Accepts applicable state from a predecessor behavior.</summary>
        public abstract void UnionWith(Behavior source);

        /// <summary>Compares semantic content against another behavior.</summary>
        public abstract bool SemEquals(Behavior other);

        protected virtual void StartUpdating()
        {
            if (!IsUpdating)
            {
                IsUpdating = true;
                _onStart?.Invoke();
            }
        }

        protected virtual void StopUpdating()
        {
            if (IsUpdating)
            {
                IsUpdating = false;
                _onComplete?.Invoke();
            }
        }

        /// <summary>Returns the behavior to its pooled, inactive state.</summary>
        public virtual void Reset()
        {
            StopUpdating();
            State = Lifecycle.Inactive;
            _onReset?.Invoke();

            _onActivate = null;
            _onStart = null;
            _onComplete = null;
            _onUpdate = null;
            _onReset = null;
        }

        /// <summary>Creates an untyped copy of the behavior.</summary>
        public abstract Behavior CloneUntyped();

        /// <summary>Returns a short debug identifier for the behavior.</summary>
        public override string ToString()
        {
            return $"{GetType().Name}:{GetHashCode() & 0xFFFFF:X5}";
        }
    }

    /// <summary>Provides the typed value/target contract for concrete behaviors. (M-BEHAVIOR)</summary>
    public abstract class Behavior<T> : Behavior
    {
        protected T _value = default!;
        protected T _target = default!;

        protected bool _isValueSetByUser;
        protected bool _isTargetSetByUser;

        /// <summary>Gets or sets the current value of the behavior.</summary>
        public virtual T Value
        {
            get => _value;
            set
            {
                _isValueSetByUser = true;
                _value = value;
            }
        }

        /// <summary>Gets or sets the desired target state of the behavior.</summary>
        public virtual T Target
        {
            get => _target;
            set
            {
                _isTargetSetByUser = true;

                if (EqualityComparer<T>.Default.Equals(_target, value))
                    return;

                _target = value;

                Log.Debug("M-BEHAVIOR", "target.assigned", ("behavior", this), ("target", _target));

                if (State == Lifecycle.Active)
                {
                    StartUpdating();
                }
            }
        }

        /// <summary>Returns the typed behavior to its pooled, inactive state.</summary>
        public override void Reset()
        {
            _isValueSetByUser = false;
            _isTargetSetByUser = false;

            _value = default;
            _target = default;

            base.Reset();
        }

        /// <summary>Creates a typed copy of the behavior.</summary>
        public abstract Behavior<T> Clone();

        /// <summary>Creates an untyped copy of the behavior.</summary>
        public sealed override Behavior CloneUntyped()
        {
            return Clone();
        }

        /// <summary>Compares semantic content against another behavior.</summary>
        public override bool SemEquals(Behavior other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is not Behavior<T> otherTyped)
                return false;

            if (_isTargetSetByUser != otherTyped._isTargetSetByUser)
                return false;

            if (
                _isTargetSetByUser && !EqualityComparer<T>.Default.Equals(Target, otherTyped.Target)
            )
                return false;

            if (_isValueSetByUser != otherTyped._isValueSetByUser)
                return false;

            if (_isValueSetByUser && !EqualityComparer<T>.Default.Equals(Value, otherTyped.Value))
                return false;

            return true;
        }

        /// <summary>Lifts a plain value into an immediate static behavior. (REQ-001, REQ-004)</summary>
        public static implicit operator Behavior<T>(T value)
        {
            return new StaticBehavior<T> { Target = value };
        }
    }
}
