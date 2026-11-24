// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: instant (zero-duration) and no-op children do not stall
    /// the chain. The chain is [instant -> waypoint 1, tween -> waypoint 2,
    /// no-op, tween -> waypoint 3]: the value snaps through the first leg
    /// and still reaches the target.
    /// </summary>
    public partial class SequenceInstantScenario : Scenario
    {
        public static new string Description =>
            "Instant and no-op children never stall the chain: the value still reaches the final target.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } =
            NewSequence(
                new StaticBehavior<Vector2> { Target = Waypoint1 },
                Tween(Waypoint2),
                NullBehavior.Instance,
                Tween(Waypoint3)
            );

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "05  /  INSTANT CHILDREN",
                    Title = "Silent steps still advance",
                    Instruction = "Retarget the field and watch the tween lamps.",
                    Expected =
                        "The instant child and no-op stay silent. The chain passes through both and reaches the final target.",
                    Failure = "A stalled chain, or lifecycle events from instant / no-op children.",
                    StageAccessory = new SequenceLampsRow
                    {
                        Sequence = Sequence,
                        Names = new[] { "instant", "tween", "no-op", "tween" },
                    },
                    Controls = null,
                    ShowEvents = true,
                },
            ];
    }
}
