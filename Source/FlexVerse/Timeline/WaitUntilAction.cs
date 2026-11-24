// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Waits until a predicate evaluates to true. (REQ-004)</summary>
    public sealed class WaitUntilAction : ITimelineAction
    {
        private readonly Func<bool> _predicate;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a predicate-polling wait action.</summary>
        public WaitUntilAction(Func<bool> predicate)
        {
            _predicate = predicate ?? throw new ArgumentNullException(nameof(predicate));
        }

        /// <summary>Begins polling and completes for an immediately true predicate.</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;
            Status = TimelineActionStatus.Running;
            if (_predicate() && Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Polls the predicate and completes when it becomes true.</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            if (_predicate() && Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Marks a running action as interrupted.</summary>
        public void Stop()
        {
            if (Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Interrupted;
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone() => new WaitUntilAction(_predicate);
    }
}
