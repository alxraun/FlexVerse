// parents: @.frostline/graph/m-xunit.xml
using System;
using System.Collections.Generic;
using System.Reflection;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    [CollectionDefinition("Renderer GUI", DisableParallelization = true)]
    public class RendererGuiCollection { }

    [Collection("Renderer GUI")]
    public class WidgetRendererTests
    {
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ChildScaleUsesUnclippedPivotInsideHostAndWidgetGroups(bool nativeClip)
        {
            var tree = new WidgetTree(new WidgetEngine("renderer_group_pivot"));
            var root = (Probe)tree.CreateInstance(new Probe(), null);
            var child = (Probe)tree.CreateInstance(new Probe(), root);
            root.NativeClip = nativeClip;
            root.Overflow = Overflows.Hidden;
            child.Scale = new Vector2(2f, 2f);
            var center = default(Vector3);
            child.Draw = rect =>
            {
                var matrix = GUI.matrix;
                GUI.matrix = Matrix4x4.identity;
                var unclipped = GUIUtility.GUIToScreenPoint(rect.center);
                GUI.matrix = matrix;
                center = matrix.MultiplyPoint3x4(new Vector3(unclipped.x, unclipped.y, 0f));
            };
            var snapshot = new WidgetGeometrySnapshot();
            new WidgetGeometryBuilder(
                tree,
                new Dictionary<Widget, Rect>
                {
                    [root] = new Rect(20, 30, 100, 100),
                    [child] = new Rect(26, 38, 10, 10),
                },
                new Dictionary<Widget, Rect> { [root] = new Rect(5, 7, 90, 80) },
                snapshot
            ).Refresh();

            var originalMatrix = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.Scale(new Vector3(1.5f, 1.5f, 1f));
                GUI.BeginGroup(new Rect(200, 50, 500, 400));
                try
                {
                    new WidgetRenderer(snapshot, () => { }, null).RenderInstanceTree();

                    Assert.Equal(346.5f, center.x, 3f);
                    Assert.Equal(139.5f, center.y, 3f);
                }
                finally
                {
                    GUI.EndGroup();
                }
            }
            finally
            {
                GUI.matrix = originalMatrix;
            }
        }

        public sealed class Probe : Widget
        {
            public bool NativeClip;
            public bool TransformChildren;

            protected override Matrix4x4 GetChildrenTransform() =>
                TransformChildren ? Matrix4x4.Translate(new Vector3(9, 0, 0)) : Matrix4x4.identity;

            public Action<Rect>? Draw;
            public Rect Viewport;
            public int Ends;

            public override void OnGUI(Rect rect) => Draw?.Invoke(rect);

            public override ChildrenGuiSpace BeginChildrenGUI(Rect rect, Rect viewport)
            {
                Viewport = viewport;
                if (!NativeClip)
                    return default;
                GUI.BeginGroup(viewport);
                if (TransformChildren)
                    GUI.matrix *= GetChildrenTransform();
                return new ChildrenGuiSpace(viewport.position, TransformChildren, true);
            }

            public override void EndChildrenGUI()
            {
                Ends++;
                if (NativeClip)
                    GUI.EndGroup();
            }
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, false, false)]
        [InlineData(true, true, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, true)]
        [InlineData(true, false, true)]
        [InlineData(true, true, true)]
        public void ExplicitNativeSpaceAndStandardClipShareViewportAndRestoreScopes(
            bool nativeClip,
            bool throwOnChild,
            bool transformChildren
        )
        {
            var originalMatrix = Matrix4x4.Translate(new Vector3(3, 4, 0));
            GUI.matrix = originalMatrix;
            var depth = typeof(GUI).GetField(
                "_groupDepth",
                BindingFlags.NonPublic | BindingFlags.Static
            );
            Assert.NotNull(depth);
            var initialDepth = (int)depth.GetValue(null)!;
            var tree = new WidgetTree(new WidgetEngine("renderer_scope"));
            var root = (Probe)tree.CreateInstance(new Probe(), null);
            var child = (Probe)tree.CreateInstance(new Probe(), root);
            root.Overflow = Overflows.Hidden;
            root.NativeClip = nativeClip;
            root.TransformChildren = transformChildren;
            var childDraws = 0;
            var drawnRect = Rect.zero;
            var drawMatrix = Matrix4x4.identity;
            var drawDepth = -1;
            child.Draw = rect =>
            {
                childDraws++;
                drawnRect = rect;
                drawMatrix = GUI.matrix;
                drawDepth = (int)depth.GetValue(null)!;
                if (throwOnChild)
                    throw new InvalidOperationException("child probe");
            };
            var snapshot = new WidgetGeometrySnapshot();
            new WidgetGeometryBuilder(
                tree,
                new Dictionary<Widget, Rect>
                {
                    [root] = new Rect(20, 30, 100, 100),
                    [child] = new Rect(26, 38, 10, 10),
                },
                new Dictionary<Widget, Rect> { [root] = new Rect(5, 7, 90, 80) },
                snapshot
            ).Refresh();

            try
            {
                new WidgetRenderer(
                    snapshot,
                    () => throw new InvalidOperationException("unexpected refresh"),
                    null
                ).RenderInstanceTree();
                Assert.Equal(1, childDraws);
                Assert.Equal(new Rect(1, 1, 10, 10), drawnRect);
                Assert.Equal(initialDepth + 1, drawDepth);
                Assert.Equal(
                    originalMatrix
                        * (
                            transformChildren
                                ? Matrix4x4.Translate(new Vector3(9, 0, 0))
                                : Matrix4x4.identity
                        ),
                    drawMatrix
                );
                Assert.Equal(new Rect(25, 37, 90, 80), root.Viewport);
                Assert.Equal(1, root.Ends);
                Assert.Equal(initialDepth, (int)depth.GetValue(null)!);
                Assert.Equal(originalMatrix, GUI.matrix);
            }
            finally
            {
                GUI.matrix = Matrix4x4.identity;
            }
        }
    }
}
