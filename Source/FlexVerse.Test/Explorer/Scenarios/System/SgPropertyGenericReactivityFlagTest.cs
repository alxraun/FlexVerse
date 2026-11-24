// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class SgPropertyGenericReactivityFlagTest : Test
    {
        [State]
        private Behavior<int> AbstractReconcileOnSetState { get; set; }

        [State]
        private Behavior<int> AbstractReconcileOnUpdateState { get; set; }

        [State]
        private Behavior<int> AbstractMarkDirtyOnSetState { get; set; }

        [State]
        private Behavior<int> AbstractMarkDirtyOnUpdateState { get; set; }

        [Prop]
        private Behavior<int> AbstractReconcileOnSetProp { get; set; }

        [Prop]
        private Behavior<int> AbstractReconcileOnUpdateProp { get; set; }

        [Prop]
        private Behavior<int> AbstractMarkDirtyOnSetProp { get; set; }

        [Prop]
        private Behavior<int> AbstractMarkDirtyOnUpdateProp { get; set; }

        [InheritedContext]
        private Behavior<int> AbstractReconcileOnSetInheritedContext { get; set; }

        [InheritedContext]
        private Behavior<int> AbstractReconcileOnUpdateInheritedContext { get; set; }

        [InheritedContext]
        private Behavior<int> AbstractMarkDirtyOnSetInheritedContext { get; set; }

        [InheritedContext]
        private Behavior<int> AbstractMarkDirtyOnUpdateInheritedContext { get; set; }

        [State]
        private TweenBehavior<int> ConcreteReconcileOnSetState { get; set; }

        [State]
        private TweenBehavior<int> ConcreteReconcileOnUpdateState { get; set; }

        [State]
        private TweenBehavior<int> ConcreteMarkDirtyOnSetState { get; set; }

        [State]
        private TweenBehavior<int> ConcreteMarkDirtyOnUpdateState { get; set; }

        [Prop]
        private TweenBehavior<int> ConcreteReconcileOnSetProp { get; set; }

        [Prop]
        private TweenBehavior<int> ConcreteReconcileOnUpdateProp { get; set; }

        [Prop]
        private TweenBehavior<int> ConcreteMarkDirtyOnSetProp { get; set; }

        [Prop]
        private TweenBehavior<int> ConcreteMarkDirtyOnUpdateProp { get; set; }

        [InheritedContext]
        private TweenBehavior<int> ConcreteReconcileOnSetInheritedContext { get; set; }

        [InheritedContext]
        private TweenBehavior<int> ConcreteReconcileOnUpdateInheritedContext { get; set; }

        [InheritedContext]
        private TweenBehavior<int> ConcreteMarkDirtyOnSetInheritedContext { get; set; }

        [InheritedContext]
        private TweenBehavior<int> ConcreteMarkDirtyOnUpdateInheritedContext { get; set; }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                var _ = AbstractMarkDirtyOnSetState;
                var __ = AbstractMarkDirtyOnUpdateState.Value;
                var ___ = AbstractMarkDirtyOnSetProp;
                var ____ = AbstractMarkDirtyOnUpdateProp.Value;
                var _____ = AbstractMarkDirtyOnSetInheritedContext;
                var ______ = AbstractMarkDirtyOnUpdateInheritedContext.Value;

                var _______ = ConcreteMarkDirtyOnSetState;
                var ________ = ConcreteMarkDirtyOnUpdateState.Value;
                var _________ = ConcreteMarkDirtyOnSetProp;
                var __________ = ConcreteMarkDirtyOnUpdateProp.Value;
                var ___________ = ConcreteMarkDirtyOnSetInheritedContext;
                var ____________ = ConcreteMarkDirtyOnUpdateInheritedContext.Value;

                return new Size(default, default);
            };

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SgpgafReceiver { Behavior = AbstractReconcileOnSetState },
                new SgpgafReceiver { Behavior = AbstractReconcileOnUpdateState.Value },
                new SgpgafReceiver { Behavior = AbstractReconcileOnSetProp },
                new SgpgafReceiver { Behavior = AbstractReconcileOnUpdateProp.Value },
                new SgpgafReceiver { Behavior = AbstractReconcileOnSetInheritedContext },
                new SgpgafReceiver { Behavior = AbstractReconcileOnUpdateInheritedContext.Value },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnSetState },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnUpdateState.Value },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnSetProp },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnUpdateProp.Value },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnSetInheritedContext },
                new SgpgafReceiver { Behavior = ConcreteReconcileOnUpdateInheritedContext.Value },
            ];

        public partial class SgpgafReceiver : Widget
        {
            [Prop]
            public Behavior<int> Behavior { get; set; }
        }
    }
}
