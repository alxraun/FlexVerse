// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextSimpleConsumerScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a component receives a context value from its direct parent.
            - Given: A Parent component provides a context value. A Child component consumes this context and uses a ColorDisplay fixture to visualize the consumed value.
            - When: The component tree is mounted.
            - Then: The Child's ColorDisplay shows the color provided by the parent, confirming it received the value.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcscParent { };
        }

        public partial class IcscParent : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; } = UnityEngine.Color.green;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IcscChild { };
            }
        }

        public partial class IcscChild : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new ColorDisplay { DisplayColor = Color };
            }
        }
    }
}
