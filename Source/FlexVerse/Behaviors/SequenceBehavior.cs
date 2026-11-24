// parents: @.frostline/graph/m-sequence-behavior.xml

using System;
using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Defines elastic and restart parameters for sequence retargeting. (M-SEQUENCE-BEHAVIOR)</summary>
    public struct SequenceStrategy
    {
        /// <summary>Blends rigid and rubber-band target redistribution.</summary>
        public float Elasticity;

        /// <summary>Picks the pivot point for rigid redistribution.</summary>
        public float Anchor;

        /// <summary>Selects replay-from-first-child on retarget.</summary>
        public bool Restart;

        /// <summary>Distributes retargeting proportionally across the chain.</summary>
        public static readonly SequenceStrategy Proportional = new()
        {
            Elasticity = 1f,
            Anchor = 0f,
            Restart = true,
        };

        /// <summary>Shifts intermediate targets by the full retarget delta.</summary>
        public static readonly SequenceStrategy FixedOffset = new()
        {
            Elasticity = 0f,
            Anchor = 1f,
            Restart = true,
        };

        /// <summary>Keeps intermediate targets fixed and retargets the last child only.</summary>
        public static readonly SequenceStrategy LastOnly = new()
        {
            Elasticity = 0f,
            Anchor = 0f,
            Restart = true,
        };
    }

    /// <summary>Executes child behaviors in order and adapts their targets under a strategy. (M-SEQUENCE-BEHAVIOR)</summary>
    public class SequenceBehavior<T> : Behavior<T>
    {
        private readonly List<Behavior> _children;
        private SequenceStrategy _strategy = SequenceStrategy.Proportional;

        private int _currentIndex = -1;
        private float _currentChildProgress = 0f;

        private readonly SequenceGeometry _geometry = new();

        private Action<float> _childOnUpdateHandler;
        private Action _childOnCompleteHandler;

        /// <summary>Builds a sequence from the ordered child behaviors.</summary>
        public SequenceBehavior(params Behavior[] behaviors)
        {
            _children = [];
            if (behaviors != null)
            {
                foreach (var d in behaviors)
                {
                    if (d != null)
                        _children.Add(d);
                }
            }
            InitializeHandlers();
        }

        private SequenceBehavior(List<Behavior> children, SequenceStrategy strategy)
        {
            _children = children;
            _strategy = strategy;
            InitializeHandlers();
        }

        /// <summary>Gets or sets the retargeting strategy of the sequence.</summary>
        public SequenceStrategy Strategy
        {
            get => _strategy;
            set => _strategy = value;
        }

        /// <summary>Lists the ordered child behaviors.</summary>
        public IReadOnlyList<Behavior> Children => _children;

        /// <summary>Sets the child behaviors of the sequence.</summary>
        public IEnumerable<Behavior> Behaviors
        {
            set => SetChildren(value);
        }

        /// <summary>Resolves the composite target from the last typed child.</summary>
        public override T Target
        {
            get
            {
                if (_isTargetSetByUser)
                    return _target;
                var lastGeneric = GetLastGenericChild();
                return lastGeneric is not null ? lastGeneric.Target : _target;
            }
            set
            {
                if (EqualityComparer<T>.Default.Equals(Target, value))
                    return;

                _isTargetSetByUser = true;
                _target = value;

                Log.Debug(
                    "M-SEQUENCE-BEHAVIOR",
                    "target.assigned",
                    ("behavior", this),
                    ("target", _target)
                );

                if (State == Lifecycle.Active)
                {
                    ApplyStrategy(value);

                    if (CurrentChild?.IsUpdating == true)
                        StartUpdating();
                }
            }
        }

        /// <summary>Forwards the assigned value to the current typed child.</summary>
        public override T Value
        {
            get => _value;
            set
            {
                _isValueSetByUser = true;
                _value = value;

                if (GetCurrentGenericChild() is { } current)
                {
                    current.Value = value;
                }
            }
        }

        private Behavior CurrentChild =>
            _currentIndex >= 0 && _currentIndex < _children.Count ? _children[_currentIndex] : null;

        private void InitializeHandlers()
        {
            _childOnUpdateHandler = progress =>
            {
                _currentChildProgress = progress;

                if (CurrentChild is Behavior<T> typedChild)
                {
                    _value = typedChild.Value;
                }
            };
            _childOnCompleteHandler = ActivateNextChild;
        }

        private void SetChildren(IEnumerable<Behavior> newChildren)
        {
            var newList = new List<Behavior>();
            if (newChildren != null)
            {
                foreach (var d in newChildren)
                {
                    if (d != null)
                        newList.Add(d);
                }
            }

            if (AreChildrenIdentical(newList))
                return;

            var previousChild = CurrentChild;
            UnsubscribeFromChild(previousChild);

            int newIndex = previousChild != null ? newList.IndexOf(previousChild) : -1;

            _children.Clear();
            _children.AddRange(newList);

            _geometry.Invalidate();

            if (State != Lifecycle.Active)
            {
                _currentIndex = -1;
                return;
            }

            if (newIndex >= 0)
            {
                _currentIndex = newIndex;
                SubscribeToChild(CurrentChild);
            }
            else
            {
                _currentIndex = Math.Max(0, Math.Min(_currentIndex, _children.Count)) - 1;
                ActivateNextChild();
            }
        }

        private bool AreChildrenIdentical(List<Behavior> newList)
        {
            if (_children.Count != newList.Count)
                return false;

            for (int i = 0; i < _children.Count; i++)
            {
                if (!ReferenceEquals(_children[i], newList[i]))
                    return false;
            }

            return true;
        }

        /// <summary>Activates the chain starting from the first child.</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;

            _geometry.Ensure(_value, _children);

            if (_isTargetSetByUser)
            {
                ApplyStrategy(_target);
            }

            _onActivate?.Invoke();
            StartUpdating();

            _currentIndex = -1;
            ActivateNextChild();
        }

        /// <summary>Drives the current child and reports aggregated progress. (REQ-001)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating)
                return;

            var child = CurrentChild;
            if (child is null)
            {
                StopUpdating();
                return;
            }

            child.Update(deltaTime);

            _onUpdate?.Invoke(GetAggregatedProgress());
        }

        /// <summary>Resets the chain and all children for pooling.</summary>
        public override void Reset()
        {
            UnsubscribeFromChild(CurrentChild);

            foreach (var child in _children)
            {
                child.Reset();
            }

            _currentIndex = -1;
            _currentChildProgress = 0f;
            _geometry.Invalidate();

            base.Reset();
        }

        /// <summary>Hands the predecessor state to the first typed child.</summary>
        public override void UnionWith(Behavior source)
        {
            if (source is not Behavior<T> typedSource)
                return;

            if (GetFirstGenericChild() is { } firstGeneric)
            {
                firstGeneric.UnionWith(typedSource);

                if (!_isValueSetByUser)
                {
                    _value = typedSource.Value;
                }
            }
        }

        /// <summary>Compares strategy and child semantics.</summary>
        public override bool SemEquals(Behavior other)
        {
            if (other is not SequenceBehavior<T> otherSequence)
                return false;

            if (_children.Count != otherSequence._children.Count)
                return false;

            if (
                !EqualityComparer<float>.Default.Equals(
                    _strategy.Elasticity,
                    otherSequence._strategy.Elasticity
                )
            )
                return false;
            if (
                !EqualityComparer<float>.Default.Equals(
                    _strategy.Anchor,
                    otherSequence._strategy.Anchor
                )
            )
                return false;
            if (_strategy.Restart != otherSequence._strategy.Restart)
                return false;

            for (int i = 0; i < _children.Count; i++)
            {
                if (!_children[i].SemEquals(otherSequence._children[i]))
                    return false;
            }

            return base.SemEquals(otherSequence);
        }

        /// <summary>Creates an independent clone of the chain and its children.</summary>
        public override Behavior<T> Clone()
        {
            var clonedChildren = new List<Behavior>(_children.Count);
            foreach (var child in _children)
            {
                clonedChildren.Add(child.CloneUntyped());
            }

            var clone = new SequenceBehavior<T>(clonedChildren, _strategy);

            if (_isValueSetByUser)
                clone.Value = Value;
            if (_isTargetSetByUser)
                clone.Target = Target;

            return clone;
        }

        private void ActivateNextChild()
        {
            UnsubscribeFromChild(CurrentChild);

            while (++_currentIndex < _children.Count)
            {
                _currentChildProgress = 0f;
                var current = _children[_currentIndex];

                if (current is Behavior<T> currentGeneric)
                {
                    currentGeneric.Value = _value;

                    if (!ReferenceEquals(CurrentChild, current))
                        return;

                    if (_geometry.TryGetResult(CountGenericBefore(_currentIndex), out var target))
                    {
                        currentGeneric.Target = target;

                        if (!ReferenceEquals(CurrentChild, current))
                            return;
                    }
                }

                SubscribeToChild(current);
                current.Activate();

                if (!ReferenceEquals(CurrentChild, current))
                    return;

                if (current.IsUpdating)
                    return;

                UnsubscribeFromChild(current);
            }

            _currentChildProgress = 0f;
            StopUpdating();
        }

        private int CountGenericBefore(int exclusiveEnd)
        {
            int count = 0;
            for (int i = 0; i < exclusiveEnd; i++)
            {
                if (_children[i] is Behavior<T>)
                    count++;
            }
            return count;
        }

        private void SubscribeToChild(Behavior child)
        {
            if (child == null)
                return;
            child.OnUpdate += _childOnUpdateHandler;
            child.OnComplete += _childOnCompleteHandler;
        }

        private void UnsubscribeFromChild(Behavior child)
        {
            if (child == null)
                return;
            child.OnUpdate -= _childOnUpdateHandler;
            child.OnComplete -= _childOnCompleteHandler;
        }

        private Behavior<T> GetFirstGenericChild()
        {
            foreach (var child in _children)
                if (child is Behavior<T> t)
                    return t;
            return null;
        }

        private Behavior<T> GetCurrentGenericChild()
        {
            return CurrentChild as Behavior<T>;
        }

        private Behavior<T> GetLastGenericChild()
        {
            for (int i = _children.Count - 1; i >= 0; i--)
                if (_children[i] is Behavior<T> t)
                    return t;
            return null;
        }

        private float GetAggregatedProgress()
        {
            int count = _children.Count;
            if (count <= 0 || _currentIndex >= count)
                return 1f;
            float completed = Math.Max(0, _currentIndex);
            float current = Math.Max(0f, Math.Min(1f, _currentChildProgress));
            return (completed + current) / count;
        }

        private void ApplyStrategy(T newGlobalTarget)
        {
            _geometry.Ensure(_value, _children);

            if (!_geometry.HasTargets)
                return;

            _geometry.Recalculate(_value, newGlobalTarget, _strategy);

            if (_strategy.Restart)
            {
                RestartSequence();
            }
            else if (_currentIndex >= _children.Count)
            {
                for (int i = _children.Count - 1; i >= 0; i--)
                {
                    if (_children[i] is not Behavior<T>)
                        continue;

                    _currentIndex = i - 1;
                    ActivateNextChild();
                    break;
                }
            }

            int genericIndex = 0;
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i] is not Behavior<T> child)
                    continue;

                if (i == _currentIndex)
                {
                    if (_geometry.TryGetResult(genericIndex, out var newTarget))
                    {
                        child.Target = newTarget;
                    }
                }
                genericIndex++;
            }
        }

        private void RestartSequence()
        {
            UnsubscribeFromChild(CurrentChild);

            _currentIndex = -1;
            _currentChildProgress = 0f;

            ActivateNextChild();
        }

        private sealed class SequenceGeometry
        {
            private bool _isValid;
            private T _origin = default!;
            private readonly List<T> _originals = [];
            private readonly List<T> _results = [];

            public bool HasTargets => _originals.Count > 0;

            public void Invalidate()
            {
                _isValid = false;
                _originals.Clear();
                _results.Clear();
            }

            public void Ensure(T currentOrigin, List<Behavior> children)
            {
                if (_isValid)
                    return;

                _origin = currentOrigin;
                _originals.Clear();
                foreach (var child in children)
                {
                    if (child is Behavior<T> typed)
                    {
                        _originals.Add(typed.Target);
                    }
                }

                _results.Clear();
                _results.AddRange(_originals);

                _isValid = true;
            }

            public void Recalculate(T entryPoint, T newGlobalTarget, SequenceStrategy strategy)
            {
                if (_originals.Count == 0)
                    return;

                if (_results.Count != _originals.Count)
                {
                    _results.Clear();
                    _results.AddRange(_originals);
                }

                T originalGlobalTarget = _originals[^1];
                T delta = TypeUtils.Subtract(newGlobalTarget, originalGlobalTarget);

                var positions = ComputePathPositions();

                for (int index = 0; index < _originals.Count - 1; index++)
                {
                    T originalSegmentTarget = _originals[index];

                    T pRubber = TypeUtils.Interpolate(
                        entryPoint,
                        newGlobalTarget,
                        positions[index]
                    );

                    T pRigidStart = originalSegmentTarget;
                    T pRigidEnd = TypeUtils.Add(originalSegmentTarget, delta);

                    T pRigid = TypeUtils.Interpolate(pRigidStart, pRigidEnd, strategy.Anchor);

                    T newSegmentTarget = TypeUtils.Interpolate(
                        pRigid,
                        pRubber,
                        strategy.Elasticity
                    );

                    _results[index] = newSegmentTarget;
                }

                _results[^1] = newGlobalTarget;
            }

            /// <summary>
            /// Position of every original waypoint along the authored polyline
            /// (_origin -> w0 -> ... -> wN), normalized to [0, 1]. The last
            /// waypoint is always 1, so the rubber band always ends exactly at
            /// the new global target regardless of the entry point.
            /// </summary>
            private float[] ComputePathPositions()
            {
                int count = _originals.Count;
                var positions = new float[count];

                positions[0] = TypeUtils.GetDistance(_origin, _originals[0]);
                for (int i = 1; i < count; i++)
                {
                    positions[i] =
                        positions[i - 1] + TypeUtils.GetDistance(_originals[i - 1], _originals[i]);
                }

                var total = positions[count - 1];
                if (total > 0.0001f)
                {
                    for (int i = 0; i < count; i++)
                        positions[i] /= total;
                }
                else
                {
                    for (int i = 0; i < count; i++)
                        positions[i] = 1f;
                }

                return positions;
            }

            public bool TryGetResult(int index, out T result)
            {
                if (index >= 0 && index < _results.Count)
                {
                    result = _results[index];
                    return true;
                }
                result = default!;
                return false;
            }
        }
    }
}
