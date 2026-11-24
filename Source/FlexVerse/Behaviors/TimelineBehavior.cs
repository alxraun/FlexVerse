// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Plays a declarative action tree as a behavior. (M-TIMELINE-BEHAVIOR)</summary>
    public sealed class TimelineBehavior : Behavior
    {
        private readonly ITimelineAction _rootAction;
        private TimelinePlayer _player;

        /// <summary>Wraps an action blueprint as an inactive timeline behavior.</summary>
        public TimelineBehavior(ITimelineAction rootAction)
        {
            _rootAction = rootAction ?? throw new ArgumentNullException(nameof(rootAction));
        }

        /// <summary>Starts playback of the action tree. (REQ-001)</summary>
        public override void Activate()
        {
            if (State == Lifecycle.Active)
                return;

            State = Lifecycle.Active;
            _player = new TimelinePlayer(_rootAction);
            _onActivate?.Invoke();
            StartUpdating();

            try
            {
                if (IsUpdating)
                {
                    _player.Start();
                    FinishIfTerminated();
                }
            }
            catch
            {
                Stop();
                throw;
            }
        }

        /// <summary>Advances playback and finishes on termination. (REQ-002)</summary>
        public override void Update(float deltaTime)
        {
            if (!IsUpdating || _player == null)
                return;

            try
            {
                _player.Update(deltaTime);
                FinishIfTerminated();
            }
            catch
            {
                Stop();
                throw;
            }
        }

        /// <summary>Stops playback and ends the updating cycle. (REQ-008)</summary>
        public void Stop()
        {
            _player?.Stop();
            StopUpdating();
        }

        private void FinishIfTerminated()
        {
            if (_player != null && _player.Status != TimelineActionStatus.Running)
                StopUpdating();
        }

        /// <summary>Terminates playback and detaches the player. (REQ-008)</summary>
        public override void Reset()
        {
            Stop();
            _player = null;
            base.Reset();
        }

        /// <summary>Creates a new playback instance from a cloned blueprint. (REQ-009)</summary>
        public override Behavior CloneUntyped() => new TimelineBehavior(_rootAction.Clone());

        /// <summary>Accepts state handoff with no effect.</summary>
        public override void UnionWith(Behavior source) { }

        /// <summary>Compares root action identity by reference.</summary>
        public override bool SemEquals(Behavior other) =>
            other is TimelineBehavior timeline
            && ReferenceEquals(_rootAction, timeline._rootAction);
    }
}
