// parents: @.frostline/graph/m-computed-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Authors computed behaviors from a source behavior and selector. (M-COMPUTED-BEHAVIOR)</summary>
    public static class Computed
    {
        /// <summary>Creates a computed behavior projecting one typed dependency. (REQ-016)</summary>
        public static ComputedBehavior<TOut> Select<TIn, TOut>(
            this Behavior<TIn> source,
            Func<TIn, TOut> selector
        )
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (selector == null)
                throw new ArgumentNullException(nameof(selector));

            return new ComputedBehavior<TOut>(() => selector(source.Value), source);
        }
    }
}
