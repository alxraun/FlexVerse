// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextOnMountScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer can access and use an inherited context value within its OnMount lifecycle method.
            - Given: A Provider component supplies a context value. A Consumer component needs this value to initialize its own internal state upon mounting.
            - When: The Consumer component is mounted.
            - Then: The Consumer successfully reads the context value inside its OnMount method and uses it to set its initial state, which is then visualized correctly.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcupwsProvider
            {
                FlexDirection = FlexDirections.Column,
                AlignItems = Align.Center,
            };
        }

        public partial class IcupwsProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> ContextColor { get; set; } = Color.green;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return [new IcupwsConsumer { }];
            }
        }

        public partial class IcupwsConsumer : Widget
        {
            [InheritedContext]
            public Behavior<Color> ContextColor { get; set; }

            [State]
            public Behavior<Color> Color { get; set; }

            public override void OnMount()
            {
                Color = ContextColor;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new ColorDisplay { DisplayColor = Color };
            }
        }
    }
}
