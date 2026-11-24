// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Repeats a body action while its condition holds. (REQ-004, REQ-010)</summary>
    public sealed class WhileAction : ITimelineAction
    {
        private readonly Func<bool> _condition;
        private readonly ITimelineAction _body;
        private ITimelineAction _iteration;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a conditional loop action.</summary>
        public WhileAction(Func<bool> condition, ITimelineAction body)
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _body = body ?? throw new ArgumentNullException(nameof(body));
        }

        /// <summary>Begins the loop with its first iteration.</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            Status = TimelineActionStatus.Running;
            StartIteration();
        }

        /// <summary>Advances the current iteration and starts at most one next iteration per tick. (REQ-010)</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            if (_iteration.Status == TimelineActionStatus.Running)
                _iteration.Update(deltaTime);

            if (Status != TimelineActionStatus.Running)
                return;

            if (_iteration.Status == TimelineActionStatus.Interrupted)
            {
                Stop();
                return;
            }

            if (_iteration.Status == TimelineActionStatus.Completed)
                StartIteration();
        }

        private void StartIteration()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            bool repeat = _condition();
            if (Status != TimelineActionStatus.Running)
                return;

            if (!repeat)
            {
                Status = TimelineActionStatus.Completed;
                return;
            }

            _iteration = _body.Clone();
            _iteration.Start();
            if (_iteration.Status == TimelineActionStatus.Interrupted)
                Stop();
        }

        /// <summary>Interrupts the current iteration.</summary>
        public void Stop()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            Status = TimelineActionStatus.Interrupted;
            _iteration?.Stop();
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone() => new WhileAction(_condition, _body.Clone());
    }
}
