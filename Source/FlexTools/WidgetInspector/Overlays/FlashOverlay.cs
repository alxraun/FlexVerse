// parents: @.frostline/graph/m-dbg-overlay.xml

using FlexTools.WidgetInspector.Tracking;
using FlexVerse;
using UnityEngine;

namespace FlexTools.WidgetInspector.Overlays
{
    /// <summary>Highlights widgets that recently changed with decaying heat.</summary>
    public class FlashOverlay
    {
        /// <summary>Draws heat flashes over the engine tree.</summary>
        public void Draw(WidgetEngine engine, ChangeTracker tracker, float duration) =>
            DrawWidget(engine, engine.GetRoot(), tracker, duration);

        private void DrawWidget(
            WidgetEngine engine,
            Widget widget,
            ChangeTracker tracker,
            float duration
        )
        {
            if (widget == null)
                return;
            var heat = tracker.GetHeat(widget, duration);
            if (heat > 0.01f && engine.Geometry.TryGet(widget, out var geometry))
                OverlayScope.Draw(
                    engine,
                    widget,
                    () =>
                        Verse.Widgets.DrawBoxSolid(
                            new Rect(Vector2.zero, geometry.LayoutRect.size),
                            new Color(0.2f, 1f, 0.2f, heat * 0.18f)
                        )
                );
            foreach (var child in engine.GetChildren(widget))
                DrawWidget(engine, child, tracker, duration);
        }
    }
}
