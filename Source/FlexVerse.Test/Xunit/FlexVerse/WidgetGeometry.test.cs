// parents: @.frostline/graph/m-xunit.xml
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Xunit;

namespace FlexVerse.Test
{
    public class WidgetGeometryTests
    {
        public sealed class Probe : Widget { }

        [Fact]
        public void PublishedGeometryRetainsTransformsClipsAndPaintOrderUntilRefresh()
        {
            var engine = new WidgetEngine("snapshot_consistency");
            var tree = new WidgetTree(engine);
            var root = tree.CreateInstance(new Probe(), null);
            var first = tree.CreateInstance(new Probe(), root);
            var second = tree.CreateInstance(new Probe(), root);
            root.Overflow = Overflows.Hidden;
            var rects = new Dictionary<Widget, Rect>
            {
                [root] = new Rect(0, 0, 100, 100),
                [first] = new Rect(0, 0, 100, 100),
                [second] = new Rect(0, 0, 100, 100),
            };
            var padding = new Dictionary<Widget, Rect> { [root] = new Rect(0, 0, 50, 50) };
            var snapshot = new WidgetGeometrySnapshot();
            var builder = new WidgetGeometryBuilder(tree, rects, padding, snapshot);
            var transforms = new TransformService(snapshot);
            var clips = new ClipService(snapshot, transforms);
            var hits = new WidgetHitTester(snapshot, transforms, clips);
            builder.Refresh();
            Assert.True(snapshot.TryGet(root, out var previous));
            Assert.Same(second, hits.HitTest(new Vector2(10, 10)));

            first.ZIndex = 10;
            first.Translate = new Vector2(200, 0);
            root.Overflow = Overflows.Visible;
            Assert.Same(second, hits.HitTest(new Vector2(10, 10)));
            Assert.False(clips.ContainsChildrenPoint(root, new Vector2(80, 80)));
            Assert.Equal(Vector2.zero, transforms.ToScreen(first, Vector2.zero));

            builder.Refresh();
            Assert.Equal(new Vector2(200, 0), transforms.ToScreen(first, Vector2.zero));
            Assert.Same(first, hits.HitTest(new Vector2(210, 10)));
            Assert.True(clips.ContainsChildrenPoint(root, new Vector2(80, 80)));
            Assert.True(previous.ClipsChildren);
            Assert.Same(second, previous.PaintChildren[1]);

            tree.RemoveInstance(root);
            builder.Refresh();
            Assert.Null(snapshot.Root);
            Assert.False(snapshot.TryGet(first, out _));
            Assert.Null(hits.HitTest(new Vector2(210, 10)));
        }
    }
}
