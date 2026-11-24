// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextDeepConsumerScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a component receives a context value from a distant ancestor.
            - Given: A Grandparent provides a context value. An intermediate Parent component does not interact with the context. A Grandchild component consumes the context.
            - When: The component tree is mounted.
            - Then: The Grandchild's ColorDisplay shows the color from the Grandparent, confirming the context 'passed through' the intermediate component.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcdcParent { };
        }

        public partial class IcdcParent : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; } = UnityEngine.Color.green;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return [new IcdcChild { }];
            }
        }

        public partial class IcdcChild : Widget
        {
            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IcdcGrandchild { };
            }
        }

        public partial class IcdcGrandchild : Widget
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
