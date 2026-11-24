// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindHistoryTrack : View
    {
        [Prop]
        public Behavior<float> Playback { get; set; } = 0f;

        [Prop]
        public Behavior<bool> Playing { get; set; } = false;

        [Prop]
        public Behavior<float> Pulse { get; set; } = 0f;

        [Prop]
        public Behavior<RimRewindProfile> Profile { get; set; }

        [Prop]
        public Behavior<float> Depth { get; set; }

        [Prop]
        public Behavior<Action<float>> OnDepthChange { get; set; }

        [State]
        private Behavior<float> Available { get; set; }
        private bool _dragging;

        public override void OnMount() =>
            Available = Profile.Select(profile =>
                Mathf.Clamp01(profile.AvailableDepth / profile[RimRewindSetting.HistoryDepth])
            );

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildAxis(),
                Profile.Value.Enabled(RimRewindSetting.MarkerLabels)
                && Profile.Value[RimRewindSetting.EventMarkers] < 2
                    ? BuildEventLabels()
                    : null,
            ];

        private Widget BuildAxis() =>
            new View
            {
                FlexDirection = FlexDirections.Row,
                JustifyContent = Justify.SpaceBetween,
                Children = new WidgetList
                {
                    RimRewindSkin.BuildCaption(
                        Profile.Select(profile => $"−{profile[RimRewindSetting.HistoryDepth]:0} s")
                    ),
                    RimRewindSkin.BuildCaption("Past"),
                    RimRewindSkin.BuildCaption("Now", RimRewindSkin.Accent),
                },
            };

        private static Widget BuildEventLabels() =>
            new View
            {
                FlexDirection = FlexDirections.Row,
                JustifyContent = Justify.SpaceAround,
                MarginTop = 27f.Point(),
                Children = new WidgetList
                {
                    RimRewindSkin.BuildCaption("Raid start"),
                    RimRewindSkin.BuildCaption("Injury"),
                    RimRewindSkin.BuildCaption("Anchor Point", RimRewindSkin.Amber),
                },
            };

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (Event.current.type != EventType.Repaint)
                return;
            var profile = Profile.Value;
            var depth = profile[RimRewindSetting.HistoryDepth];
            var rail = new Rect(rect.x + 2f, rect.y + 34f, Mathf.Max(1f, rect.width - 4f), 6f);
            var available = Available.Value;
            Widgets.DrawBoxSolid(rail, RimRewindSkin.Raised);
            Widgets.DrawBoxSolid(
                new Rect(
                    rail.xMax - rail.width * available,
                    rail.y,
                    rail.width * available,
                    rail.height
                ),
                RimRewindSkin.Alpha(RimRewindSkin.Rewind, .45f + Mathf.Clamp01(Pulse.Value) * .4f)
            );
            if (profile.Enabled(RimRewindSetting.UnavailableHistory) && available < 1f)
                for (var x = rail.x; x < rail.xMax - rail.width * available; x += 9f)
                    Widgets.DrawBoxSolid(
                        new Rect(x, rail.y - 2f, 1f, 10f),
                        RimRewindSkin.Alpha(RimRewindSkin.Amber, .35f)
                    );
            var count =
                profile[RimRewindSetting.EventMarkers] == 0 ? 3
                : profile[RimRewindSetting.EventMarkers] == 1 ? 7
                : 1;
            for (var i = 0; i < count; i++)
            {
                var fraction = count == 1 ? .72f : .18f + i * .65f / (count - 1);
                Widgets.DrawBoxSolid(
                    new Rect(rail.x + rail.width * fraction - 2f, rail.y - 5f, 4f, 16f),
                    i == count - 1 ? RimRewindSkin.Amber : RimRewindSkin.Muted
                );
            }
            var shown = Playing.Value ? Playback.Value * depth : Depth.Value;
            var cursor = rail.xMax - Mathf.Clamp(shown, 0f, depth) / depth * rail.width;
            if (Playing.Value)
            {
                var selected = rail.xMax - Mathf.Clamp(Depth.Value, 0f, depth) / depth * rail.width;
                Widgets.DrawBoxSolid(
                    new Rect(selected, rail.y - 7f, 1f, 21f),
                    RimRewindSkin.Alpha(RimRewindSkin.Accent, .4f)
                );
            }
            Widgets.DrawBoxSolid(new Rect(cursor - 1f, rail.y - 12f, 2f, 30f), RimRewindSkin.Text);
            Widgets.DrawBoxSolid(new Rect(cursor - 4f, rail.y - 12f, 8f, 4f), RimRewindSkin.Accent);
        }

        private void HandleSeek(FlexEvent e)
        {
            if (!Engine.TransformService.TryToLocal(this, e.MousePosition, out var local))
                return;
            var width = Engine.GetLayoutResults()[this].width;
            var fraction = Mathf.Clamp01((local.x - 2f) / Mathf.Max(1f, width - 4f));
            OnDepthChange.Value?.Invoke(
                Mathf.Round((1f - fraction) * Profile.Value[RimRewindSetting.HistoryDepth] * 2f)
                    * .5f
            );
            e.StopPropagation();
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (e.Button == 0)
            {
                _dragging = true;
                HandleSeek(e);
            }
        }

        public override void OnMouseDrag(FlexEvent e)
        {
            if (_dragging && e.Button == 0)
                HandleSeek(e);
        }

        public override void OnMouseUp(FlexEvent e)
        {
            if (e.Button == 0)
                _dragging = false;
        }

        public override void OnMouseLeave(FlexEvent e) => _dragging = false;
    }
}
