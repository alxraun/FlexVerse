// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Describes the execution state of a timeline action.</summary>
    public enum TimelineActionStatus
    {
        /// <summary>The action has started execution.</summary>
        Ready,

        /// <summary>The action is currently executing.</summary>
        Running,

        /// <summary>The action finished successfully.</summary>
        Completed,

        /// <summary>The action was terminated before completion.</summary>
        Interrupted,
    }

    /// <summary>Defines the executable contract for timeline actions.</summary>
    public interface ITimelineAction
    {
        /// <summary>Gets the current execution state of the action.</summary>
        TimelineActionStatus Status { get; }

        /// <summary>Begins execution of the action.</summary>
        void Start();

        /// <summary>Advances execution by one tick.</summary>
        void Update(float deltaTime);

        /// <summary>Terminates execution of the action.</summary>
        void Stop();

        /// <summary>Creates an independent copy of the action blueprint.</summary>
        ITimelineAction Clone();
    }

    internal sealed class TimelinePlayer
    {
        private readonly ITimelineAction _blueprint;
        private ITimelineAction _root;

        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        public TimelinePlayer(ITimelineAction blueprint)
        {
            _blueprint = blueprint ?? throw new ArgumentNullException(nameof(blueprint));
        }

        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            _root = _blueprint.Clone();
            Status = TimelineActionStatus.Running;
            try
            {
                _root.Start();
                Synchronize();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            try
            {
                _root.Update(deltaTime);
                Synchronize();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        public void Stop()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            Status = TimelineActionStatus.Interrupted;
            _root?.Stop();
        }

        private void Synchronize()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            if (_root.Status == TimelineActionStatus.Completed)
                Status = TimelineActionStatus.Completed;
            else if (_root.Status == TimelineActionStatus.Interrupted)
            {
                Status = TimelineActionStatus.Interrupted;
                _root.Stop();
            }
        }
    }
}
