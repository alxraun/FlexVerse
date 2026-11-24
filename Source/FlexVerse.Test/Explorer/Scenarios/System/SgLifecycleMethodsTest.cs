// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class SgLifecycleMethodsTest : Test
    {
        [Prop]
        private Behavior<int> GenericAbstractProp { get; set; }

        [Prop]
        private TweenBehavior<int> GenericConcreteProp { get; set; }

        [Prop]
        private Behavior NonGenericAbstractProp { get; set; }

        [Prop]
        private EffectBehavior NonGenericConcreteProp { get; set; }

        [State]
        private Behavior<int> GenericAbstractState { get; set; }

        [State]
        private TweenBehavior<int> GenericConcreteState { get; set; }

        [State]
        private Behavior NonGenericAbstractState { get; set; }

        [State]
        private EffectBehavior NonGenericConcreteState { get; set; }

        [InheritedContext]
        private Behavior<int> GenericAbstractInheritedContextProvider { get; set; } = 1;

        [InheritedContext]
        private TweenBehavior<int> GenericConcreteInheritedContextProvider { get; set; } =
            Animate.State(1);

        [InheritedContext]
        private Behavior NonGenericAbstractInheritedContextProvider { get; set; } = Effect.Create();

        [InheritedContext]
        private EffectBehavior NonGenericConcreteInheritedContextProvider { get; set; } =
            Effect.Create();

        [InheritedContext]
        private Behavior<int> GenericAbstractInheritedContextConsumer { get; set; }

        [InheritedContext]
        private TweenBehavior<int> GenericConcreteInheritedContextConsumer { get; set; }

        [InheritedContext]
        private Behavior NonGenericAbstractInheritedContextConsumer { get; set; }

        [InheritedContext]
        private EffectBehavior NonGenericConcreteInheritedContextConsumer { get; set; }
    }
}
