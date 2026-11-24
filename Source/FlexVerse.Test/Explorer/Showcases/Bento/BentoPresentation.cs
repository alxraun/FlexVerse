// parents: @.frostline/graph/m-explorer.xml

using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Six purposeful compositions; local responses retain their own spring state.</summary>
    public partial class BentoPresentation
    {
        [FVP]
        public Behavior<float> Focus { get; private set; }

        [FVP]
        public Behavior<float> Balance { get; private set; }

        [FVP]
        public Behavior<float> Density { get; private set; }

        [FVP]
        public Behavior<Length> FooterHeight { get; private set; }

        [FVP]
        public Behavior<string> Phase { get; private set; } = "ASSEMBLY";

        [FVP]
        public Behavior<int> PhaseIndex { get; private set; } = 0;

        [FVP]
        public Behavior<int> Cue { get; private set; } = 0;

        [FVP]
        private Behavior<float> FocusProgram { get; set; } = 0.4f;

        [FVP]
        private Behavior<float> BalanceProgram { get; set; } = 0.5f;

        [FVP]
        private Behavior<float> DensityProgram { get; set; } = 0.3f;

        [FVP]
        private Behavior Playback { get; set; }

        [FVP]
        private Behavior<float> FirstPressure { get; set; }

        [FVP]
        private Behavior<float> SecondPressure { get; set; }

        [FVP]
        private Behavior<float> ThirdPressure { get; set; }
        public BentoSectionMotion[] Sections { get; private set; }

        public void Initialize()
        {
            Focus = Spring.State(0.4f, tension: 36f, friction: 14f);
            Balance = Spring.State(0.5f, tension: 44f, friction: 15f);
            Density = Spring.State(0.3f, tension: 48f, friction: 16f);
            Watch.FocusProgram.OnUpdate += _ => Focus.Target = FocusProgram.Value;
            Watch.BalanceProgram.OnUpdate += _ => Balance.Target = BalanceProgram.Value;
            Watch.DensityProgram.OnUpdate += _ => Density.Target = DensityProgram.Value;
            FooterHeight = Density.Select(value => (62f + value * 38f).Point());
            FirstPressure = Group.Combine(
                Focus,
                Balance,
                (focus, balance) => Mathf.Clamp01(focus * 0.9f + balance * 0.1f)
            );
            SecondPressure = Group.Combine(
                Focus,
                Balance,
                (focus, balance) => Mathf.Clamp01((1f - focus) * 0.8f + balance * 0.2f)
            );
            ThirdPressure = Group.Combine(
                Focus,
                Balance,
                (focus, balance) => Mathf.Clamp01(balance * 0.85f + focus * 0.15f)
            );
            var pressures = new[] { FirstPressure, SecondPressure, ThirdPressure };
            Sections = Enumerable
                .Range(0, 3)
                .Select(index =>
                {
                    var section = new BentoSectionMotion();
                    section.Initialize(index, pressures[index], Density, Cue);
                    return section;
                })
                .ToArray();
            Playback = Timeline.Create(
                Timeline.While(
                    () => true,
                    Timeline.Sequence(
                        PhaseAction(0, "ASSEMBLY", 0.45f, 0.5f, 0.3f, 1.1f),
                        PhaseAction(1, "EXPANSION", 1f, 0.2f, 0.2f, 1.6f),
                        PhaseAction(2, "CASCADE", 0.7f, 0.95f, 0.55f, 1.8f),
                        PhaseAction(3, "COMPRESSION", 0.12f, 0.25f, 1f, 1.6f),
                        PhaseAction(4, "RECOMPOSITION", 0.85f, 1f, 0.72f, 2.1f),
                        PhaseAction(5, "RESOLUTION", 0.45f, 0.5f, 0.25f, 1.5f),
                        Timeline.Wait(1.5f)
                    )
                )
            );
        }

        private ITimelineAction PhaseAction(
            int index,
            string name,
            float focus,
            float balance,
            float density,
            float duration
        ) =>
            Timeline.Sequence(
                Timeline.Call(() =>
                {
                    Phase.Target = name;
                    PhaseIndex.Target = index;
                    Cue.Target++;
                }),
                Timeline.Parallel(
                    AnimateProgram(Refs.FocusProgram, focus, duration, Easing.SineEaseInOut),
                    Timeline.Sequence(
                        Timeline.Wait(0.15f),
                        Timeline.Parallel(
                            AnimateProgram(
                                Refs.BalanceProgram,
                                balance,
                                duration * 0.85f,
                                Easing.CubicEaseInOut
                            ),
                            Timeline.Sequence(
                                Timeline.Wait(0.15f),
                                AnimateProgram(
                                    Refs.DensityProgram,
                                    density,
                                    duration * 0.75f,
                                    Easing.SineEaseInOut
                                )
                            )
                        )
                    )
                ),
                Timeline.WaitUntil(() =>
                    Sections.All(section => section.Settled)
                    && !Focus.IsUpdating
                    && !Balance.IsUpdating
                    && !Density.IsUpdating
                ),
                Timeline.Wait(index == 5 ? 0.8f : 0.65f)
            );

        private static ITimelineAction AnimateProgram(
            IPropertyRef property,
            float destination,
            float duration,
            EasingFunction easing
        ) =>
            Timeline.If(
                () => ((Behavior<float>)property.Get()).Value != destination,
                Timeline.Set(property, Animate.To(destination, duration, easing))
            );

        public void Release()
        {
            foreach (var section in Sections)
                section.Release();
            ReleaseFvp();
        }
    }
}
