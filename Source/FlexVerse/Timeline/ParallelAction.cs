// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Starts children concurrently and completes after all complete. (REQ-003)</summary>
    public sealed class ParallelAction : ITimelineAction
    {
        private readonly ITimelineAction[] _children;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates a concurrent composite action.</summary>
        public ParallelAction(params ITimelineAction[] children)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
            foreach (var child in _children)
                if (child == null)
                    throw new ArgumentException(
                        "Parallel children cannot be null.",
                        nameof(children)
                    );
        }

        /// <summary>Starts all children concurrently. (REQ-003)</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;

            Status = TimelineActionStatus.Running;
            foreach (var child in _children)
            {
                if (Status != TimelineActionStatus.Running)
                    return;

                child.Start();
                if (child.Status == TimelineActionStatus.Interrupted)
                {
                    Stop();
                    return;
                }
            }
            Synchronize();
        }

        /// <summary>Advances running children and completes after all complete. (REQ-003)</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            foreach (var child in _children)
            {
                if (Status != TimelineActionStatus.Running)
                    return;

                if (child.Status == TimelineActionStatus.Running)
                    child.Update(deltaTime);

                if (child.Status == TimelineActionStatus.Interrupted)
                {
                    Stop();
                    return;
                }
            }
            Synchronize();
        }

        private void Synchronize()
        {
            bool allCompleted = true;
            foreach (var child in _children)
            {
                if (child.Status == TimelineActionStatus.Interrupted)
                {
                    Stop();
                    return;
                }
                if (child.Status != TimelineActionStatus.Completed)
                    allCompleted = false;
            }
            if (allCompleted)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Interrupts all children.</summary>
        public void Stop()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            Status = TimelineActionStatus.Interrupted;
            foreach (var child in _children)
                child.Stop();
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone()
        {
            var copies = new ITimelineAction[_children.Length];
            for (var i = 0; i < copies.Length; i++)
                copies[i] = _children[i].Clone();
            return new ParallelAction(copies);
        }
    }
}
