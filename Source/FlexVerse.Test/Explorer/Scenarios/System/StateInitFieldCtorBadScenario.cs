// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class StateInitFieldCtorBadScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To prove that caching a [State] property into a plain field during the constructor is an invalid pattern.
            - Given: A parent widget's constructor initializes a [State] property and immediately caches its reference in a plain field.
            - When: The widget instance is created and synchronized with its definition. The synchronization process calls Reset() on the original behavior created in the instance's constructor before replacing it with the behavior from the definition.
            - Then: BuildRenderTree uses the plain field, which still holds a reference to the now-reset behavior (with its `Value` set to null). This causes a runtime error when the child widget tries to use it. The ActualBox will fail to render or render incorrectly.
            ";

        [State]
        private Behavior<Length> Desc0 { get; set; } =
            new StaticBehavior<Length> { Target = 10f.Point() };

        private Behavior<Length> _behavior;

        public StateInitFieldCtorBadScenario()
        {
            _behavior = Desc0;
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new ActualBox { PaddingAll = _behavior }];
        }
    }
}
