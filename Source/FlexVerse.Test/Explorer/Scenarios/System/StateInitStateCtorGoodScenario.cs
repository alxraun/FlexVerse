// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class StateInitStateCtorGoodScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To prove that propagating state between two [State] properties in the constructor is a valid pattern.
            - Given: A parent widget's constructor assigns one [State] property to another.
            - When: The widget instance is created and synchronized. The framework's synchronization logic correctly processes both [State] properties, ensuring they both reference the final, correct behavior from the definition.
            - Then: BuildRenderTree uses the second [State] property. The ActualBox renders correctly with the specified padding.
            ";

        [State]
        private Behavior<Length> Desc0 { get; set; } =
            new StaticBehavior<Length> { Target = 10f.Point() };

        [State]
        private Behavior<Length> Desc1 { get; set; }

        public StateInitStateCtorGoodScenario()
        {
            Desc1 = Desc0;
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new ActualBox { PaddingAll = Desc1 }];
        }
    }
}
