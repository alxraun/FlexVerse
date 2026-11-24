// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorBindEventPropertyScopeScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a subscription made via `Bind` is scoped exclusively to the property slot it was bound to.
            - Given: A component with a `CurrentBehavior` property that is assigned 'Behavior A'. A `Bind` subscription is made to `CurrentBehavior.OnUpdate`. The component also holds a separate, persistent reference to 'Behavior A'.
            - When: The `CurrentBehavior` property is reassigned to 'Behavior B', and then the original 'Behavior A' is manually updated.
            - Then: The bound handler for `CurrentBehavior` is NOT invoked, confirming that the subscription was tied to the *property slot*, not the behavior instance, and was correctly revoked on reassignment.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DbepsParent()];
        }

        public partial class DbepsParent : Widget
        {
            [State]
            public Behavior<int> BehaviorA { get; set; } = 1;

            [State]
            public Behavior<int> BehaviorB { get; set; } = 2;

            [State]
            public Behavior<int> CurrentBehavior { get; set; }

            public DbepsParent()
            {
                FlexDirection = FlexDirections.Column;

                CurrentBehavior = BehaviorA;
                Refs.CurrentBehavior.Changed += () =>
                {
                    Bind.CurrentBehavior.OnUpdate += (_) =>
                        Messages.Message(
                            "Current behavior invokes its handler.",
                            MessageTypeDefOf.NeutralEvent
                        );
                };
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = CurrentBehavior == BehaviorA ? "Switch to B" : "Switch to A",
                        OnClick = new Action(() =>
                            CurrentBehavior = (CurrentBehavior == BehaviorA) ? BehaviorB : BehaviorA
                        ),
                    },
                    new Button
                    {
                        Label = "Invoke handler of A",
                        OnClick = new Action(() => BehaviorA.InvokeEvent("OnUpdate", 0)),
                    },
                    new Button
                    {
                        Label = "Invoke handler of B",
                        OnClick = new Action(() => BehaviorB.InvokeEvent("OnUpdate", 0)),
                    },
                ];
            }
        }
    }
}
