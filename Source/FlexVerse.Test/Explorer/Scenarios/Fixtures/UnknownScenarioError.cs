// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class UnknownScenarioError : Scenario
    {
        [Prop]
        public Behavior<string> ScenarioName { get; set; }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new Label { Content = $"Unknown scenario '{ScenarioName.Value}'" };
        }
    }
}
