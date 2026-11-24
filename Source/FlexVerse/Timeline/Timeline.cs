// parents: @.frostline/graph/m-timeline-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Builds inert timeline action trees and timeline behaviors. (M-TIMELINE-BEHAVIOR)</summary>
    public static class Timeline
    {
        /// <summary>Creates an ordered composite action.</summary>
        public static ITimelineAction Sequence(params ITimelineAction[] actions) =>
            new SequenceAction(actions);

        /// <summary>Creates a concurrent composite action.</summary>
        public static ITimelineAction Parallel(params ITimelineAction[] actions) =>
            new ParallelAction(actions);

        /// <summary>Creates a typed reactive-slot assignment action.</summary>
        public static ITimelineAction Set<T>(IPropertyRef<T> property, Behavior<T> behavior) =>
            new SetAction(property, behavior);

        /// <summary>Creates an untyped reactive-slot assignment action.</summary>
        public static ITimelineAction Set(IPropertyRef property, Behavior behavior) =>
            new SetAction(property, behavior);

        /// <summary>Creates a fixed-duration wait action.</summary>
        public static ITimelineAction Wait(float duration) => new WaitAction(duration);

        /// <summary>Creates a predicate-polling wait action.</summary>
        public static ITimelineAction WaitUntil(Func<bool> predicate) =>
            new WaitUntilAction(predicate);

        /// <summary>Creates a one-shot callback action.</summary>
        public static ITimelineAction Call(Action callback) => new CallAction(callback);

        /// <summary>Creates a conditional branch action.</summary>
        public static ITimelineAction If(
            Func<bool> condition,
            ITimelineAction thenAction,
            ITimelineAction elseAction = null
        ) => new IfAction(condition, thenAction, elseAction);

        /// <summary>Creates a conditional loop action.</summary>
        public static ITimelineAction While(Func<bool> condition, ITimelineAction action) =>
            new WhileAction(condition, action);

        /// <summary>Wraps an action blueprint as an inactive timeline behavior. (REQ-001)</summary>
        public static TimelineBehavior Create(ITimelineAction rootAction) =>
            new TimelineBehavior(rootAction);
    }
}
