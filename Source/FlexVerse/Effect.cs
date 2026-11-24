// parents: @.frostline/graph/m-effect-behavior.xml

using System;

namespace FlexVerse
{
    /// <summary>Authors dependency-triggered effect behaviors. (M-EFFECT-BEHAVIOR)</summary>
    public static class Effect
    {
        /// <summary>Creates an effect with an explicit trigger set and dependencies.</summary>
        public static EffectBehavior Create(
            Action effectCallback,
            EffectTrigger trigger,
            params Behavior[] dependencies
        )
        {
            return new EffectBehavior
            {
                EffectCallback = effectCallback,
                Trigger = trigger,
                Dependencies = dependencies,
            };
        }

        /// <summary>Creates an effect with default triggers and dependencies.</summary>
        public static EffectBehavior Create(
            Action effectCallback,
            params Behavior[] dependencies
        ) =>
            Create(effectCallback, EffectTrigger.OnActivate | EffectTrigger.OnUpdate, dependencies);

        /// <summary>Creates an effect with default triggers and no dependencies.</summary>
        public static EffectBehavior Create(Action effectCallback) =>
            Create(effectCallback, EffectTrigger.OnActivate | EffectTrigger.OnUpdate);

        /// <summary>Creates an unconfigured effect behavior.</summary>
        public static EffectBehavior Create()
        {
            return new EffectBehavior { };
        }
    }
}
