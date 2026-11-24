// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: aggregated chain progress lives in [0, 1] and finishes at
    /// exactly 100% — never above, whatever the retarget timing.
    /// </summary>
    public partial class SequenceProgressScenario : Scenario
    {
        public static new string Description =>
            "Aggregated progress stays within [0, 1] and ends exactly at 100%.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } = NewDefaultChain();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "06  /  AGGREGATE PROGRESS",
                    Title = "Every run ends at 100%",
                    Instruction = "Click or drag on the canvas to retarget mid-flight.",
                    Expected =
                        "Progress stays in 0..100% and finishes at exactly 100%. With Restart ON, retargeting restarts chain progress.",
                    Failure =
                        "Progress outside the range, or a completed run that never reaches 100%.",
                    StageAccessory = null,
                    Controls = new SequenceProgressBar { Sequence = Sequence },
                    ShowEvents = false,
                },
            ];
    }
}
