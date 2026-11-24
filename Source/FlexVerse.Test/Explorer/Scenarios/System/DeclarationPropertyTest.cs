// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/System/DeclarationProperty")]
    public partial class DeclarationPropertyTest : Test
    {
        public static new string Description =>
            @"
            - Goal: Verify that the source generator preserves property declaration details in widgets and scenarios.
            - Given: A set of widgets/scenarios with diverse property accessors (auto, private set), modifiers (public, internal, protected, etc.), and different combinations.
            - When: The source generator processes these widgets.
            - Then: The partial class generated retains the exact property declarations, allowing compilation and correct functionality (incremental counter as a basic test).
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(StandardAccessorScenario),
                    typeof(PrivateSetAccessorScenario),
                    typeof(PublicModifierScenario),
                    typeof(InternalModifierScenario),
                    typeof(ProtectedModifierScenario),
                    typeof(PrivateModifierScenario),
                    typeof(ProtectedInternalModifierScenario),
                    typeof(PrivateProtectedModifierScenario),
                    typeof(OverrideModifierScenario),
                    typeof(NewModifierScenario),
                    typeof(AbstractModifierScenario),
                    typeof(SealedOverrideModifierScenario),
                },
            };
        }

        #region Accessor Variations
        public partial class StandardAccessorScenario : Scenario
        {
            [Prop]
            public Behavior<int> Count { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }

        public partial class PrivateSetAccessorScenario : Scenario
        {
            [Prop]
            public Behavior<int> Count { get; private set; } = 0;

            public PrivateSetAccessorScenario() => Count = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }
        #endregion

        #region Access Modifier Variations
        public partial class PublicModifierScenario : Scenario
        {
            [Prop]
            public Behavior<int> Count { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }

        public partial class InternalModifierScenario : Scenario
        {
            [Prop]
            internal Behavior<int> Count { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }

        public abstract partial class BaseForAccessModifierScenario : Scenario
        {
            [Prop]
            protected Behavior<int> ProtectedCount { get; set; } = 0;

            [Prop]
            private protected Behavior<int> PrivateProtectedCount { get; set; } = 0;
        }

        public partial class ProtectedModifierScenario : BaseForAccessModifierScenario
        {
            public ProtectedModifierScenario() => ProtectedCount = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = ProtectedCount,
                        OnIncrement = new Action(() => ProtectedCount.Target++),
                    },
                ];
        }

        public partial class PrivateProtectedModifierScenario : BaseForAccessModifierScenario
        {
            public PrivateProtectedModifierScenario() => PrivateProtectedCount = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = PrivateProtectedCount,
                        OnIncrement = new Action(() => PrivateProtectedCount.Target++),
                    },
                ];
        }

        public partial class PrivateModifierScenario : Scenario
        {
            [Prop]
            private Behavior<int> Count { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }

        public partial class ProtectedInternalModifierScenario : Scenario
        {
            [Prop]
            protected internal Behavior<int> Count { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = Count,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];
        }
        #endregion

        #region Inheritance Modifier Variations
        public abstract partial class BaseForInheritanceScenario : Scenario
        {
            [Prop]
            public virtual Behavior<int> OverridableCount { get; set; } = 0;

            [Prop]
            public Behavior<int> ShadowableCount { get; set; } = 0;

            [Prop]
            public abstract Behavior<int> AbstractCount { get; set; }

            [Prop]
            public virtual Behavior<int> SealableCount { get; set; } = 0;
        }

        public partial class OverrideModifierScenario : BaseForInheritanceScenario
        {
            [Prop]
            public override Behavior<int> OverridableCount { get; set; } = 1;
            public override Behavior<int> AbstractCount { get; set; }

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = OverridableCount,
                        OnIncrement = new Action(() => OverridableCount.Target++),
                    },
                ];
        }

        public partial class NewModifierScenario : BaseForInheritanceScenario
        {
            [Prop]
            public new Behavior<int> ShadowableCount { get; set; } = 1;
            public override Behavior<int> AbstractCount { get; set; }

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = ShadowableCount,
                        OnIncrement = new Action(() => ShadowableCount.Target++),
                    },
                ];
        }

        public partial class AbstractModifierScenario : BaseForInheritanceScenario
        {
            [Prop]
            public override Behavior<int> AbstractCount { get; set; } = 1;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = AbstractCount,
                        OnIncrement = new Action(() => AbstractCount.Target++),
                    },
                ];
        }

        public partial class SealedOverrideModifierScenario : BaseForInheritanceScenario
        {
            [Prop]
            public sealed override Behavior<int> SealableCount { get; set; } = 1;
            public override Behavior<int> AbstractCount { get; set; }

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new CounterDisplay
                    {
                        Count = SealableCount,
                        OnIncrement = new Action(() => SealableCount.Target++),
                    },
                ];
        }
        #endregion

        #region Negative Test Cases

        #endregion
    }
}
