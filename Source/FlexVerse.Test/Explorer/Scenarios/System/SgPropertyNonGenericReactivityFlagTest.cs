// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class SgPropertyNonGenericReactivityFlagTest : Test
    {
        [State]
        private Behavior AbstractReconcileOnSetState { get; set; }

        [State]
        private Behavior AbstractMarkDirtyOnSetState { get; set; }

        [Prop]
        private Behavior AbstractReconcileOnSetProp { get; set; }

        [Prop]
        private Behavior AbstractMarkDirtyOnSetProp { get; set; }

        [InheritedContext]
        private Behavior AbstractReconcileOnSetInheritedContext { get; set; }

        [InheritedContext]
        private Behavior AbstractMarkDirtyOnSetInheritedContext { get; set; }

        [State]
        private EffectBehavior ConcreteReconcileOnSetState { get; set; }

        [State]
        private EffectBehavior ConcreteMarkDirtyOnSetState { get; set; }

        [Prop]
        private EffectBehavior ConcreteReconcileOnSetProp { get; set; }

        [Prop]
        private EffectBehavior ConcreteMarkDirtyOnSetProp { get; set; }

        [InheritedContext]
        private EffectBehavior ConcreteReconcileOnSetInheritedContext { get; set; }

        [InheritedContext]
        private EffectBehavior ConcreteMarkDirtyOnSetInheritedContext { get; set; }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                var _ = AbstractMarkDirtyOnSetState;
                var __ = AbstractMarkDirtyOnSetProp;
                var ___ = AbstractMarkDirtyOnSetInheritedContext;

                var ____ = ConcreteMarkDirtyOnSetState;
                var _____ = ConcreteMarkDirtyOnSetProp;
                var ______ = ConcreteMarkDirtyOnSetInheritedContext;

                return new Size(default, default);
            };

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new SgpngafReceiver { Behavior = AbstractReconcileOnSetState },
                new SgpngafReceiver { Behavior = AbstractReconcileOnSetProp },
                new SgpngafReceiver { Behavior = AbstractReconcileOnSetInheritedContext },
                new SgpngafReceiver { Behavior = ConcreteReconcileOnSetState },
                new SgpngafReceiver { Behavior = ConcreteReconcileOnSetProp },
                new SgpngafReceiver { Behavior = ConcreteReconcileOnSetInheritedContext },
            ];

        public partial class SgpngafReceiver : Widget
        {
            [Prop]
            public Behavior Behavior { get; set; }
        }
    }
}
