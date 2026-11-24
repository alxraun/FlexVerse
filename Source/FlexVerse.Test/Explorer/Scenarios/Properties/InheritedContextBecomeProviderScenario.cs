// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class InheritedContextBecomeProviderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a consumer updates when an ancestor component becomes a provider after the initial mount.
            - Given: An Ancestor component that does not initially provide a context, and a Descendant consumer rendered below it. The Ancestor has a Button.
            - When: The user clicks the Button, causing the Ancestor to set a context value.
            - Then: The Descendant's ColorDisplay updates from the default color to the newly provided color.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new IcbpProvider
            {
                FlexDirection = FlexDirections.Column,
                AlignItems = Align.Center,
            };
        }

        public partial class IcbpProvider : Widget
        {
            [InheritedContext]
            public Behavior<Color> ContextColor { get; set; }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = "Become a Provider",
                        OnClick = new Action(() => ContextColor = Color.green),
                    },
                    new IcbpConsumer { },
                ];
            }
        }

        public partial class IcbpConsumer : Widget
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
