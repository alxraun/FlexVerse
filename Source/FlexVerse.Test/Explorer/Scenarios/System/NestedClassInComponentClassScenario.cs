// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class NestedClassInComponentClassScenario : Scenario
    {
        [State]
        private Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new NestedClassInComponentClass
                {
                    Count = Count.Value,
                    OnIncrement = new Action(() => Count.Target++),
                },
            ];

        public partial class NestedClassInComponentClass : CounterDisplay { }
    }
}
