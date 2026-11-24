// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Selects one branch from a condition and runs it. (REQ-004)</summary>
    public sealed class IfAction : ITimelineAction
    {
        private readonly Func<bool> _condition;
        private readonly ITimelineAction _then;
        private readonly ITimelineAction _else;
        private ITimelineAction _selected;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a conditional branch action.</summary>
        public IfAction(
            Func<bool> condition,
            ITimelineAction thenAction,
            ITimelineAction elseAction = null
        )
        {
            _condition = condition ?? throw new ArgumentNullException(nameof(condition));
            _then = thenAction ?? throw new ArgumentNullException(nameof(thenAction));
            _else = elseAction;
        }

        /// <summary>Evaluates the condition once and starts the selected branch.</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            Status = TimelineActionStatus.Running;
            _selected = _condition() ? _then : _else;

            if (Status != TimelineActionStatus.Running)
                return;

            if (_selected == null)
            {
                Status = TimelineActionStatus.Completed;
                return;
            }

            _selected.Start();
            Synchronize();
        }

        /// <summary>Advances the selected branch.</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            _selected.Update(deltaTime);
            Synchronize();
        }

        private void Synchronize()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            if (_selected.Status == TimelineActionStatus.Completed)
                Status = TimelineActionStatus.Completed;
            else if (_selected.Status == TimelineActionStatus.Interrupted)
            {
                Status = TimelineActionStatus.Interrupted;
                _selected.Stop();
            }
        }

        /// <summary>Interrupts the selected branch.</summary>
        public void Stop()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            Status = TimelineActionStatus.Interrupted;
            _selected?.Stop();
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone() => new IfAction(_condition, _then.Clone(), _else?.Clone());
    }
}
