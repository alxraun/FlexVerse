// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Runs child actions in order and propagates interruption. (REQ-003)</summary>
    public sealed class SequenceAction : ITimelineAction
    {
        private readonly ITimelineAction[] _children;
        private int _index;

        /// <summary>Gets the current execution state of the action.</summary>
        public TimelineActionStatus Status { get; private set; } = TimelineActionStatus.Ready;

        /// <summary>Creates an ordered composite action.</summary>
        public SequenceAction(params ITimelineAction[] children)
        {
            _children = children ?? throw new ArgumentNullException(nameof(children));
            foreach (var child in _children)
                if (child == null)
                    throw new ArgumentException(
                        "Sequence children cannot be null.",
                        nameof(children)
                    );
        }

        /// <summary>Starts the first child. (REQ-003)</summary>
        public void Start()
        {
            if (Status != TimelineActionStatus.Ready)
                return;
            Status = TimelineActionStatus.Running;
            Advance();
        }

        /// <summary>Advances through children as they complete. (REQ-003)</summary>
        public void Update(float deltaTime)
        {
            if (Status != TimelineActionStatus.Running)
                return;

            _children[_index].Update(deltaTime);
            Advance();
        }

        private void Advance()
        {
            while (Status == TimelineActionStatus.Running && _index < _children.Length)
            {
                var child = _children[_index];

                if (child.Status == TimelineActionStatus.Ready)
                    child.Start();

                if (Status != TimelineActionStatus.Running)
                    return;

                if (child.Status == TimelineActionStatus.Interrupted)
                {
                    Status = TimelineActionStatus.Interrupted;
                    child.Stop();
                    return;
                }

                if (child.Status != TimelineActionStatus.Completed)
                    return;

                _index++;
            }

            if (Status == TimelineActionStatus.Running)
                Status = TimelineActionStatus.Completed;
        }

        /// <summary>Interrupts the current child.</summary>
        public void Stop()
        {
            if (Status != TimelineActionStatus.Running)
                return;

            Status = TimelineActionStatus.Interrupted;
            if (_index < _children.Length)
                _children[_index].Stop();
        }

        /// <summary>Creates an independent copy of the action.</summary>
        public ITimelineAction Clone()
        {
            var copies = new ITimelineAction[_children.Length];
            for (var i = 0; i < copies.Length; i++)
                copies[i] = _children[i].Clone();
            return new SequenceAction(copies);
        }
    }
}
