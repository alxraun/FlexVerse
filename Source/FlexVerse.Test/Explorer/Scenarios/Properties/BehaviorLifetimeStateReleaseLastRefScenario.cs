// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorLifetimeStateReleaseLastRefScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a behavior is reset when its sole reference holder releases the reference via a property change.
            - Given: A component is the sole reference holder for a behavior. An external observer subscribes to the behavior's `OnReset` hook to log a confirmation message.
            - When: The component's property is updated to a different behavior, causing it to release its reference to the original one.
            - Then: A message is logged, confirming the original behavior's `Reset()` method was triggered.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DlprlrParent { }];
        }

        public partial class DlprlrParent : Widget
        {
            public override IEnumerable<Widget> BuildRenderTree()
            {
                return [new DlprlrChild { }];
            }
        }

        public partial class DlprlrChild : Widget
        {
            [State]
            private Behavior<int> Behavior { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = "Change Behavior",
                        OnClick = new Action(() =>
                        {
                            Behavior.OnReset += OnResetHandler;
                            Behavior = Behavior.Value == 1 ? 2 : 1;
                        }),
                    },
                ];
            }

            private void OnResetHandler() =>
                Messages.Message("Behavior was reset", MessageTypeDefOf.NeutralEvent);
        }
    }
}
