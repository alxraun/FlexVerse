// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorLifetimeMountAndUnmountNonSoleRefHolderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a behavior is **not** reset when a non-sole reference holder is unmounted.
            - Given: A `Parent` component holds a reference to a behavior and shares it with a `Child` component. The `Parent` subscribes to the behavior's `OnReset` hook to log a message if it is triggered.
            - When: The `Child` component is unmounted, causing it to release its reference.
            - Then: The `OnReset` hook is **not** triggered, confirming the behavior remains active as the `Parent` still holds a reference.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DlmunsrhParent { }];
        }

        public partial class DlmunsrhParent : Widget
        {
            [State]
            private Behavior<bool> Toggle { get; set; } = false;

            [State]
            private Behavior<int> Behavior { get; set; } = 1;

            public override void OnMount()
            {
                Behavior.OnReset += OnResetHandler;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    Toggle.Value ? new DlmunsrhChild { Behavior = Behavior } : null,
                    new Button
                    {
                        Label = Toggle.Value ? "Unmount Child" : "Mount Child",
                        OnClick = new Action(() => Toggle.Target = !Toggle.Value),
                    },
                ];
            }

            private void OnResetHandler() =>
                Messages.Message("Behavior was reset", MessageTypeDefOf.NegativeEvent);
        }

        public partial class DlmunsrhChild : Widget
        {
            [Prop]
            public Behavior<int> Behavior { get; set; }
        }
    }
}
