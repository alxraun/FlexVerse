// parents: @.frostline/graph/m-explorer.xml

using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoSectionMotion
    {
        [FVP]
        public Behavior<float> Energy { get; private set; }

        [FVP]
        public Behavior<float> Detail { get; private set; }

        [FVP]
        public Behavior<float> Reveal { get; private set; }

        [FVP]
        public Behavior<float> Weight { get; private set; }

        [FVP]
        public Behavior<float> TopWeight { get; private set; }

        [FVP]
        public Behavior<float> MiddleWeight { get; private set; }

        [FVP]
        public Behavior<float> BottomWeight { get; private set; }

        [FVP]
        public Behavior<Length> Insets { get; private set; }

        [FVP]
        public Behavior<bool> Ready { get; private set; } = false;

        [FVP]
        private Behavior<float> DetailProgram { get; set; } = 0f;

        [FVP]
        private Behavior<float> RevealProgram { get; set; } = 0f;

        [FVP]
        private Behavior Following { get; set; }

        [FVP]
        private Behavior Playback { get; set; }

        [FVP]
        private Behavior PhaseResponse { get; set; }
        public BentoPanelMotion[] Panels { get; private set; }
        private Behavior<float> _pressure;
        private Behavior<float> _density;
        private int _identity;
        public bool Settled =>
            Ready.Value
            && !Energy.IsUpdating
            && !Detail.IsUpdating
            && !Reveal.IsUpdating
            && Panels.All(panel => panel.Settled);

        public void Initialize(
            int identity,
            Behavior<float> pressure,
            Behavior<float> density,
            Behavior<int> cue
        )
        {
            _identity = identity;
            _pressure = pressure;
            _density = density;
            Energy = Spring.State(0.2f, tension: 38f + identity * 8f, friction: 14f);
            Detail = Spring.State(0.2f, tension: 48f + identity * 7f, friction: 16f);
            Reveal = Spring.State(0f, tension: 65f, friction: 18f);
            Weight = Energy.Select(value => 1f + value * 2.1f);
            TopWeight = Group.Combine(
                Energy,
                Detail,
                (energy, detail) => 1.2f + energy * 1.3f + detail * 0.4f
            );
            MiddleWeight = Group.Combine(
                Energy,
                Detail,
                (energy, detail) => 1f + detail * 1.5f + (1f - energy) * 0.35f
            );
            BottomWeight = Group.Combine(
                Detail,
                density,
                (detail, compact) => 0.9f + (1f - detail) * 0.5f + compact * 0.9f
            );
            Insets = density.Select(compact => (5f + (1f - compact) * 5f).Point());
            Watch.DetailProgram.OnUpdate += _ => UpdateDetail();
            Watch.RevealProgram.OnUpdate += _ => Reveal.Target = RevealProgram.Value;
            Panels = Enumerable
                .Range(0, 4)
                .Select(index =>
                {
                    var panel = new BentoPanelMotion();
                    panel.Initialize(identity * 4 + index, Energy, Detail, Reveal);
                    return panel;
                })
                .ToArray();
            Following = Effect.Create(
                () =>
                {
                    Energy.Target = Mathf.Clamp01(_pressure.Value);
                    UpdateDetail();
                },
                pressure,
                density
            );

            PhaseResponse = Effect.Create(StartResponse, EffectTrigger.OnUpdate, cue);
        }

        private void UpdateDetail() =>
            Detail.Target = Mathf.Clamp01(
                DetailProgram.Value * 0.7f + _density.Value * 0.2f + _pressure.Value * 0.1f
            );

        private void StartResponse()
        {
            Ready.Target = false;
            var opening = Reveal.Value < 0.05f;
            Playback = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Wait(_identity * 0.18f),
                    Timeline.Parallel(
                        Timeline.Set(
                            Refs.RevealProgram,
                            Animate.FromTo(opening ? 0f : 0.78f, 1f, 0.75f, Easing.CubicEaseOut)
                        ),
                        Timeline.Sequence(
                            Timeline.Set(
                                Refs.DetailProgram,
                                Animate.To(0.18f + _identity * 0.1f, 0.42f, Easing.SineEaseInOut)
                            ),
                            Timeline.Set(
                                Refs.DetailProgram,
                                Animate.To(
                                    0.8f - _identity * 0.13f,
                                    0.8f + _identity * 0.12f,
                                    Easing.SineEaseInOut
                                )
                            )
                        )
                    ),
                    Timeline.Call(() => Ready.Target = true)
                )
            );
        }

        public void Release()
        {
            foreach (var panel in Panels)
                panel.ReleaseFvp();
            ReleaseFvp();
        }
    }
}
