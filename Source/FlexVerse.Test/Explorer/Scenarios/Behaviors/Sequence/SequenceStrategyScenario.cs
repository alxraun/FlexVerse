// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using static FlexVerse.Test.Explorer.Tests.SequenceChains;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// One thing: the three strategy presets lay out retargeted waypoints
    /// differently. Same chain, same retarget —
    /// compare where the cyan vertices land: spread on the corridor / the whole path
    /// shifted rigidly / only the last one moved.
    /// </summary>
    public partial class SequenceStrategyScenario : Scenario
    {
        public static new string Description =>
            "Same retarget, three layouts: Proportional spreads vertices on the corridor, FixedOffset shifts the whole path, LastOnly moves only the last.";

        [State]
        private SequenceBehavior<Vector2> Sequence { get; set; } = NewDefaultChain();

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SequencePanel
                {
                    Sequence = Sequence,
                    Kicker = "03  /  GEOMETRY STRATEGIES",
                    Title = "Same chain. Three layouts.",
                    Instruction = "Choose a strategy, then retarget the field.",
                    Expected =
                        "Proportional: spread by path distance.\nFixedOffset: shift the whole path by one delta.\nLastOnly: move only the final vertex.",
                    Failure =
                        "A preset moving vertices contrary to its rule. Restart controls whether replay begins at vertex 1.",
                    StageAccessory = null,
                    Controls = new SequenceStrategyPicker { Sequence = Sequence },
                    ShowEvents = false,
                },
            ];
    }
}
