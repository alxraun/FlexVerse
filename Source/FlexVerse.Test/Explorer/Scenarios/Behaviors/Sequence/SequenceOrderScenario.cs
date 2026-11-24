// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: the chain plays strictly in order — child 2 starts only
    /// after child 1 reported completion. The numbered canvas vertices light
    /// up one by one; lamps under the canvas confirm the order.
    /// </summary>
    public partial class SequenceOrderScenario : Scenario
    {
        public static new string Description =>
            "The chain plays strictly in order: each numbered vertex lights up only after the previous child completes.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } = NewDefaultChain();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "01  /  EXECUTION ORDER",
                    Title = "One child at a time",
                    Instruction = "Retarget the field and follow the numbered vertices.",
                    Expected =
                        "Vertices become current in order 1 → 2 → 3. Each child completes before the next starts.",
                    Failure =
                        "Two children updating together, or a child starting before its predecessor completes.",
                    StageAccessory = new SequenceLampsRow { Sequence = Sequence },
                    Controls = null,
                    ShowEvents = true,
                },
            ];
    }
}
