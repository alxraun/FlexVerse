// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextNoProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer gracefully handles the absence of a provider in its ancestry.
            - Given: A Consumer component is rendered in a tree with no ancestor providing the required context.
            - When: The component tree is mounted.
            - Then: The Consumer's ColorDisplay shows its default color, confirming the system provided a fallback value without errors.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcnpParent { };
        }

        public partial class IcnpParent : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IcnpChild { };
            }
        }

        public partial class IcnpChild : Widget
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
