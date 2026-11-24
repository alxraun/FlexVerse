// parents: @.frostline/graph/m-canvas-view-widget.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse
{
    public partial class CanvasView : Widget
    {
        [Prop]
        public Behavior<Vector2> Pan { get; set; } = Vector2.zero;

        [Prop]
        public Behavior<float> Zoom { get; set; } = 1f;

        [Prop]
        public Behavior<float> MinZoom { get; set; } = 0.1f;

        [Prop]
        public Behavior<float> MaxZoom { get; set; } = 8f;

        private bool _isPanning;
        private int _panButton = -1;
        private Vector2 _lastPointerLocal;

        public CanvasView()
        {
            FlexDirection = FlexDirections.Column;
            FlexShrink = 0f;
            Overflow = Overflows.Hidden;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            Children.Value ?? Enumerable.Empty<Widget>();

        protected override Matrix4x4 GetChildrenTransform()
        {
            var pan = Pan.Value;
            var zoom = EffectiveZoom;
            return Matrix4x4.Translate(new Vector3(pan.x, pan.y, 0f))
                * Matrix4x4.Scale(new Vector3(zoom, zoom, 1f));
        }

        public override void OnMouseDown(FlexEvent e)
        {
            _isPanning = false;
            _panButton = -1;

            bool middleButton = e.Button == 2;
            bool emptyCanvasLeftButton = e.Button == 0 && ReferenceEquals(e.Target, this);

            if (
                (!middleButton && !emptyCanvasLeftButton)
                || HasScrollableDescendantOnPath(e.Target)
                || !Engine.TransformService.TryToLocal(this, e.MousePosition, out var local)
            )
                return;

            _isPanning = true;
            _panButton = e.Button;
            _lastPointerLocal = local;
            e.StopPropagation();
        }

        public override void OnMouseDrag(FlexEvent e)
        {
            if (!_isPanning || e.Button != _panButton)
                return;

            if (!Engine.TransformService.TryToLocal(this, e.MousePosition, out var local))
                return;

            Pan = Pan.Value + (local - _lastPointerLocal);
            _lastPointerLocal = local;
            e.StopPropagation();
        }

        public override void OnMouseUp(FlexEvent e)
        {
            if (!_isPanning || e.Button != _panButton)
                return;

            _isPanning = false;
            _panButton = -1;
            e.StopPropagation();
        }

        public override void OnMouseLeave(FlexEvent e)
        {
            _isPanning = false;
            _panButton = -1;
        }

        public override void OnScrollWheel(FlexEvent e)
        {
            if (HasScrollableDescendantOnPath(e.Target))
                return;

            if (!Engine.TransformService.TryToLocal(this, e.MousePosition, out var local))
                return;

            var oldZoom = EffectiveZoom;
            var factor = Mathf.Pow(1.1f, -e.ScrollDelta.y);
            if (float.IsNaN(factor) || float.IsInfinity(factor))
                return;

            var nextZoom = Mathf.Clamp(oldZoom * factor, EffectiveMinZoom, EffectiveMaxZoom);
            if (Mathf.Approximately(oldZoom, nextZoom))
                return;

            var nextPan = local - (local - Pan.Value) * (nextZoom / oldZoom);
            Pan = nextPan;
            Zoom = nextZoom;
            e.StopPropagation();
        }

        public override void OnUnmount()
        {
            _isPanning = false;
            _panButton = -1;
        }

        private bool HasScrollableDescendantOnPath(Widget target)
        {
            for (
                var widget = target;
                widget != null && !ReferenceEquals(widget, this);
                widget = Engine.GetParent(widget)
            )
            {
                if (widget.Overflow.Value == Overflows.Scroll)
                    return true;
            }

            return false;
        }

        private static float FinitePositive(float value, float fallback) =>
            !float.IsNaN(value) && !float.IsInfinity(value) && value > 0f ? value : fallback;

        private float EffectiveMinZoom => FinitePositive(MinZoom.Value, 0.1f);

        private float EffectiveMaxZoom =>
            Mathf.Max(EffectiveMinZoom, FinitePositive(MaxZoom.Value, 8f));

        private float EffectiveZoom
        {
            get
            {
                var zoom = Zoom.Value;
                if (float.IsNaN(zoom) || float.IsInfinity(zoom))
                    zoom = 1f;

                return Mathf.Clamp(zoom, EffectiveMinZoom, EffectiveMaxZoom);
            }
        }
    }
}
