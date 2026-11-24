// parents: @.frostline/graph/m-xunit.xml
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    [Collection("Renderer GUI")]
    public class ScalePivotTests
    {
        private sealed class Probe : Widget
        {
            public Matrix4x4 DrawMatrix;
            public Rect DrawnRect;

            public override void OnGUI(Rect rect)
            {
                DrawnRect = rect;
                DrawMatrix = GUI.matrix;
            }
        }

        private static Vector2 PivotOf(Matrix4x4 m)
        {
            var sx = new Vector2(m.m00, m.m10).magnitude;
            var sy = new Vector2(m.m01, m.m11).magnitude;
            var t = new Vector2(m.m03, m.m13);
            return new Vector2(t.x / (1f - sx), t.y / (1f - sy));
        }

        [Fact]
        public void ChildScalePivotIsLayoutCenter()
        {
            var tree = new WidgetTree(new WidgetEngine("scale_pivot"));
            var root = (Probe)tree.CreateInstance(new Probe(), null);
            var child = (Probe)tree.CreateInstance(new Probe(), root);

            child.Scale = new Vector2(2f, 2f);

            var snapshot = new WidgetGeometrySnapshot();
            new WidgetGeometryBuilder(
                tree,
                new Dictionary<Widget, Rect>
                {
                    [root] = new Rect(0, 0, 400, 300),
                    [child] = new Rect(200, 150, 100, 60),
                },
                new Dictionary<Widget, Rect>(),
                snapshot
            ).Refresh();

            try
            {
                new WidgetRenderer(snapshot, () => { }, null).RenderInstanceTree();

                var pivot = PivotOf(child.DrawMatrix);
                Assert.Equal(250f, pivot.x, 3f);
                Assert.Equal(180f, pivot.y, 3f);
            }
            finally
            {
                GUI.matrix = Matrix4x4.identity;
            }
        }
    }
}
