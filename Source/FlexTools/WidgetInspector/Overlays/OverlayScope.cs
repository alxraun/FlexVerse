// parents: @.frostline/graph/m-dbg-overlay.xml

using System;
using FlexVerse;
using UnityEngine;

namespace FlexTools.WidgetInspector.Overlays
{
    internal static class OverlayScope
    {
        /// <summary>Draws content transformed and clipped into the widget's context.</summary>
        public static void Draw(WidgetEngine engine, Widget widget, Action draw)
        {
            if (!engine.Geometry.TryGet(widget, out var geometry) || !geometry.Invertible)
                return;
            var original = GUI.matrix;
            var hostOrigin = NativeOrigin();
            var host = original * Translation(hostOrigin);
            var groups = 0;
            try
            {
                foreach (var clip in engine.ClipService.GetClips(widget))
                {
                    GUI.matrix = host * clip.LocalToScreen * Translation(-NativeOrigin());
                    GUI.BeginGroup(clip.LocalBounds);
                    groups++;
                }
                GUI.matrix =
                    host
                    * engine.TransformService.GetWorldMatrix(widget)
                    * Translation(-NativeOrigin());
                draw();
            }
            finally
            {
                while (groups-- > 0)
                    GUI.EndGroup();
                GUI.matrix = original;
            }
        }

        private static Matrix4x4 Translation(Vector2 point) =>
            Matrix4x4.Translate(new Vector3(point.x, point.y, 0f));

        private static Vector2 NativeOrigin()
        {
            var original = GUI.matrix;
            try
            {
                GUI.matrix = Matrix4x4.identity;
                return GUIUtility.GUIToScreenPoint(Vector2.zero);
            }
            finally
            {
                GUI.matrix = original;
            }
        }
    }
}
