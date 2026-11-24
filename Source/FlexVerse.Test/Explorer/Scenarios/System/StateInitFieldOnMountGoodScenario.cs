// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class StateInitFieldOnMountGoodScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To prove that caching a [State] property in OnMount is a valid pattern.
            - Given: A parent widget caches its [State] property in a plain field inside OnMount.
            - When: The widget instance is created and synchronized. OnMount is called *after* synchronization is complete. The plain field now captures a reference to the final, correct behavior.
            - Then: BuildRenderTree uses the correct behavior from the field. The ActualBox renders correctly with the specified padding.
            ";

        [State]
        private Behavior<Length> Desc0 { get; set; } =
            new StaticBehavior<Length> { Target = 10f.Point() };

        private Behavior<Length> _behavior;

        public override void OnMount()
        {
            _behavior = Desc0;
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new ActualBox { PaddingAll = _behavior }];
        }
    }
}
