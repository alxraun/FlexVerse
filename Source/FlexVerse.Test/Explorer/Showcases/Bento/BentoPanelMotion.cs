// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>A panel's response has its own inertia, independent of its parent's spring.</summary>
    public partial class BentoPanelMotion
    {
        [FVP]
        public Behavior<float> Fill { get; private set; }

        [FVP]
        public Behavior<float> Bias { get; private set; }

        [FVP]
        public Behavior<Color> Ink { get; private set; }

        [FVP]
        public Behavior<Color> MutedInk { get; private set; }

        [FVP]
        public Behavior<float> Split { get; private set; }

        [FVP]
        private Behavior Following { get; set; }
        private Behavior<float> _energy;
        private Behavior<float> _detail;
        private int _identity;
        public Behavior<float> Reveal { get; private set; }
        public bool Settled => !Fill.IsUpdating && !Bias.IsUpdating;

        public void Initialize(
            int identity,
            Behavior<float> energy,
            Behavior<float> detail,
            Behavior<float> reveal
        )
        {
            _identity = identity;
            Reveal = reveal;
            _energy = energy;
            _detail = detail;
            Fill = Spring.State(
                0.05f,
                tension: 38f + identity % 4 * 8f,
                friction: 13f + identity % 3
            );
            Bias = Spring.State(0.4f, tension: 32f + identity % 3 * 7f, friction: 13f);
            Split = Group.Combine(Fill, Bias, (fill, bias) => 0.8f + fill * 1.8f + bias * 0.6f);
            Ink = Group.Combine(
                Fill,
                reveal,
                (fill, alpha) =>
                {
                    var color = Color.Lerp(new Color(0.35f, 0.43f, 0.49f), Accent(identity), fill);
                    color.a = Mathf.Clamp01(alpha);
                    return color;
                }
            );
            MutedInk = Ink.Select(color => new Color(color.r, color.g, color.b, color.a * 0.22f));
            Following = Effect.Create(Respond, energy, detail);
        }

        private void Respond()
        {
            Fill.Target = Mathf.Clamp01(
                0.12f + _energy.Value * (0.5f + _identity % 3 * 0.1f) + _detail.Value * 0.2f
            );
            Bias.Target = Mathf.Clamp01(
                0.15f + _detail.Value * 0.6f + _energy.Value * (_identity % 2 == 0 ? 0.2f : -0.12f)
            );
        }

        private static Color Accent(int identity)
        {
            var section = identity / 4;
            return section switch
            {
                0 => new Color(0.69f, 0.83f, 0.79f),
                1 => new Color(0.62f, 0.72f, 0.91f),
                _ => new Color(0.84f, 0.73f, 0.60f),
            };
        }
    }
}
