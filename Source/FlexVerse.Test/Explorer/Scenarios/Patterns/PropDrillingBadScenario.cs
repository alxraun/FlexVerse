// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class PropDrillingBadScenario : Scenario
    {
        [State]
        public Behavior<int> Count { get; set; } = 0;

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new PdbParent
            {
                Count = Count.Value,
                OnIncrement = new Action(() => Count.Target++),
            };
        }

        public partial class PdbParent : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new PdbChild { Count = Count.Value, OnIncrement = OnIncrement.Value };
            }
        }

        public partial class PdbChild : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new PdbGrandchild
                {
                    Count = Count.Value,
                    OnIncrement = OnIncrement.Value,
                };
            }
        }

        public partial class PdbGrandchild : Widget
        {
            [Prop]
            public Behavior<int> Count { get; set; }

            [Prop]
            public Behavior<Action> OnIncrement { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new CounterDisplay
                {
                    Count = Count.Value,
                    OnIncrement = OnIncrement.Value,
                };
            }
        }
    }
}
