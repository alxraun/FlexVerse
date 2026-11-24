// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ComputedOneDependencyScenario : Scenario
    {
        [State]
        public Behavior<int> Count { get; set; }

        [State]
        public Behavior<int> Double { get; set; }

        public override void OnMount()
        {
            Double = Count.Select(x => x * 2);
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new BehaviorDisplay
            {
                ActiveBehavior = Count,
                Label = $"Count: {Count.Value}",
                OnClick = new Action(() => Count.Target++),
            };
            yield return new BehaviorDisplay
            {
                ActiveBehavior = Double,
                Label = $"Double: {Double.Value}",
            };
        }
    }
}
