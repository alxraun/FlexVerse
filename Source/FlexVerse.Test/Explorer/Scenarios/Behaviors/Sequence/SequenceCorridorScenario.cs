// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: a mid-flight retarget builds a corridor from the live
    /// value — the cyan live polyline re-lands onto the white dashed line
    /// value -> target, with spacing determined by the original path distances.
    /// </summary>
    public partial class SequenceCorridorScenario : Scenario
    {
        public static new string Description =>
            "Mid-flight retarget: the cyan live polyline lands on the value -> target corridor, preserving path-distance spacing.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } = NewDefaultChain();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "02  /  RETARGET CORRIDOR",
                    Title = "A new target, a live corridor",
                    Instruction = "Retarget while the amber value is moving.",
                    Expected =
                        "Cyan vertices land on the white dashed value → target corridor. Uneven spacing follows the original path distances.",
                    Failure =
                        "Vertices off the corridor, or equal spacing replacing the original path distances.",
                    StageAccessory = null,
                    Controls = null,
                    ShowEvents = false,
                },
            ];
    }
}
