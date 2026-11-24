// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    [Screen("Showcases/Unblock")]
    public partial class Unblock : View
    {
        [State]
        public Behavior<int> Beat { get; set; } = 0;

        [State]
        public Behavior Playback { get; set; }

        [State]
        public Behavior<Texture2D> Glow { get; set; }

        [State]
        public Behavior<float> Pulse { get; set; } = 1f;

        [State]
        public Behavior Pulsing { get; set; }

        public Unblock()
        {
            BackgroundColor = UnblockVisuals.Background;
            FlexGrow = 1;
            Width = 100f.Percent();
            Height = 100f.Percent();
            AlignItems = Align.Center;
            JustifyContent = Justify.Center;
            PaddingAll = 24f.Point();
        }

        public override void OnMount()
        {
            Glow = UnblockVisuals.CreateGlow();
            Pulsing = Timeline.Create(
                Timeline.While(
                    () => true,
                    Timeline.Sequence(
                        Timeline.Set(Refs.Pulse, Animate.To(0.35f, 0.7f)),
                        Timeline.Set(Refs.Pulse, Animate.To(1f, 0.7f))
                    )
                )
            );
            Playback = Timeline.Create(
                Timeline.While(
                    () => true,
                    Timeline.Sequence(
                        Timeline.Wait(0.75f),
                        Timeline.Call(() =>
                            Beat.Target = (Beat.Value + 1) % UnblockChoreography.Beats
                        ),
                        Timeline.If(() => Beat.Value == 11, Timeline.Wait(0.65f))
                    )
                )
            );
        }

        public override void OnUnmount()
        {
            if (Glow.Value != null)
                Object.Destroy(Glow.Value);
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new UnblockBackdrop
                {
                    Glow = Glow,
                    PositionLeft = 0f.Point(),
                    PositionTop = 0f.Point(),
                    PositionType = PositionTypes.Absolute,
                    Width = 100f.Percent(),
                    Height = 100f.Percent(),
                },
                new View
                {
                    Width = 100f.Percent(),
                    MaxWidth = 1152f.Point(),
                    MaxHeight = 100f.Percent(),
                    PaddingAll = 18f.Point(),
                    BorderAll = 2f.Point(),
                    BorderColor = UnblockVisuals.Frame,
                    BackgroundColor = UnblockVisuals.Board,
                    Children = new WidgetList
                    {
                        new UnblockCanvas
                        {
                            Width = 100f.Percent(),
                            AspectRatio =
                                (float)UnblockChoreography.Columns / UnblockChoreography.Rows,
                            FlexShrink = 1,
                            Children = BuildBoard(),
                        },
                    },
                },
            ];

        private WidgetList BuildBoard() =>
            new(
                new Widget[]
                {
                    new UnblockGate
                    {
                        Beat = Beat,
                        Glow = Glow,
                        Pulse = Pulse,
                    },
                }.Concat(
                    UnblockChoreography.Blocks.OrderBy(block => block.IsHero).Select(BuildTile)
                )
            );

        private UnblockTile BuildTile(UnblockBlock block) =>
            new()
            {
                Key = block.Id,
                Block = block,
                Beat = Beat,
                Glow = Glow,
                Pulse = Pulse,
                PositionType = PositionTypes.Absolute,
                Width = 100f.Percent(),
                Height = 100f.Percent(),
            };
    }
}
