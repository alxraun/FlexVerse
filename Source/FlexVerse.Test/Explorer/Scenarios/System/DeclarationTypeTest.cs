// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/System/DeclarationType")]
    public partial class DeclarationTypeTest : Test
    {
        public static new string Description =>
            @"
            - Goal: Verify that the source generator correctly reproduces the declaration header for any valid C# type declaration.
            - Given: A series of widgets defined using various C# keywords (class, struct, record), modifiers (sealed, abstract), access levels (public, private, etc.), and generic signatures (<T>, where T : ...).
            - When: The source generator processes these widgets.
            - Then: The generated partial class has a perfectly matching header, allowing the project to compile and the widget (a simple counter) to function correctly.
            ";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new ScenarioHost
            {
                Scenarios = new List<Type>
                {
                    typeof(ClassWidgetScenario),
                    typeof(StructContainerScenario),
                    typeof(RecordContainerScenario),
                    typeof(RecordStructContainerScenario),
                    typeof(SealedWidgetScenario),
                    typeof(AbstractWidgetScenario),
                    typeof(PublicWidgetScenario),
                    typeof(InternalWidgetScenario),
                    typeof(ProtectedWidgetScenario),
                    typeof(PrivateWidgetScenario),
                    typeof(ProtectedInternalWidgetScenario),
                    typeof(PrivateProtectedWidgetScenario),
                    typeof(GenericWidgetScenario),
                    typeof(ConstrainedGenericWidgetScenario),
                    typeof(MultiParamGenericWidgetScenario),
                    typeof(DeeplyNestedWidgetScenario),
                },
            };
        }

        #region Type Keywords
        public partial class ClassWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ClassWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class ClassWidget : CounterDisplay { }
        }

        public partial class StructContainerScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new StructContainer.WidgetInStruct
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial struct StructContainer
            {
                public partial class WidgetInStruct : CounterDisplay { }
            }
        }

        public partial class RecordContainerScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new RecordContainer.WidgetInRecord
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial record RecordContainer
            {
                public partial class WidgetInRecord : CounterDisplay { }
            }
        }

        public partial class RecordStructContainerScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new RecordStructContainer.WidgetInRecordStruct
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial record struct RecordStructContainer
            {
                public partial class WidgetInRecordStruct : CounterDisplay { }
            }
        }
        #endregion

        #region Modifiers
        public partial class SealedWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new SealedWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private sealed partial class SealedWidget : CounterDisplay { }
        }

        public partial class AbstractWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ConcreteWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private abstract partial class AbstractWidget : CounterDisplay { }

            private partial class ConcreteWidget : AbstractWidget { }
        }
        #endregion

        #region Access Modifiers
        public partial class PublicWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new PublicWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            public partial class PublicWidget : CounterDisplay { }
        }

        public partial class InternalWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new InternalWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            internal partial class InternalWidget : CounterDisplay { }
        }

        public partial class ProtectedWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ChildOfProtected
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            protected partial class ProtectedWidget : CounterDisplay { }

            private partial class ChildOfProtected : ProtectedWidget { }
        }

        public partial class PrivateWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new PrivateWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class PrivateWidget : CounterDisplay { }
        }

        public partial class ProtectedInternalWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ProtectedInternalWidget
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            protected internal partial class ProtectedInternalWidget : CounterDisplay { }
        }

        public partial class PrivateProtectedWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ChildOfPrivateProtected
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private protected partial class PrivateProtectedWidget : CounterDisplay { }

            private partial class ChildOfPrivateProtected : PrivateProtectedWidget { }
        }
        #endregion

        #region Generics
        public partial class GenericWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new GenericWidget<int>
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class GenericWidget<T> : CounterDisplay { }
        }

        public partial class ConstrainedGenericWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new ConstrainedGenericWidget<List<int>>
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class ConstrainedGenericWidget<T> : CounterDisplay
                where T : class, new() { }
        }

        public partial class MultiParamGenericWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new MultiParamGenericWidget<int, string>
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class MultiParamGenericWidget<TKey, TValue> : CounterDisplay
                where TKey : struct
                where TValue : class { }
        }
        #endregion

        #region Nesting
        public partial class DeeplyNestedWidgetScenario : Scenario
        {
            [State]
            private Behavior<int> Count { get; set; } = 0;

            public override IEnumerable<Widget> BuildRenderTree() =>
                [
                    new LevelOne.LevelTwo.NestedCounter
                    {
                        Count = Count.Value,
                        OnIncrement = new Action(() => Count.Target++),
                    },
                ];

            private partial class LevelOne
            {
                public partial class LevelTwo
                {
                    public partial class NestedCounter : CounterDisplay { }
                }
            }
        }
        #endregion
    }
}
