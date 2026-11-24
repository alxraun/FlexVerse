// parents: @.frostline/graph/m-computed-behavior.xml

using System;
using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Derives a value lazily and memoized from behavior dependencies. (M-COMPUTED-BEHAVIOR)</summary>
    public class ComputedBehavior<TResult> : Behavior<TResult>
    {
        private readonly Func<TResult> _computer;
        private readonly Behavior[] _dependencies;
        private TResult _assignedValue = default!;

        private bool _isDirty = true;
        private bool _isComputing;
        private bool _isNotifying;
        private bool _pendingNotification;

        /// <summary>Lists the borrowed dependency behaviors. (REQ-006)</summary>
        public IReadOnlyList<Behavior> Dependencies { get; }

        /// <summary>Builds a computed behavior from a pure computation and dependencies. (REQ-002)</summary>
        public ComputedBehavior(Func<TResult> computer, params Behavior[] dependencies)
        {
            _computer = computer ?? throw new ArgumentNullException(nameof(computer));
            if (dependencies == null)
                throw new ArgumentNullException(nameof(dependencies));

            var unique = new HashSet<Behavior>();
            var selected = new List<Behavior>(dependencies.Length);
            foreach (var dependency in dependencies)
            {
                if (dependency == null)
                    throw new ArgumentException(
                        "A dependency cannot be null.",
                        nameof(dependencies)
                    );

                if (unique.Add(dependency))
                    selected.Add(dependency);
            }

            _dependencies = selected.ToArray();
            Dependencies = Array.AsReadOnly(_dependencies);
        }

        /// <summary>Returns the cached derived value or a stored override. (REQ-003, REQ-004)</summary>
        public override TResult Value
        {
            get
            {
                if (_isDirty)
                {
                    if (_isComputing)
                        throw new InvalidOperationException("Cyclic computed dependency detected.");

                    _isComputing = true;
                    try
                    {
                        TResult result = _computer();
                        _value = result;
                        _isDirty = false;
                    }
                    finally
                    {
                        _isComputing = false;
                    }
                }

                return _value;
            }
            set
            {
                _isValueSetByUser = true;
                _assignedValue = value;
                _value = value;
                _isDirty = false;
            }
        }

        /// <summary>Stores passive target state independent of computation. (REQ-005)</summary>
        public override TResult Target
        {
            get => _target;
            set
            {
                _isTargetSetByUser = true;
                _target = value;
            }
        }

        /// <summary>Activates idempotently and subscribes to dependency updates. (REQ-006, REQ-007)</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            _isDirty = true;
            State = Lifecycle.Active;

            foreach (var dependency in _dependencies)
                dependency.OnUpdate += HandleDependencyUpdate;

            _onActivate?.Invoke();
        }

        /// <summary>Advances the computed behavior with no per-tick work. (REQ-011)</summary>
        public override void Update(float deltaTime) { }

        /// <summary>Transfers typed state while keeping the cache dirty. (REQ-012)</summary>
        public override void UnionWith(Behavior source)
        {
            if (source is not Behavior<TResult> typedSource)
                return;

            if (!_isValueSetByUser)
                _value = typedSource.Value;

            if (!_isTargetSetByUser)
                _target = typedSource.Target;

            _isDirty = true;
        }

        /// <summary>Compares computation and dependency identities. (REQ-013)</summary>
        public override bool SemEquals(Behavior other)
        {
            if (ReferenceEquals(this, other))
                return true;

            if (other is not ComputedBehavior<TResult> computed)
                return false;

            if (!Equals(_computer, computed._computer))
                return false;

            if (_dependencies.Length != computed._dependencies.Length)
                return false;

            for (int i = 0; i < _dependencies.Length; i++)
            {
                if (!ReferenceEquals(_dependencies[i], computed._dependencies[i]))
                    return false;
            }

            if (
                _isValueSetByUser != computed._isValueSetByUser
                || _isTargetSetByUser != computed._isTargetSetByUser
            )
                return false;

            if (
                _isTargetSetByUser
                && !EqualityComparer<TResult>.Default.Equals(_target, computed._target)
            )
                return false;

            if (
                _isValueSetByUser
                && !EqualityComparer<TResult>.Default.Equals(
                    _assignedValue,
                    computed._assignedValue
                )
            )
                return false;

            return true;
        }

        /// <summary>Creates a separate computed instance with a dirty cache. (REQ-014)</summary>
        public override Behavior<TResult> Clone()
        {
            var clone = new ComputedBehavior<TResult>(_computer, _dependencies);

            if (_isValueSetByUser)
            {
                clone._isValueSetByUser = true;
                clone._assignedValue = _assignedValue;
                clone._value = _assignedValue;
            }

            if (_isTargetSetByUser)
                clone.Target = _target;

            return clone;
        }

        /// <summary>Releases subscriptions and runtime state. (REQ-014)</summary>
        public override void Reset()
        {
            foreach (var dependency in _dependencies)
                dependency.OnUpdate -= HandleDependencyUpdate;

            _isDirty = true;
            _assignedValue = default!;
            _isComputing = false;
            _pendingNotification = false;

            base.Reset();
        }

        private void HandleDependencyUpdate(float progress)
        {
            if (State != Lifecycle.Active || _isDirty)
                return;

            _isDirty = true;
            _pendingNotification = true;

            if (_isNotifying)
                return;

            _isNotifying = true;
            try
            {
                while (_pendingNotification && State == Lifecycle.Active)
                {
                    _pendingNotification = false;

                    try
                    {
                        StartUpdating();
                        _onUpdate?.Invoke(1f);
                    }
                    finally
                    {
                        StopUpdating();
                    }
                }
            }
            finally
            {
                _isNotifying = false;
            }
        }
    }
}
