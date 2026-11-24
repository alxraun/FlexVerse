// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextConditionalProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer correctly unbinds and re-binds as its provider is conditionally added or removed from the tree.
            - Given: A Consumer is a child of a component that can be swapped between a Provider version and a NonProvider version. A Button toggles which version is rendered.
            - When: The user clicks the Button, toggling the provider's presence.
            - Then: The Consumer's ColorDisplay correctly switches between the provider's color and the default fallback color, confirming it can dynamically find and lose its provider.
            ";

        [State]
        public Behavior<bool> IsProviderEnabled { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new Button
                {
                    Label = $"Toggle ({(IsProviderEnabled.Value ? "ON" : "OFF")})",
                    OnClick = new Action(() => IsProviderEnabled.Target = !IsProviderEnabled.Value),
                },
                IsProviderEnabled.Value ? new IccpProvider { } : new IccpNotProvider { },
            ];
        }

        public partial class IccpProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> Color { get; set; } = UnityEngine.Color.green;

            [Prop]
            public Behavior<Widget> ChildContent { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IccpConsumer { };
            }
        }

        public partial class IccpNotProvider : Widget
        {
            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new IccpConsumer { };
            }
        }

        public partial class IccpConsumer : Widget
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
