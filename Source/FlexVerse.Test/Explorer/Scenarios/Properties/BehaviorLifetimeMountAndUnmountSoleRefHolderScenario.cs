// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorLifetimeMountAndUnmountSoleRefHolderScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a behavior is reset when its sole reference holder is unmounted.
            - Given: A component creates a behavior, becoming its sole reference holder. The component subscribes to the behavior's `OnReset` hook to log a confirmation message.
            - When: The component is unmounted, causing it to release its reference.
            - Then: A message is logged, confirming that the behavior's `Reset()` method was triggered correctly.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DlmusrhParent { }];
        }

        public partial class DlmusrhParent : Widget
        {
            [State]
            private Behavior<bool> Toggle { get; set; } = false;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    Toggle.Value ? new DlmusrhChild { } : null,
                    new Button
                    {
                        Label = Toggle.Value ? "Unmount Widget" : "Mount Widget",
                        OnClick = new Action(() => Toggle.Target = !Toggle.Value),
                    },
                ];
            }
        }

        public partial class DlmusrhChild : Widget
        {
            [State]
            private Behavior<int> Behavior { get; set; } = 1;

            public override void OnMount()
            {
                Behavior.OnReset += OnResetHandler;
            }

            private void OnResetHandler() =>
                Messages.Message("Widget's Behavior was reset", MessageTypeDefOf.NeutralEvent);
        }
    }
}
