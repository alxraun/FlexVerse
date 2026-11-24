// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoChart : View
    {
        [Prop]
        public Behavior<BentoPanelMotion> Motion { get; set; }

        [Prop]
        public Behavior<int> Identity { get; set; }

        [Prop]
        public Behavior<int> Bars { get; set; } = 12;

        public BentoChart()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.FlexEnd;
            FlexGrow = 1f;
            MinHeight = 0f.Point();
            MinWidth = 0f.Point();
            PaddingTop = 6f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            Enumerable.Range(0, Bars.Value).Select(BuildBar).ToArray();

        private Widget BuildBar(int index)
        {
            var model = Motion.Value;
            var profile =
                0.25f + Mathf.Abs(Mathf.Sin((index + 1) * 0.73f + Identity.Value * 0.5f)) * 0.65f;
            return new BentoRect
            {
                Key = index,
                Ink = index % 3 == 0 ? model.Ink : model.MutedInk,
                FlexGrow = model.Bias.Select(bias => 0.7f + (index % 3 + 1) * bias * 0.3f),
                FlexBasis = 0f.Point(),
                MarginHorizontal = 1.5f.Point(),
                Height = Group.Combine(
                    model.Fill,
                    model.Bias,
                    model.Reveal,
                    (fill, bias, reveal) =>
                    {
                        var assembled = Mathf.Clamp01((reveal - index * 0.025f) / 0.65f);
                        var level = Mathf.Clamp01(
                            fill * profile + bias * (index % 2 == 0 ? 0.2f : 0.35f)
                        );
                        return (4f + 88f * level * assembled).Percent();
                    }
                ),
            };
        }
    }
}
