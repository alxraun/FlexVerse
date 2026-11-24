// parents: @.frostline/graph/m-flex-events.xml
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Targets widgets from screen points over published geometry and clips. (M-FLEX-EVENTS)</summary>
    public sealed class WidgetHitTester
    {
        private readonly WidgetGeometrySnapshot _snapshot;
        private readonly TransformService _transforms;
        private readonly ClipService _clips;

        /// <summary>Binds the hit tester to geometry, transforms and clips.</summary>
        public WidgetHitTester(
            WidgetGeometrySnapshot snapshot,
            TransformService transforms,
            ClipService clips
        )
        {
            _snapshot = snapshot;
            _transforms = transforms;
            _clips = clips;
        }

        /// <summary>Returns the deepest widget hit by a screen point. (REQ-001)</summary>
        public Widget HitTest(Vector2 screenPoint)
        {
            var root = _snapshot.Root;
            return root == null ? null : FindHit(root, screenPoint);
        }

        private Widget FindHit(Widget widget, Vector2 screenPoint)
        {
            if (
                !_snapshot.TryGet(widget, out var geometry)
                || !_transforms.TryToLocal(widget, screenPoint, out var local)
            )
                return null;

            var inside = new Rect(Vector2.zero, geometry.LayoutRect.size).Contains(local);

            if (!_clips.ContainsChildrenPoint(widget, screenPoint))
                return inside ? widget : null;

            return FindChildHit(widget, screenPoint) ?? (inside ? widget : null);
        }

        private Widget FindChildHit(Widget widget, Vector2 screenPoint)
        {
            var children = _snapshot.TryGet(widget, out var geometry)
                ? geometry.PaintChildren
                : System.Array.Empty<Widget>();
            for (var i = children.Count - 1; i >= 0; i--)
            {
                var child = children[i];
                if (child == null)
                    continue;

                var hit = FindHit(child, screenPoint);
                if (hit != null)
                    return hit;
            }

            return null;
        }
    }
}
