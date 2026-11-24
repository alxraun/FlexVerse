// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Waits a finite nonnegative duration. (REQ-004)</summary>
    public sealed class WaitAction : ITimelineAction
    {
        private readonly float _duration;
        private float _elapsed;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a fixed-duration wait action.</summary>
        public WaitAction(float duration)
        {
            if (float.IsNaN(duration) || float.IsInfinity(duration) || duration < 0f)
                throw new ArgumentOutOfRangeException(nameof(duration));
            _duration = duration;
        }

        /// <summary>Begins the wait and completes instantly for zero duration.</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            _elapsed = 0f;
            Status =
                _duration == 0f ? TimelineActionStatus.Completed : TimelineActionStatus.Running;
        }

        /// <summary>Accumulates elapsed time and completes at the duration.</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            _elapsed += deltaTime;
            if (_elapsed >= _duration)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Marks a running action as interrupted.</summary>
        public void Stop()
        {
            if (Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Interrupted;
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone() => new WaitAction(_duration);
    }
}
