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
    /// Core tests for WidgetEngine key identity and idempotent teardown
    ///.
    /// </summary>
    public class WidgetEngineTests
    {
        [Fact]
        public void Constructor_ExplicitKey_AssignsKeyProperty()
        {
            var engine = new WidgetEngine("test_key");

            Assert.Equal("test_key", engine.Key);
        }

        [Fact]
        public void Constructor_Parameterless_GeneratesAutoKey()
        {
            var engine = new WidgetEngine();

            Assert.NotNull(engine.Key);
            Assert.StartsWith("auto:", engine.Key, StringComparison.Ordinal);
            Assert.True(engine.Key.Length > "auto:".Length);
        }

        [Fact]
        public void Constructor_InvalidKey_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new WidgetEngine(null));
            Assert.Throws<ArgumentException>(() => new WidgetEngine("   "));
        }

        [Fact]
        public void Shutdown_IsIdempotent()
        {
            var engine = new WidgetEngine("test_key");

            engine.Shutdown();
            engine.Shutdown();
            engine.Shutdown();

            Assert.Null(engine.GetRoot());
        }
    }

    /// <summary>
    /// R03: the widget owns native matrix scopes; WidgetRenderer is only the
    /// recursive traversal. These fixtures are intentionally not run by the
    /// implementation agent: a separate verification agent runs the suite.
    /// </summary>
    [Collection("Renderer GUI")]
    public class WidgetRenderWalkTests
    {
        private static StaticBehavior<WidgetList> MakeChildren(WidgetList list)
        {
            var behavior = new StaticBehavior<WidgetList>();
            behavior.Value = list;
            behavior.Target = list;
            return behavior;
        }

        private sealed class RecordingConfig
        {
            public readonly List<string> Calls;
            public readonly Vector2 ChildOffset;
            public readonly bool ThrowOnDraw;

            public RecordingConfig(List<string> calls, Vector2 childOffset, bool throwOnDraw)
            {
                Calls = calls;
                ChildOffset = childOffset;
                ThrowOnDraw = throwOnDraw;
            }
        }

        private sealed class RecordingWidget : Widget
        {
            private RecordingConfig Config => (RecordingConfig)Key;

            public RecordingWidget Configure(
                List<string> calls,
                Vector2 childOffset = default,
                bool throwOnDraw = false
            )
            {
                Key = new RecordingConfig(calls, childOffset, throwOnDraw);
                return this;
            }

            public override IEnumerable<Widget> BuildRenderTree() =>
                Children.Value ?? Enumerable.Empty<Widget>();

            protected override Matrix4x4 GetChildrenTransform()
            {
                var offset = Config.ChildOffset;
                return Matrix4x4.Translate(new Vector3(offset.x, offset.y, 0f));
            }

            public override void OnGUI(Rect rect)
            {
                var p = GUI.matrix.MultiplyPoint3x4(new Vector3(rect.x, rect.y, 0f));
                Config.Calls.Add($"gui:{p.x:0.##},{p.y:0.##}");
                if (Config.ThrowOnDraw)
                    throw new InvalidOperationException("test render failure");
            }

            public override ChildrenGuiSpace BeginChildrenGUI(Rect rect, Rect viewport)
            {
                Config.Calls.Add("begin");
                return default;
            }

            public override void EndChildrenGUI() => Config.Calls.Add("end");

            public override Widget Clone() =>
                new RecordingWidget
                {
                    Key = Key,
                    Width = Width,
                    Height = Height,
                    Overflow = Overflow,
                    ZIndex = ZIndex,
                    PositionType = PositionType,
                    PositionLeft = PositionLeft,
                    Translate = Translate,
                    Scale = Scale,
                    Rotate = Rotate,
                    TransformOrigin = TransformOrigin,
                    Children = MakeChildren(
                        Children.Value != default
                            ? new WidgetList(Children.Value)
                            : new WidgetList()
                    ),
                };
        }

        private static string[] CallKinds(IReadOnlyList<string> calls) =>
            calls.Select(c => (c == "begin" || c == "end") ? c : "gui").ToArray();

        [Fact]
        public void RenderWalk_BracketsChildDrawing()
        {
            GUI.matrix = Matrix4x4.identity;
            var calls = new List<string>();
            var engine = new WidgetEngine("r03_render_order");
            var parent = new RecordingWidget().Configure(calls);
            var child = new RecordingWidget().Configure(calls);
            parent.Width = 100.Point();
            parent.Height = 100.Point();
            child.Width = 30.Point();
            child.Height = 20.Point();
            parent.Children = MakeChildren(new WidgetList { child });

            engine.ProcessUI(parent, new Rect(0, 0, 100, 100));

            Assert.Equal(new[] { "gui", "begin", "gui", "begin", "end", "end" }, CallKinds(calls));
        }

        [Fact]
        public void RenderWalk_ChildSpaceMatrixAppliesToDescendants()
        {
            GUI.matrix = Matrix4x4.identity;
            var calls = new List<string>();
            var engine = new WidgetEngine("r03_children_matrix");
            var parent = new RecordingWidget().Configure(calls, new Vector2(10, 20));
            var child = new RecordingWidget().Configure(calls);
            parent.Width = 100.Point();
            parent.Height = 100.Point();
            child.Width = 30.Point();
            child.Height = 20.Point();
            parent.Children = MakeChildren(new WidgetList { child });

            engine.ProcessUI(parent, new Rect(0, 0, 100, 100));

            Assert.Equal("gui:10,20", calls[2]);
            Assert.Equal(Matrix4x4.identity, GUI.matrix);
        }

        [Fact]
        public void RenderWalk_ComposesNestedChildMatrices()
        {
            GUI.matrix = Matrix4x4.identity;
            var calls = new List<string>();
            var engine = new WidgetEngine("r03_nested_matrices");
            var parent = new RecordingWidget().Configure(calls, new Vector2(10, 20));
            var child = new RecordingWidget().Configure(calls, new Vector2(5, 7));
            var grandchild = new RecordingWidget().Configure(calls);
            parent.Width = 100.Point();
            parent.Height = 100.Point();
            child.Width = 30.Point();
            child.Height = 20.Point();
            grandchild.Width = 10.Point();
            grandchild.Height = 10.Point();
            child.Children = MakeChildren(new WidgetList { grandchild });
            parent.Children = MakeChildren(new WidgetList { child });

            engine.ProcessUI(parent, new Rect(0, 0, 100, 100));

            Assert.Contains("gui:15,27", calls);
        }

        [Fact]
        public void RenderWalk_RestoresScopesAfterChildThrows()
        {
            GUI.matrix = Matrix4x4.identity;
            var calls = new List<string>();
            var engine = new WidgetEngine("r03_exception_scope");
            var parent = new RecordingWidget().Configure(calls);
            var child = new RecordingWidget().Configure(calls, throwOnDraw: true);
            parent.Width = 100.Point();
            parent.Height = 100.Point();
            child.Width = 30.Point();
            child.Height = 20.Point();
            parent.Children = MakeChildren(new WidgetList { child });

            engine.ProcessUI(parent, new Rect(0, 0, 100, 100));

            Assert.Equal("end", calls.Last());
            Assert.Equal(Matrix4x4.identity, GUI.matrix);
        }

        [Fact]
        public void TransformService_NestedRotateAndScaleRoundTrip()
        {
            GUI.matrix = Matrix4x4.identity;
            var engine = new WidgetEngine("r03_roundtrip");
            var parent = new RecordingWidget().Configure(new List<string>());
            parent.Width = 100.Point();
            parent.Height = 80.Point();
            parent.Translate = new Vector2(15f, -5f);
            parent.Scale = new Vector2(-2f, 1.5f);
            parent.Rotate = 25f;
            parent.TransformOrigin = new Vector2(0.3f, 0.7f);

            engine.ProcessUI(parent, new Rect(0, 0, 100, 80));

            var instance = engine.GetRoot();
            var local = new Vector2(24f, 35f);
            var screen = engine.TransformService.ToScreen(instance, local);
            Assert.True(engine.TransformService.TryToLocal(instance, screen, out var inverse));
            Assert.InRange(Mathf.Abs(inverse.x - local.x), 0f, 0.002f);
            Assert.InRange(Mathf.Abs(inverse.y - local.y), 0f, 0.002f);
        }

        [Fact]
        public void TransformService_VisibleOverflowChildIsHitOutsideParentRect()
        {
            var engine = new WidgetEngine("r03_visible_overflow");
            var tree = new WidgetTree(engine);
            var parent = tree.CreateInstance(
                new RecordingWidget().Configure(new List<string>()),
                null
            );
            var child = tree.CreateInstance(
                new RecordingWidget().Configure(new List<string>()),
                parent
            );
            var rects = new Dictionary<Widget, Rect>
            {
                [parent] = new Rect(0f, 0f, 20f, 20f),
                [child] = new Rect(30f, 0f, 10f, 10f),
            };
            var snapshot = new WidgetGeometrySnapshot();
            new WidgetGeometryBuilder(
                tree,
                rects,
                new Dictionary<Widget, Rect>(),
                snapshot
            ).Refresh();
            var transforms = new TransformService(snapshot);

            Assert.Same(
                child,
                new WidgetHitTester(
                    snapshot,
                    transforms,
                    new ClipService(snapshot, transforms)
                ).HitTest(new Vector2(35f, 5f))
            );
        }

        [Fact]
        public void TransformService_ZeroScaleWidgetCannotReceivePointer()
        {
            GUI.matrix = Matrix4x4.identity;
            var engine = new WidgetEngine("r03_zero_scale");
            var root = new RecordingWidget().Configure(new List<string>());
            root.Width = 100.Point();
            root.Height = 100.Point();
            root.Scale = Vector2.zero;

            engine.ProcessUI(root, new Rect(0, 0, 100, 100));

            Assert.Null(engine.HitTester.HitTest(new Vector2(50, 50)));
        }
    }
}
