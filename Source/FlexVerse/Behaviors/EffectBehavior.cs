// parents: @.frostline/graph/m-effect-behavior.xml

using System;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Selects the events that admit an effect callback. (M-EFFECT-BEHAVIOR)</summary>
    [Flags]
    public enum EffectTrigger
    {
        /// <summary>Admits no callback execution.</summary>
        None = 0,

        /// <summary>Admits a callback on effect activation.</summary>
        OnActivate = 1,

        /// <summary>Admits a callback on dependency update ticks.</summary>
        OnUpdate = 2,

        /// <summary>Admits a callback on dependency completion.</summary>
        OnComplete = 4,
    }

    /// <summary>Executes a callback on configured dependency behavior events. (M-EFFECT-BEHAVIOR)</summary>
    public class EffectBehavior : Behavior
    {
        private Action _effectCallback;
        private Behavior[] _dependencies;
        private EffectTrigger _trigger = EffectTrigger.OnActivate | EffectTrigger.OnUpdate;

        /// <summary>Gets or sets the callback executed on admitted triggers.</summary>
        public Action EffectCallback
        {
            get => _effectCallback;
            set => _effectCallback = value;
        }

        /// <summary>Gets or sets the dependency behaviors observed for triggers.</summary>
        public Behavior[] Dependencies
        {
            get => _dependencies;
            set
            {
                if (ReferenceEquals(_dependencies, value))
                    return;
                if (State == Lifecycle.Active)
                    SetAndSubscribe(ref _dependencies, value);
                else
                    _dependencies = value;
            }
        }

        /// <summary>Gets or sets the admitted trigger events. (REQ-001)</summary>
        public EffectTrigger Trigger
        {
            get => _trigger;
            set
            {
                if (_trigger == value)
                    return;
                if (State == Lifecycle.Active)
                    SetAndSubscribe(ref _trigger, value);
                else
                    _trigger = value;
            }
        }

        private Action<float> _onUpdateHandler;
        private Action _onCompleteHandler;
        private int _lastFrame = -1;
        private bool _isExecuting;

        /// <summary>Activates the effect and subscribes to dependency events.</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;

            _onUpdateHandler = _ => HandleEffectTrigger();
            _onCompleteHandler = HandleEffectTrigger;

            Subscribe();

            _onActivate?.Invoke();

            if ((_trigger & EffectTrigger.OnActivate) != 0)
                HandleEffectTrigger();
        }

        /// <summary>Advances the effect with no per-tick work. (REQ-001)</summary>
        public override void Update(float deltaTime) { }

        /// <summary>Accepts state handoff with no effect.</summary>
        public override void UnionWith(Behavior source) { }

        /// <summary>Compares effect identity by reference.</summary>
        public override bool SemEquals(Behavior other)
        {
            return ReferenceEquals(this, other);
        }

        /// <summary>Releases dependency subscriptions for pooling. (REQ-004)</summary>
        public override void Reset()
        {
            Unsubscribe();

            _lastFrame = -1;
            _isExecuting = false;
            _onUpdateHandler = null;
            _onCompleteHandler = null;

            base.Reset();
        }

        /// <summary>Creates a clone sharing the callback, dependencies and trigger.</summary>
        public override Behavior CloneUntyped()
        {
            return new EffectBehavior
            {
                EffectCallback = EffectCallback,
                Dependencies = Dependencies,
                Trigger = Trigger,
            };
        }

        private void Subscribe()
        {
            if (_dependencies == null)
                return;

            for (int i = 0; i < _dependencies.Length; i++)
            {
                var dependency = _dependencies[i];
                if (dependency == null)
                    continue;

                if (_trigger.HasFlag(EffectTrigger.OnUpdate) && _onUpdateHandler != null)
                    dependency.OnUpdate += _onUpdateHandler;
                if (_trigger.HasFlag(EffectTrigger.OnComplete) && _onCompleteHandler != null)
                    dependency.OnComplete += _onCompleteHandler;
            }
        }

        private void Unsubscribe()
        {
            if (_dependencies == null)
                return;

            for (int i = 0; i < _dependencies.Length; i++)
            {
                var dependency = _dependencies[i];
                if (dependency == null)
                    continue;

                if (_onUpdateHandler != null)
                    dependency.OnUpdate -= _onUpdateHandler;
                if (_onCompleteHandler != null)
                    dependency.OnComplete -= _onCompleteHandler;
            }
        }

        private void HandleEffectTrigger()
        {
            int frame = Time.frameCount;
            if (frame == _lastFrame || _isExecuting || _effectCallback == null)
                return;

            _lastFrame = frame;
            _isExecuting = true;
            try
            {
                try
                {
                    StartUpdating();
                    _effectCallback();

                    if (State == Lifecycle.Active)
                        _onUpdate?.Invoke(1f);
                }
                finally
                {
                    StopUpdating();
                }
            }
            finally
            {
                _isExecuting = false;
            }
        }

        private void SetAndSubscribe<T>(ref T field, T value)
        {
            Unsubscribe();
            field = value;
            Subscribe();
        }
    }
}
