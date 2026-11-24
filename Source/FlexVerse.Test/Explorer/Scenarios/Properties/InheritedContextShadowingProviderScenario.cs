// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextShadowingProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer receives the value from the nearest ancestor provider, ignoring more distant ones.
            - Given: A Grandparent provides one context value. Its child, a Parent component, provides a different value, shadowing the first. Consumers are placed as children of both providers.
            - When: The component tree is mounted.
            - Then: Each consumer's ColorDisplay shows the color from its nearest provider, confirming the shadowing logic is correct.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcspsFirstProvider { };
        }

        public partial class IcspsFirstProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; } = UnityEngine.Color.green;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IcspsSecondProvider { };
                yield return new IcspsConsumer { };
            }
        }

        public partial class IcspsSecondProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; } = UnityEngine.Color.blue;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IcspsConsumer { };
            }
        }

        public partial class IcspsConsumer : Widget
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
