// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class NestedClassInStaticClassScenario : Scenario
    {
        [State]
        private Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new NestedClassInStaticClassContainer.NestedClassInStaticClass
                {
                    Count = Count.Value,
                    OnIncrement = new Action(() => Count.Target++),
                },
            ];
    }

    public static partial class NestedClassInStaticClassContainer
    {
        public partial class NestedClassInStaticClass : CounterDisplay { }
    }
}
