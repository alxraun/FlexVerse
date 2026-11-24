// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextSetProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer automatically updates when its provider changes the context value.
            - Given: A Provider component and a Consumer component. The Provider has a Button to change its provided color.
            - When: The user clicks the Button, causing the provider to set a new behavior.
            - Then: The Consumer's ColorDisplay immediately updates its color to the new value, confirming the system's reactivity.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcspProvider
            {
                FlexDirection = FlexDirections.Column,
                AlignItems = Align.Center,
            };
        }

        public partial class IcspProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> ContextColor { get; set; } = Color.green;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = "Change Color",
                        OnClick = new Action(() =>
                            ContextColor =
                                ContextColor.Target == Color.green ? Color.blue : Color.green
                        ),
                    },
                    new IcspConsumer { },
                ];
            }
        }

        public partial class IcspConsumer : Widget
        {
            [InheritedContext]
            public Behavior<Color> ContextColor { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new ColorDisplay { DisplayColor = ContextColor };
            }
        }
    }
}
