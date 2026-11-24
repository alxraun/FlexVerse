// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// R05: sibling-local ZIndex is one paint-order contract used by both
    /// WidgetRenderer and WidgetHitTester through the shared geometry snapshot.
    /// </summary>
    [Collection("Renderer GUI")]
    public class ZIndexTests
    {
        private sealed class LayerProbe : Widget
        {
            public LayerProbe() { }

            public string Name { get; set; } = "";
            public List<string> DrawLog { get; set; } = new();

            public override void InitMembersFromDefinition(Widget definition)
            {
                base.InitMembersFromDefinition(definition);
                var probe = (LayerProbe)definition;
                Name = probe.Name;
                DrawLog = probe.DrawLog;
            }

            public override IEnumerable<Widget> BuildRenderTree() =>
                Children.Value ?? Enumerable.Empty<Widget>();

            public override void OnGUI(Rect rect) => DrawLog?.Add(Name);

            public override Widget Clone() =>
                new LayerProbe
                {
                    Key = Key,
                    Name = Name,
                    DrawLog = DrawLog,
                    ZIndex = ZIndex,
                    Width = Width,
                    Height = Height,
                    FlexShrink = FlexShrink,
                    Overflow = Overflow,
                    PositionType = PositionType,
                    PositionLeft = PositionLeft,
                    PositionTop = PositionTop,
                    Translate = Translate,
                    Scale = Scale,
                    Rotate = Rotate,
                    TransformOrigin = TransformOrigin,
                    Children = Children.Clone(),
                };
        }

        private static LayerProbe Root(List<string> log, params Widget[] children) =>
            new LayerProbe
            {
                Key = "root",
                Name = "root",
                DrawLog = log,
                Width = 100.Point(),
                Height = 100.Point(),
                Children = new WidgetList(children),
            };

        private static LayerProbe Layer(
            string name,
            List<string> log,
            int z,
            float width = 80f,
            float height = 80f
        ) =>
            new LayerProbe
            {
                Key = name,
                Name = name,
                DrawLog = log,
                ZIndex = z,
                Width = width.Point(),
                Height = height.Point(),
                PositionType = PositionTypes.Absolute,
                PositionLeft = 0.Point(),
                PositionTop = 0.Point(),
            };

        [Fact]
        public void Siblings_PaintAscendingAndHitDescendingWithoutReorderingChildren()
        {
            var log = new List<string>();
            var high = Layer("high", log, 20);
            var low = Layer("low", log, 5);
            var middle = Layer("middle", log, 10);
            var definition = Root(log, high, low, middle);
            var engine = new WidgetEngine("r05_sibling_order");

            engine.ProcessUI(definition, new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "root", "low", "middle", "high" }, log);
            var root = engine.GetRoot();
            Assert.Equal(
                new[] { "high", "low", "middle" },
                engine.GetChildren(root).Select(child => child.Key as string).ToArray()
            );
            Assert.Equal("high", engine.HitTester.HitTest(new Vector2(10f, 10f))?.Key);
        }

        [Fact]
        public void EqualZIndex_TiesKeepDeclarationOrderIncludingNegativeValues()
        {
            var log = new List<string>();
            var first = Layer("first", log, 0);
            var second = Layer("second", log, -3);
            var third = Layer("third", log, 0);
            var fourth = Layer("fourth", log, 0);
            var engine = new WidgetEngine("r05_stable_ties");

            engine.ProcessUI(Root(log, first, second, third, fourth), new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "root", "second", "first", "third", "fourth" }, log);
            Assert.Equal("fourth", engine.HitTester.HitTest(new Vector2(10f, 10f))?.Key);
        }

        [Fact]
        public void UpdatingInstanceZIndex_ChangesDrawAndHitWithoutFlexboxOrReconciliation()
        {
            var log = new List<string>();
            var first = Layer("first", log, 5);
            var second = Layer("second", log, 10);
            var definition = Root(log, first, second);
            var engine = new WidgetEngine("r05_dynamic");
            engine.ProcessUI(definition, new Rect(0, 0, 100, 100));

            var root = engine.GetRoot();
            var firstInstance = engine.GetChildren(root)[0];
            var secondInstance = engine.GetChildren(root)[1];

            firstInstance.ZIndex = 20;
            log.Clear();
            engine.ProcessUI(definition, new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "root", "second", "first" }, log);
            Assert.False(engine.DidLayoutCalculationHappenThisCycle);
            Assert.Same(root, engine.GetRoot());
            Assert.Same(firstInstance, engine.GetChildren(root)[0]);
            Assert.Same(secondInstance, engine.GetChildren(root)[1]);
            Assert.Same(firstInstance, engine.HitTester.HitTest(new Vector2(10f, 10f)));

            firstInstance.ZIndex = 10;
            log.Clear();
            engine.ProcessUI(definition, new Rect(0, 0, 100, 100));
            Assert.Equal(new[] { "root", "first", "second" }, log);
            Assert.Same(secondInstance, engine.HitTester.HitTest(new Vector2(10f, 10f)));
            Assert.False(engine.DidLayoutCalculationHappenThisCycle);
        }

        [Fact]
        public void SubtreeOrder_IsAtomicAcrossSiblingBoundaries()
        {
            var log = new List<string>();
            var innerHigh = Layer("inner-high", log, 100);
            var lowParent = Layer("low-parent", log, 1);
            lowParent.Children = new WidgetList { innerHigh };

            var highParent = Layer("high-parent", log, 10);
            var engine = new WidgetEngine("r05_subtree");
            engine.ProcessUI(Root(log, highParent, lowParent), new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "root", "low-parent", "inner-high", "high-parent" }, log);
            Assert.Equal("high-parent", engine.HitTester.HitTest(new Vector2(10f, 10f))?.Key);
        }

        [Fact]
        public void HighChildIndex_DoesNotEscapeAncestorClip()
        {
            var log = new List<string>();
            var child = Layer("clipped-child", log, 100, width: 80f);
            var parent = Layer("clipped-parent", log, 10, width: 30f);
            parent.Overflow = Overflows.Hidden;
            parent.Children = new WidgetList { child };

            var behind = Layer("behind", log, -1);
            var engine = new WidgetEngine("r05_clip");
            engine.ProcessUI(Root(log, parent, behind), new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "root", "behind", "clipped-parent", "clipped-child" }, log);
            Assert.Equal("clipped-child", engine.HitTester.HitTest(new Vector2(20f, 10f))?.Key);
            Assert.Equal("behind", engine.HitTester.HitTest(new Vector2(50f, 10f))?.Key);
            Assert.Equal(Matrix4x4.identity, GUI.matrix);
        }
    }
}
