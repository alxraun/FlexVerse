// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class EffectMultipleDependenciesScenario : Scenario
    {
        [State]
        public Behavior<int> FirstCount { get; set; }

        [State]
        public Behavior<int> SecondCount { get; set; }

        [State]
        public Behavior ActiveEffect { get; set; }

        public override void OnMount()
        {
            ActiveEffect = Effect.Create(
                () => Messages.Message("Count Changed", MessageTypeDefOf.NeutralEvent),
                FirstCount,
                SecondCount
            );
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
        }
    }
}
