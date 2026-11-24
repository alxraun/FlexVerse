// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Patterns/PropsDownEventsUp")]
    public partial class PropsDownEventsUpTest : Test
    {
        [State]
        public Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new CounterDisplay
                {
                    Count = Count,
                    OnIncrement = new Action(() =>
                    {
                        Count.Target = Count.Value + 1;
                    }),
                },
            ];
        }
    }
}
