// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class PropDrillingGoodScenario : Scenario
    {
        [State]
        public Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new PdgParent
            {
                Count = Count,
                OnIncrement = new Action(() => Count.Target++),
            };
        }

        public partial class PdgParent : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new PdgChild { Count = Count, OnIncrement = OnIncrement };
            }
        }

        public partial class PdgChild : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new PdgGrandchild { Count = Count, OnIncrement = OnIncrement };
            }
        }

        public partial class PdgGrandchild : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new CounterDisplay { Count = Count, OnIncrement = OnIncrement };
            }
        }
    }
}
