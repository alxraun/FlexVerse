// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ComputedMultipleDependenciesScenario : Scenario
    {
        [State]
        public Behavior<int> FirstCount { get; set; }

        [State]
        public Behavior<int> SecondCount { get; set; }

        [State]
        public Behavior<int> Sum { get; set; }

        public override void OnMount()
        {
            Sum = Group.Combine(FirstCount, SecondCount, (a, b) => a + b);
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new BehaviorDisplay
            {
                ActiveBehavior = FirstCount,
                Label = $"First Count: {FirstCount.Value}",
                OnClick = new Action(() => FirstCount.Target++),
            };
            yield return new BehaviorDisplay
            {
                ActiveBehavior = SecondCount,
                Label = $"Second Count: {SecondCount.Value}",
                OnClick = new Action(() => SecondCount.Target++),
            };
            yield return new BehaviorDisplay { ActiveBehavior = Sum, Label = $"Sum: {Sum.Value}" };
        }
    }
}
