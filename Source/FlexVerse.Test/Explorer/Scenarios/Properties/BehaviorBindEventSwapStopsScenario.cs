// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using FlexVerse.Test.Explorer;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorBindEventSwapStopsScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that a `Bind` subscription is automatically revoked when the property it is bound to is reassigned.
            - Given: A child component whose `Behavior` property is initially assigned 'Behavior A'. An `OnUpdate` handler is attached via `Bind.Behavior.OnUpdate`.
            - When: The parent rebuilds its render tree, resulting in the child's `Behavior` property being reassigned to 'Behavior B'. Subsequently, the original 'Behavior A' is manually updated.
            - Then: The child's bound handler is NOT invoked, confirming the subscription to 'Behavior A' was terminated upon property reassignment.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DbessParent()];
        }

        public partial class DbessParent : Widget
        {
            [State]
            public Behavior<int> BehaviorA { get; set; } = 1;

            [State]
            public Behavior<int> BehaviorB { get; set; } = 2;

            [State]
            public Behavior<bool> Toggle { get; set; } = true;

            public DbessParent() => FlexDirection = FlexDirections.Column;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = Toggle.Value ? "Switch to B" : "Switch to A",
                        OnClick = new Action(() => Toggle.Target = !Toggle.Value),
                    },
                    new Button
                    {
                        Label = "Invoke handler of A",
                        OnClick = new Action(() => BehaviorA.InvokeEvent("OnUpdate", 1f)),
                    },
                    new Button
                    {
                        Label = "Invoke handler of B",
                        OnClick = new Action(() => BehaviorB.InvokeEvent("OnUpdate", 1f)),
                    },
                    new DbessChild { Behavior = Toggle.Value ? BehaviorA : BehaviorB },
                ];
            }
        }

        public partial class DbessChild : Widget
        {
            [Prop]
            public Behavior<int> Behavior { get; set; }

            public DbessChild()
            {
                Refs.Behavior.Changed += () =>
                {
                    Bind.Behavior.OnUpdate += (_) =>
                        Messages.Message(
                            "Behavior invokes its handler",
                            MessageTypeDefOf.NeutralEvent
                        );
                };
            }
        }
    }
}
