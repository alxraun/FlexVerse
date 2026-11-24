// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextUpdateProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer automatically updates when its provider changes the context value.
            - Given: A Provider component and a Consumer component. The Provider has a Button to change its provided color.
            - When: The user clicks the Button, causing the provider to update its existing behavior.
            - Then: The Consumer's ColorDisplay immediately updates its color to the new value, confirming the system's reactivity.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcupProvider
            {
                FlexDirection = FlexDirections.Column,
                AlignItems = Align.Center,
            };
        }

        public partial class IcupProvider : Widget
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
                            ContextColor.Target =
                                ContextColor.Target == Color.green ? Color.blue : Color.green
                        ),
                    },
                    new IcupConsumer { },
                ];
            }
        }

        public partial class IcupConsumer : Widget
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
