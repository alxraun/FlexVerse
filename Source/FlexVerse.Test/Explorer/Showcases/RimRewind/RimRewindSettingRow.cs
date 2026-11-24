// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindSettingRow : Widget
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [Prop]
        public Behavior<RimRewindOption> Option { get; set; }

        [Prop]
        public Behavior<int> EntranceIndex { get; set; } = 0;

        [State]
        private Behavior<RimRewindProfile> Profile { get; set; }

        [State]
        private Behavior<float> Entrance { get; set; } = Spring.State(.9f, 80f, 16f);

        [State]
        private Behavior<float> ChangeFlash { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<float> Hover { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior Intro { get; set; }

        [State]
        private Behavior Reaction { get; set; }

        [State]
        private Behavior Observe { get; set; }
        private float _lastObserved;

        public RimRewindSettingRow()
        {
            MinHeight = 104f.Point();
            MarginBottom = 8f.Point();
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
        }

        public override void OnMount()
        {
            Profile = Session.Value.Draft;
            _lastObserved = Profile.Value[Option.Value.Id];
            Intro = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Wait(Math.Min(.12f, EntranceIndex.Value * .018f)),
                    Timeline.Call(() => Entrance.Target = 1f)
                )
            );
            Observe = Effect.Create(HandleProfileChange, Profile);
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    AlignItems = Align.Center,
                    MinHeight = 104f.Point(),
                    PaddingAll = 12f.Point(),
                    BorderAll = 0f.Point(),
                    BorderRadius = RimRewindSkin.CornerRadius,
                    BorderColor = Group.Combine(Hover, ChangeFlash, BuildBorderColor),
                    BackgroundColor = Group.Combine(Hover, ChangeFlash, BuildBackgroundColor),
                    Translate = Entrance.Select(value => new Vector2(
                        0f,
                        (1f - Mathf.Clamp01(value)) * 22f
                    )),
                    Children = new WidgetList
                    {
                        new RimRewindSettingDescription
                        {
                            Option = Option,
                            FlexGrow = 1f,
                            FlexBasis = 0f.Point(),
                            MinWidth = 0f.Point(),
                            MarginRight = 24f.Point(),
                        },
                        new RimRewindSettingControl
                        {
                            Option = Option,
                            Width = 35f.Percent(),
                            MinWidth = 200f.Point(),
                            MaxWidth = 330f.Point(),
                        },
                        new RimRewindSettingReset { Option = Option },
                    },
                },
            ];

        private static Color BuildBorderColor(float hover, float flash) =>
            Color.Lerp(
                RimRewindSkin.Alpha(RimRewindSkin.Line, .4f + hover * .2f),
                RimRewindSkin.Alpha(RimRewindSkin.Accent, .55f),
                Mathf.Clamp01(flash)
            );

        private static Color BuildBackgroundColor(float hover, float flash) =>
            Color.Lerp(
                Color.Lerp(
                    RimRewindPalette.SurfaceLow,
                    RimRewindPalette.SurfaceContainer,
                    Mathf.Clamp01(hover) * .6f
                ),
                RimRewindSkin.GoldDark,
                Mathf.Clamp01(flash) * .5f
            );

        private void HandleProfileChange()
        {
            var current = Profile.Value[Option.Value.Id];
            if (Mathf.Approximately(_lastObserved, current))
                return;
            _lastObserved = current;
            Reaction = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Call(() => ChangeFlash.Target = 1f),
                    Timeline.Wait(.14f),
                    Timeline.Call(() => ChangeFlash.Target = 0f)
                )
            );
        }

        public override void OnMouseEnter(FlexEvent e) => Hover.Target = 1f;

        public override void OnMouseLeave(FlexEvent e) => Hover.Target = 0f;
    }
}
