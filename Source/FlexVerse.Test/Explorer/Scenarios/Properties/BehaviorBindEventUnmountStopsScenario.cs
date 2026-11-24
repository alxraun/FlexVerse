// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class BehaviorBindEventUnmountStopsScenario : Scenario
    {
        public static new string Description =>
            $@"
            - Goal: To verify that all `Bind` subscriptions made by a component are automatically revoked when the component is unmounted.
            - Given: A child component that subscribes to a shared behavior's `OnUpdate` event via `Bind` upon mounting.
            - When: The parent unmounts the child component. Subsequently, the parent manually updates the shared behavior.
            - Then: The child's bound handler is NOT invoked, confirming that all its subscriptions were terminated during the unmount lifecycle event.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new DbeusParent()];
        }

        public partial class DbeusParent : Widget
        {
            [State]
            public Behavior<int> Behavior { get; set; } = 0;

            [State]
            public Behavior<bool> MountChild { get; set; } = true;

            public DbeusParent() => FlexDirection = FlexDirections.Column;

            public override IEnumerable<Widget> BuildRenderTree()
            {
                return
                [
                    new Button
                    {
                        Label = MountChild.Value ? "Unmount Child" : "Mount Child",
                        OnClick = new Action(() => MountChild.Target = !MountChild.Value),
                    },
                    new Button
                    {
                        Label = "Update Behavior",
                        OnClick = new Action(() => Behavior.InvokeEvent("OnUpdate", 1)),
                    },
                    MountChild.Value ? new DbeusChild { Behavior = Behavior } : null,
                ];
            }
        }

        public partial class DbeusChild : Widget
        {
            [Prop]
            public Behavior<int> Behavior { get; set; }

            private bool _isMounted = false;

            public override void OnMount()
            {
                _isMounted = true;
                Messages.Message("Child mounted.", MessageTypeDefOf.NeutralEvent);
                Bind.Behavior.OnUpdate += OnBehaviorUpdate;
            }

            public override void OnUnmount()
            {
                _isMounted = false;
                Messages.Message("Child unmounted.", MessageTypeDefOf.NeutralEvent);
            }

            private void OnBehaviorUpdate(float _)
            {
                if (_isMounted)
                    Messages.Message(
                        "Child mounted, behavior invokes its handler.",
                        MessageTypeDefOf.NeutralEvent
                    );
                else
                    Messages.Message(
                        "FAIL: Child unmounted, but behavior still invokes its handler.",
                        MessageTypeDefOf.NegativeEvent
                    );
            }
        }
    }
}
