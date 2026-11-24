// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Drawing and native input only. Scene owns observations and route snapshots;
    /// plot owns the shared world/screen transform and cached axis labels.
    /// </summary>
    public partial class SequenceCanvas : Widget
    {
        [Prop]
        public Behavior<SequenceScene> Scene { get; set; }

        [Prop]
        public Behavior<Vector2> Min { get; set; } = Vector2.zero;

        [Prop]
        public Behavior<Vector2> Max { get; set; } = new Vector2(16f, 12f);

        [Prop]
        public Behavior<bool> ShowPrevious { get; set; } = true;

        [Prop]
        public Behavior<bool> ShowTrail { get; set; } = true;

        private readonly SequencePlot _plot = new();
        private SequenceScene _sceneModel;
        private Rect _canvas;
        private bool _dragging;
        private bool _historyCaptured;

        public SequenceCanvas()
        {
            Width = 100f.Percent();
            FlexGrow = 1;
            MinHeight = 260f.Point();
        }

        public override void OnMount()
        {
            base.OnMount();
            _sceneModel = Scene?.Value;
            Application.onBeforeRender += CaptureFrame;
        }

        public override void OnUnmount()
        {
            Application.onBeforeRender -= CaptureFrame;
            base.OnUnmount();
        }

        private void CaptureFrame()
        {
            _sceneModel?.Capture();
            if (_canvas.width > 0f && _canvas.height > 0f)
                _plot.Update(_canvas, Min.Value, Max.Value);
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (
                e.Button != 0
                || !_plot.Ready
                || !_plot.Bounds.Contains(e.MousePosition)
                || _sceneModel is not { Ready: true }
            )
                return;
            _dragging = true;
            _historyCaptured = false;
            HandlePointerRetarget(e.MousePosition);
            e.StopPropagation();
            e.NativeEvent.Use();
        }

        public override void OnMouseDrag(FlexEvent e)
        {
            if (!_dragging || e.Button != 0 || !_plot.Bounds.Contains(e.MousePosition))
                return;
            HandlePointerRetarget(e.MousePosition);
            e.StopPropagation();
            e.NativeEvent.Use();
        }

        private void HandlePointerRetarget(Vector2 position)
        {
            var target = _plot.FromScreen(position);
            if (_sceneModel.Target == target)
                return;
            _sceneModel.Retarget(target, !_historyCaptured);
            _historyCaptured = true;
        }

        public override void OnMouseUp(FlexEvent e) => _dragging = false;

        public override void OnMouseLeave(FlexEvent e) => _dragging = false;

        public override void OnGUI(Rect rect)
        {
            _canvas = rect;
            var color = GUI.color;
            var font = Text.Font;
            var anchor = Text.Anchor;
            var wrap = Text.WordWrap;
            try
            {
                GUI.color = Color.white;
                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperLeft;
                Text.WordWrap = false;
                Widgets.DrawBoxSolid(rect, SequenceStyle.Canvas);
                if (_sceneModel is not { Ready: true })
                {
                    GUI.color = SequenceStyle.Warning;
                    Widgets.Label(rect, "Sequence geometry unavailable");
                    return;
                }
                if (!_plot.Ready)
                    return;

                DrawFrame(rect);
                GUI.BeginGroup(_plot.Bounds);
                try
                {
                    DrawGrid();
                    if (ShowPrevious.Value)
                        DrawPrevious();
                    if (_sceneModel.HasRetargeted)
                        DrawSegment(
                            _sceneModel.Entry,
                            _sceneModel.Target,
                            SequenceStyle.Corridor,
                            true
                        );
                    if (ShowTrail.Value)
                        DrawTrail();
                    DrawRemaining();
                    DrawVertices();
                    DrawEntry();
                    DrawTarget();
                    DrawValue();
                }
                finally
                {
                    GUI.EndGroup();
                }
            }
            finally
            {
                GUI.color = color;
                Text.Font = font;
                Text.Anchor = anchor;
                Text.WordWrap = wrap;
            }
        }

        private void DrawFrame(Rect rect)
        {
            GUI.color = SequenceStyle.Text;
            Widgets.Label(
                new Rect(rect.x + 14f, rect.y + 10f, rect.width - 28f, 22f),
                _sceneModel.Readout
            );
            GUI.color = SequenceStyle.Border;
            Widgets.DrawBox(_plot.Bounds, 1);
            GUI.color = SequenceStyle.Muted;
            Widgets.Label(
                new Rect(rect.x + 14f, rect.yMax - 24f, rect.width - 28f, 20f),
                "Click / drag to retarget  ·  square world units  ·  previous plan fades after 8s"
            );
            DrawAxes();
        }

        private void DrawAxes()
        {
            GUI.color = SequenceStyle.Muted;
            Text.Anchor = TextAnchor.UpperCenter;
            foreach (var tick in _plot.X)
            {
                var x = _plot.Bounds.x + _plot.ToLocal(new Vector2(tick.Value, 0f)).x;
                Widgets.Label(new Rect(x - 20f, _plot.Bounds.yMax + 3f, 40f, 18f), tick.Label);
            }
            Text.Anchor = TextAnchor.MiddleRight;
            foreach (var tick in _plot.Y)
            {
                var y = _plot.Bounds.y + _plot.ToLocal(new Vector2(0f, tick.Value)).y;
                Widgets.Label(new Rect(_plot.Bounds.x - 36f, y - 9f, 28f, 18f), tick.Label);
            }
            Text.Anchor = TextAnchor.UpperLeft;
        }

        private void DrawGrid()
        {
            foreach (var tick in _plot.X)
            {
                var x = _plot.ToLocal(new Vector2(tick.Value, 0f)).x;
                Widgets.DrawLine(
                    new Vector2(x, 0f),
                    new Vector2(x, _plot.Bounds.height),
                    tick.Value == 0f ? SequenceStyle.Axis : SequenceStyle.Grid,
                    1f
                );
            }
            foreach (var tick in _plot.Y)
            {
                var y = _plot.ToLocal(new Vector2(0f, tick.Value)).y;
                Widgets.DrawLine(
                    new Vector2(0f, y),
                    new Vector2(_plot.Bounds.width, y),
                    tick.Value == 0f ? SequenceStyle.Axis : SequenceStyle.Grid,
                    1f
                );
            }
        }

        private void DrawPrevious()
        {
            var color = SequenceStyle.Previous;
            color.a *= Mathf.Clamp01(
                (SequenceScene.HistoryLifetime - (Time.unscaledTime - _sceneModel.PreviousTime))
                    / 2f
            );
            if (color.a <= 0f)
                return;
            for (int i = 1; i < _sceneModel.PreviousCount; i++)
                DrawSegment(_sceneModel.Previous[i - 1], _sceneModel.Previous[i], color, true);
        }

        private void DrawRemaining()
        {
            if (_sceneModel.CurrentWaypoint < 0)
                return;
            var previous = _sceneModel.Value;
            for (int i = _sceneModel.CurrentWaypoint; i < _sceneModel.Live.Count; i++)
            {
                DrawSegment(previous, _sceneModel.Live[i], SequenceStyle.Accent, false);
                previous = _sceneModel.Live[i];
            }
        }

        private void DrawTrail()
        {
            for (int i = 0; i < _sceneModel.TrailCount; i++)
            {
                var index =
                    (_sceneModel.TrailHead + SequenceScene.TrailCapacity - 1 - i)
                    % SequenceScene.TrailCapacity;
                var age = Time.unscaledTime - _sceneModel.TrailTimes[index];
                if (age >= SequenceScene.TrailLifetime)
                    continue;
                var point = _plot.ToLocal(_sceneModel.Trail[index]);
                var color = SequenceStyle.Value;
                color.a = 0.5f * (1f - age / SequenceScene.TrailLifetime);
                Widgets.DrawBoxSolid(new Rect(point.x - 1.5f, point.y - 1.5f, 3f, 3f), color);
            }
        }

        private void DrawVertices()
        {
            for (int i = 0; i < _sceneModel.Live.Count; i++)
            {
                var point = _plot.ToLocal(_sceneModel.Live[i]);
                bool current = i == _sceneModel.CurrentWaypoint;
                var color = current ? SequenceStyle.Accent : SequenceStyle.Waypoint;
                var marker = new Rect(point.x - 4f, point.y - 4f, 8f, 8f);
                Widgets.DrawBoxSolid(marker, SequenceStyle.Canvas);
                GUI.color = color;
                Widgets.DrawBox(marker, 1);
                if (current)
                {
                    GUI.color = SequenceStyle.Text;
                    Widgets.DrawBox(new Rect(point.x - 7f, point.y - 7f, 14f, 14f), 1);
                }
                Widgets.Label(
                    new Rect(point.x + 9f, point.y - 11f, 28f, 22f),
                    _sceneModel.WaypointLabels[i]
                );
            }
        }

        private void DrawEntry()
        {
            var point = _plot.ToLocal(_sceneModel.Entry);
            GUI.color = SequenceStyle.Muted;
            Widgets.DrawBox(new Rect(point.x - 3f, point.y - 3f, 6f, 6f), 1);
        }

        private void DrawTarget()
        {
            var point = _plot.ToLocal(_sceneModel.Target);
            Widgets.DrawLine(
                point - new Vector2(9f, 0f),
                point + new Vector2(9f, 0f),
                SequenceStyle.Success,
                1.5f
            );
            Widgets.DrawLine(
                point - new Vector2(0f, 9f),
                point + new Vector2(0f, 9f),
                SequenceStyle.Success,
                1.5f
            );
            GUI.color = SequenceStyle.Success;
            Widgets.DrawBox(new Rect(point.x - 5f, point.y - 5f, 10f, 10f), 1);
        }

        private void DrawValue()
        {
            var point = _plot.ToLocal(_sceneModel.Value);
            Widgets.DrawBoxSolid(new Rect(point.x - 4f, point.y - 4f, 8f, 8f), SequenceStyle.Value);
        }

        private void DrawSegment(Vector2 from, Vector2 to, Color color, bool dashed)
        {
            var a = _plot.ToLocal(from);
            var b = _plot.ToLocal(to);
            if (!dashed)
            {
                Widgets.DrawLine(a, b, color, 2f);
                return;
            }
            var delta = b - a;
            var length = delta.magnitude;
            if (length < 1f)
                return;
            var direction = delta / length;
            for (float offset = 0f; offset < length; offset += 10f)
                Widgets.DrawLine(
                    a + direction * offset,
                    a + direction * Mathf.Min(offset + 5f, length),
                    color,
                    1f
                );
        }
    }
}
