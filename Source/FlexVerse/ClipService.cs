// parents: @.frostline/graph/m-clip-service.xml
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Describes a rectangular native child clip in owner-local coordinates; nested clips intersect. (M-CLIP-SERVICE)</summary>
    public readonly struct ClipRegion
    {
        /// <summary>Gets the ancestor widget that owns the clip.</summary>
        public Widget Owner { get; }

        /// <summary>Gets the clip bounds in the owner's local coordinates.</summary>
        public Rect LocalBounds { get; }

        /// <summary>Gets the owner's self matrix before the child-space transform.</summary>
        public Matrix4x4 LocalToScreen { get; }

        internal ClipRegion(Widget owner, Rect bounds, Matrix4x4 localToScreen)
        {
            Owner = owner;
            LocalBounds = bounds;
            LocalToScreen = localToScreen;
        }
    }

    /// <summary>Answers read-only child clipping and point visibility queries over the published geometry. (M-CLIP-SERVICE)</summary>
    public sealed class ClipService
    {
        private readonly WidgetGeometrySnapshot _snapshot;
        private readonly TransformService _transforms;

        /// <summary>Binds the clip queries to a geometry snapshot and transform service.</summary>
        public ClipService(WidgetGeometrySnapshot snapshot, TransformService transforms)
        {
            _snapshot = snapshot;
            _transforms = transforms;
        }

        /// <summary>Child clip in owner-local coordinates.</summary>
        public Rect GetLocalChildrenClip(Widget widget) =>
            _snapshot.TryGet(widget, out var clip) ? clip.LocalClip : Rect.zero;

        /// <summary>Tests whether a screen point lies within the owner's child clip region. (REQ-001)</summary>
        public bool ContainsChildrenPoint(Widget owner, Vector2 screenPoint)
        {
            if (!_snapshot.TryGet(owner, out var clip))
                return false;
            if (!clip.ClipsChildren)
                return true;
            if (!_transforms.TryToLocal(owner, screenPoint, out var local))
                return false;

            return clip.LocalClip.Contains(local);
        }

        /// <summary>Tests a screen point against the intersection of all ancestor clips. (REQ-003)</summary>
        public bool IsVisible(Widget widget, Vector2 screenPoint)
        {
            if (!_snapshot.TryGet(widget, out var geometry))
                return false;

            for (var owner = geometry.Parent; owner != null; )
            {
                if (!ContainsChildrenPoint(owner, screenPoint))
                    return false;
                owner = _snapshot.TryGet(owner, out var ancestor) ? ancestor.Parent : null;
            }

            return true;
        }

        /// <summary>Returns the ordered ancestor clip stack, preserved as regions rather than a bounding box. (REQ-004)</summary>
        public IReadOnlyList<ClipRegion> GetClips(Widget widget)
        {
            var clips = new List<ClipRegion>();
            var owner = _snapshot.TryGet(widget, out var geometry) ? geometry.Parent : null;
            while (owner != null && _snapshot.TryGet(owner, out var ancestor))
            {
                if (ancestor.ClipsChildren)
                    clips.Add(new ClipRegion(owner, ancestor.LocalClip, ancestor.World));
                owner = ancestor.Parent;
            }
            clips.Reverse();
            return clips;
        }
    }
}
