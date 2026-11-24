// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class EffectOneDependencyScenario : Scenario
    {
        [State]
        public Behavior<int> Count { get; set; }

        [State]
        public Behavior ActiveEffect { get; set; }

        public override void OnMount()
        {
            ActiveEffect = Effect.Create(
                () => Messages.Message("Count Changed", MessageTypeDefOf.NeutralEvent),
                Count
            );
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new BehaviorDisplay
            {
                ActiveBehavior = Count,
                Label = $"Count: {Count.Value}",
                OnClick = new Action(() => Count.Target++),
            };
        }
    }
}
