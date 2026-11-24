// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Chain factories for the 2D sequence scenarios: tween waypoints with
    /// deliberately uneven legs (so proportional path-distance mapping is
    /// visible), a ready-made sequence and the restart-modified strategy
    /// preset. Pure data — no UI.
    /// </summary>
    internal static class SequenceChains
    {
        internal static readonly Vector2 Waypoint1 = new(4f, 1f);
        internal static readonly Vector2 Waypoint2 = new(6f, 6f);
        internal static readonly Vector2 Waypoint3 = new(13f, 8f);

        internal static TweenBehavior<Vector2> Tween(Vector2 target) => Animate.To(target, 0.5f);

        internal static SequenceBehavior<Vector2> NewSequence(params Behavior[] children) =>
            new()
            {
                Strategy = SequenceStrategy.Proportional,
                Target = Vector2.zero,
                Value = Vector2.zero,
                Behaviors = children,
            };

        internal static SequenceBehavior<Vector2> NewDefaultChain() =>
            NewSequence(Tween(Waypoint1), Tween(Waypoint2), Tween(Waypoint3));

        internal static SequenceStrategy WithRestart(SequenceStrategy preset, bool restart) =>
            new()
            {
                Elasticity = preset.Elasticity,
                Anchor = preset.Anchor,
                Restart = restart,
            };
    }
}
