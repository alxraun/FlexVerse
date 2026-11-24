// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindCostCurve : Widget
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

        [State]
        private Behavior<float> Selection { get; set; }

        [State]
        private Behavior Prepare { get; set; }
        private float[] _heights;
        private Color[] _colors;
        private bool _energy;
        private float _capacityLine;
        private const int Samples = 48;

        public override void OnMount()
        {
            _heights = new float[Samples];
            _colors = new Color[Samples];
            Selection = Group.Combine(
                Profile,
                Depth,
                (profile, depth) => Mathf.Clamp01(depth / profile[RimRewindSetting.HistoryDepth])
            );
            Prepare = Effect.Create(HandleProfileChange, Profile);
        }

        private void HandleProfileChange()
        {
            var profile = Profile.Value;
            var max = profile[RimRewindSetting.HistoryDepth];
            var ceiling = Mathf.Max(
                profile[RimRewindSetting.EnergyCapacity],
                profile.RimRewindCost(max)
            );
            var available = profile.AvailableDepth;
            _energy = profile[RimRewindSetting.Resource] == 1;
            _capacityLine = profile[RimRewindSetting.EnergyCapacity] / ceiling;
            for (var i = 0; i < Samples; i++)
            {
                var seconds = (i + 1f) / Samples * max;
                _heights[i] = _energy ? profile.RimRewindCost(seconds) / ceiling : .3f;
                _colors[i] = seconds <= available ? RimRewindSkin.Accent : RimRewindSkin.Amber;
            }
        }

        public override void OnGUI(Rect rect)
        {
            if (Event.current.type != EventType.Repaint)
                return;
            var step = rect.width / Samples;
            var selected = Playing.Value ? Playback.Value : Selection.Value;
            for (var i = 0; i < Samples; i++)
            {
                var height = Mathf.Max(3f, _heights[i] * (rect.height - 12f));
                var alpha = (i + 1f) / Samples <= selected ? .8f : .22f;
                Widgets.DrawBoxSolid(
                    new Rect(
                        rect.x + i * step,
                        rect.yMax - height,
                        Mathf.Max(1f, step - 3f),
                        height
                    ),
                    RimRewindSkin.Alpha(_colors[i], alpha + Mathf.Clamp01(Pulse.Value) * .12f)
                );
            }
            if (_energy)
                Widgets.DrawLineHorizontal(
                    rect.x,
                    rect.yMax - _capacityLine * (rect.height - 12f),
                    rect.width,
                    RimRewindSkin.Alpha(RimRewindSkin.Text, .45f)
                );
        }
    }
}
