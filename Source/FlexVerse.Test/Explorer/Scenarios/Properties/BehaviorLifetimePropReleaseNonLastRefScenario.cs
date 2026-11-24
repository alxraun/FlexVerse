// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorLifetimePropReleaseNonLastRefScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a behavior is **not** reset when a non-sole reference is released via a property change.
            - Given: A `Parent` component holds a reference to a behavior and shares it with a `Child`. The `Parent` subscribes to the behavior's `OnReset` hook.
            - When: The `Child`'s property is updated to a different behavior, causing it to release its reference.
            - Then: The `OnReset` hook is **not** triggered, confirming the behavior remains active as the `Parent` still holds a reference.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DlprnlrParent { }];
        }

        public partial class DlprnlrParent : Widget
        {
            [State]
            private Behavior<bool> Switch { get; set; } = false;

            [State]
            private Behavior<int> BehaviorOne { get; set; } = 1;

            [State]
            private Behavior<int> BehaviorTwo { get; set; } = 2;

            public override void OnMount()
            {
                BehaviorOne.OnReset += OnResetHandler;
                BehaviorTwo.OnReset += OnResetHandler;
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new DlprnlrChild { Behavior = Switch.Value ? BehaviorOne : BehaviorTwo },
                    new Button
                    {
                        Label = "Change Behavior",
                        OnClick = new Action(() => Switch.Target = !Switch.Value),
                    },
                ];
            }

            private void OnResetHandler() =>
                Messages.Message("Behavior was reset", MessageTypeDefOf.NeutralEvent);
        }

        public partial class DlprnlrChild : Widget
        {
            [Prop]
            public Behavior<int> Behavior { get; set; }
        }
    }
}
