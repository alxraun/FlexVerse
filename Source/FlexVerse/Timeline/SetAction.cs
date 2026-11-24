// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Assigns a fresh behavior clone into a reactive slot and awaits its completion. (REQ-005)</summary>
    public sealed class SetAction : ITimelineAction
    {
        private readonly IPropertyRef _property;
        private readonly Behavior _blueprint;
        private Behavior _observed;
        private bool _ignoreOwnChanging;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a reactive-slot assignment action.</summary>
        public SetAction(IPropertyRef property, Behavior behavior)
        {
            _property = property ?? throw new ArgumentNullException(nameof(property));
            _blueprint = behavior ?? throw new ArgumentNullException(nameof(behavior));
        }

        /// <summary>Assigns the cloned behavior and observes its completion. (REQ-005, REQ-006)</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            Status = TimelineActionStatus.Running;
            _observed = _blueprint.CloneUntyped();
            if (_observed == null)
                throw new InvalidOperationException("Set behavior clone cannot be null.");

            _property.Changing += OnPropertyChanging;
            _observed.OnComplete += OnBehaviorComplete;
            _ignoreOwnChanging = true;
            try
            {
                _property.Set(_observed);
            }
            catch
            {
                Interrupt();
                throw;
            }
            finally
            {
                _ignoreOwnChanging = false;
            }

            if (Status != TimelineActionStatus.Running)
                return;

            Behavior actual;
            try
            {
                actual = _property.Get();
            }
            catch
            {
                Interrupt();
                throw;
            }

            if (!ReferenceEquals(actual, _observed))
            {
                _observed.OnComplete -= OnBehaviorComplete;
                _observed = actual;

                if (_observed == null)
                {
                    Interrupt();
                    return;
                }
                _observed.OnComplete += OnBehaviorComplete;
            }
        }

        /// <summary>Advances the action with no per-tick work.</summary>
        public void Update(float deltaTime) { }

        private void OnBehaviorComplete()
        {
            if (Status != TimelineActionStatus.Running)
                return;
            Status = TimelineActionStatus.Completed;
            ReleaseSubscriptions();
        }

        private void OnPropertyChanging()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            if (_ignoreOwnChanging)
            {
                _ignoreOwnChanging = false;
                return;
            }

            Interrupt();
        }

        private void Interrupt()
        {
            if (Status != TimelineActionStatus.Running)
                return;
            Status = TimelineActionStatus.Interrupted;
            ReleaseSubscriptions();
        }

        private void ReleaseSubscriptions()
        {
            _property.Changing -= OnPropertyChanging;
            if (_observed != null)
            {
                _observed.OnComplete -= OnBehaviorComplete;
                _observed = null;
            }
        }

        /// <summary>Interrupts observation of the installed behavior. (REQ-006)</summary>
        public void Stop()
        {
            if (Status == TimelineActionStatus.Running)
                Interrupt();
        }

        /// <summary>Creates an independent copy of the action blueprint.</summary>
        public ITimelineAction Clone() => new SetAction(_property, _blueprint);
    }
}
