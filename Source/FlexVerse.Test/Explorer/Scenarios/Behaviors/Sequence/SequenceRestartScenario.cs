// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: restart does not re-Activate children — completed children
    /// are re-driven via live adaptation. With Restart on, a retarget makes
    /// child 1 report OnComplete a second time; with Restart off, the chain
    /// just continues from the current child.
    /// </summary>
    public partial class SequenceRestartScenario : Scenario
    {
        public static new string Description =>
            "Restart = re-drive, not re-Activate: with Restart on, T0 completes a second time on retarget.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } = NewDefaultChain();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "04  /  LIVE REPLAY",
                    Title = "Restart means re-drive",
                    Instruction = "Toggle Restart, then retarget mid-flight.",
                    Expected =
                        "ON: the current ring returns to vertex 1; T0 can complete again.\nOFF: continue from the current child.",
                    Failure =
                        "A new activation on replay, or replay from vertex 1 with Restart OFF.",
                    StageAccessory = new SequenceLampsRow { Sequence = Sequence },
                    Controls = new SequenceRestartSwitch { Sequence = Sequence },
                    ShowEvents = true,
                },
            ];
    }
}
