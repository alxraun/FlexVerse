// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class NestedClassDeeplyNestedScenario : Scenario
    {
        [State]
        private Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new NestedClassDeeplyNestedLevelOne.NestedClassDeeplyNestedLevelTwo.NestedClassDeeplyNested
                {
                    Count = Count.Value,
                    OnIncrement = new Action(() => Count.Target++),
                },
            ];
    }

    public partial class NestedClassDeeplyNestedLevelOne
    {
        public partial class NestedClassDeeplyNestedLevelTwo
        {
            public partial class NestedClassDeeplyNested : CounterDisplay { }
        }
    }
}
