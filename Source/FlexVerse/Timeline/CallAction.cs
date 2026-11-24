// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Invokes a callback once and completes synchronously. (REQ-004)</summary>
    public sealed class CallAction : ITimelineAction
    {
        private readonly Action _callback;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a one-shot callback action.</summary>
        public CallAction(Action callback)
        {
            _callback = callback ?? throw new ArgumentNullException(nameof(callback));
        }

        /// <summary>Invokes the callback and completes. (REQ-004)</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            Status = TimelineActionStatus.Running;
            _callback();
            if (Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Advances the action with no per-tick work.</summary>
        public void Update(float deltaTime) { }

        /// <summary>Marks a running action as interrupted.</summary>
        public void Stop()
        {
            if (Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Interrupted;
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone() => new CallAction(_callback);
    }
}
