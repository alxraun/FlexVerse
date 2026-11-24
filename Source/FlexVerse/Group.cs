// parents: @.frostline/graph/m-sequence-behavior.xml, @.frostline/graph/m-computed-behavior.xml

using System;
using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Authors sequences and computed combinations of behaviors. (M-COMPUTED-BEHAVIOR)</summary>
    public static class Group
    {
        /// <summary>Creates a sequence with a strategy and event hooks.</summary>
        public static SequenceBehavior<T> Sequence<T>(
            IReadOnlyList<Behavior<T>> behaviors,
            SequenceStrategy strategy,
            Action onActivate = null,
            Action<float> onUpdate = null,
            Action onStart = null,
            Action onComplete = null,
            Action onReset = null
        )
        {
            var list = BuildChildList(behaviors);
            var behavior = new SequenceBehavior<T>(list.ToArray()) { Strategy = strategy };

            behavior.OnActivate += onActivate;
            behavior.OnUpdate += onUpdate;
            behavior.OnStart += onStart;
            behavior.OnComplete += onComplete;
            behavior.OnReset += onReset;

            return behavior;
        }

        /// <summary>Creates a sequence with the default proportional strategy.</summary>
        public static SequenceBehavior<T> Sequence<T>(IReadOnlyList<Behavior<T>> behaviors) =>
            Sequence(behaviors, SequenceStrategy.Proportional);

        /// <summary>Creates a sequence from a behavior array.</summary>
        public static SequenceBehavior<T> Sequence<T>(params Behavior<T>[] behaviors) =>
            Sequence((IReadOnlyList<Behavior<T>>)behaviors);

        /// <summary>Creates a computed behavior from two dependencies. (REQ-016)</summary>
        public static ComputedBehavior<TResult> Combine<T1, T2, TResult>(
            Behavior<T1> first,
            Behavior<T2> second,
            Func<T1, T2, TResult> combiner
        )
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (combiner == null)
                throw new ArgumentNullException(nameof(combiner));

            return new ComputedBehavior<TResult>(
                () => combiner(first.Value, second.Value),
                first,
                second
            );
        }

        /// <summary>Creates a computed behavior from three dependencies. (REQ-016)</summary>
        public static ComputedBehavior<TResult> Combine<T1, T2, T3, TResult>(
            Behavior<T1> first,
            Behavior<T2> second,
            Behavior<T3> third,
            Func<T1, T2, T3, TResult> combiner
        )
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (third == null)
                throw new ArgumentNullException(nameof(third));
            if (combiner == null)
                throw new ArgumentNullException(nameof(combiner));

            return new ComputedBehavior<TResult>(
                () => combiner(first.Value, second.Value, third.Value),
                first,
                second,
                third
            );
        }

        /// <summary>Creates a computed behavior from four dependencies. (REQ-016)</summary>
        public static ComputedBehavior<TResult> Combine<T1, T2, T3, T4, TResult>(
            Behavior<T1> first,
            Behavior<T2> second,
            Behavior<T3> third,
            Behavior<T4> fourth,
            Func<T1, T2, T3, T4, TResult> combiner
        )
        {
            if (first == null)
                throw new ArgumentNullException(nameof(first));
            if (second == null)
                throw new ArgumentNullException(nameof(second));
            if (third == null)
                throw new ArgumentNullException(nameof(third));
            if (fourth == null)
                throw new ArgumentNullException(nameof(fourth));
            if (combiner == null)
                throw new ArgumentNullException(nameof(combiner));

            return new ComputedBehavior<TResult>(
                () => combiner(first.Value, second.Value, third.Value, fourth.Value),
                first,
                second,
                third,
                fourth
            );
        }

        /// <summary>Creates a computed behavior from an untyped dependency list. (REQ-016)</summary>
        public static ComputedBehavior<TResult> Combine<TResult>(
            Func<TResult> computer,
            params Behavior[] dependencies
        ) => new ComputedBehavior<TResult>(computer, dependencies);

        private static List<Behavior<T>> BuildChildList<T>(IReadOnlyList<Behavior<T>> behaviors)
        {
            if (behaviors == null)
            {
                return [];
            }

            var list = new List<Behavior<T>>(behaviors.Count);
            for (int i = 0; i < behaviors.Count; i++)
            {
                var item = behaviors[i];
                if (item is not null)
                {
                    list.Add(item);
                }
            }

            return list;
        }
    }
}
