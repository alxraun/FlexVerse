// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindButtonFace : Widget
    {
        [Prop]
        public Behavior<string> Label { get; set; } = "";

        [Prop]
        public Behavior<string> Detail { get; set; } = "";

        [Prop]
        public Behavior<bool> Focused { get; set; } = false;

        [Prop]
        public Behavior<float> Hover { get; set; } = 0f;

        [Prop]
        public Behavior<float> Selection { get; set; } = 0f;

        [Prop]
        public Behavior<float> Pressure { get; set; } = 0f;

        [Prop]
        public Behavior<float> Availability { get; set; } = 1f;

        [Prop]
        public Behavior<bool> Primary { get; set; } = false;

        [Prop]
        public Behavior<bool> Quiet { get; set; } = false;

        [Prop]
        public Behavior<bool> Compact { get; set; } = false;

        [Prop]
        public Behavior<TextAnchor> TextAlign { get; set; } = TextAnchor.MiddleCenter;

        [Prop]
        public Behavior<string> Tooltip { get; set; } = "";

        [Prop]
        public Behavior<bool> Outlined { get; set; } = false;

        [Prop]
        public Behavior<bool> Flat { get; set; } = false;

        [Prop]
        public Behavior<float> Radius { get; set; } = RimRewindSkin.CornerRadius;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new View
                {
                    FlexGrow = 1f,
                    MinWidth = 0f.Point(),
                    JustifyContent = Justify.Center,
                    PaddingHorizontal = Compact.Value ? 12f.Point() : 16f.Point(),
                    Tooltip = Tooltip,
                    BorderRadius = Radius,
                    BackgroundColor = Group.Combine(
                        BuildContainer,
                        Hover,
                        Pressure,
                        Selection,
                        Availability,
                        Primary,
                        Quiet,
                        Outlined,
                        Focused
                    ),
                    BorderAll = Outlined.Value ? 1f.Point() : 0f.Point(),
                    BorderColor = Group.Combine(
                        Selection,
                        Availability,
                        (selected, available) =>
                            RimRewindSkin.Alpha(
                                Color.Lerp(
                                    RimRewindPalette.Outline,
                                    RimRewindPalette.Primary,
                                    Mathf.Clamp01(selected)
                                ),
                                available < 1f ? .12f : 1f
                            )
                    ),
                    Children = new WidgetList
                    {
                        new RimRewindFocusRing { Visible = Focused, Radius = Radius },
                        BuildLabel(),
                        string.IsNullOrEmpty(Detail.Value) ? null : BuildDetail(),
                    },
                },
            ];

        private Widget BuildLabel() =>
            new Label
            {
                Content = Label,
                Font = GameFont.Small,
                Color = Group.Combine(BuildInk, Selection, Primary, Availability, Quiet, TextAlign),
                TextAlign = TextAlign,
                TextWrap = false,
                TextOverflow = TextOverflowMode.Clip,
                Height = 24f.Point(),
                MinWidth = 0f.Point(),
            };

        private Widget BuildDetail() =>
            new Label
            {
                Content = Detail,
                Font = GameFont.Tiny,
                Height = 18f.Point(),
                TextAlign = TextAlign,
                Color = Group.Combine(
                    Selection,
                    Availability,
                    (selection, availability) =>
                        RimRewindSkin.Alpha(
                            Color.Lerp(
                                RimRewindPalette.OnSurfaceVariant,
                                RimRewindPalette.OnSecondaryContainer,
                                Mathf.Clamp01(selection)
                            ),
                            availability
                        )
                ),
                TextWrap = false,
            };

        private Color BuildInk()
        {
            var color =
                Primary.Value ? RimRewindPalette.OnPrimary
                : Quiet.Value && TextAlign.Value == TextAnchor.MiddleCenter
                    ? RimRewindPalette.Primary
                : Color.Lerp(
                    RimRewindPalette.OnSurface,
                    RimRewindPalette.OnSecondaryContainer,
                    Mathf.Clamp01(Selection.Value)
                );
            return RimRewindSkin.Alpha(
                Availability.Value < 1f ? RimRewindPalette.OnSurface : color,
                Availability.Value
            );
        }

        private Color BuildContainer()
        {
            if (Availability.Value < 1f)
                return Quiet.Value || Outlined.Value
                    ? Color.clear
                    : RimRewindSkin.Alpha(RimRewindPalette.OnSurface, .12f);
            var selected = Mathf.Clamp01(Selection.Value);
            var color = Primary.Value
                ? RimRewindPalette.Primary
                : Color.Lerp(
                    Quiet.Value || Outlined.Value ? Color.clear : RimRewindPalette.SurfaceHighest,
                    RimRewindPalette.SecondaryContainer,
                    selected
                );

            var layer = Flat.Value ? 0f : Mathf.Clamp01(Hover.Value) * .08f * (1f - selected);
            if (!Flat.Value && Focused.Value)
                layer = Mathf.Max(layer, .10f * (1f - selected));
            layer += Mathf.Clamp01(Pressure.Value) * .12f;
            layer = Mathf.Clamp01(layer);
            var ink = BuildInk();
            return color.a < .001f
                ? RimRewindSkin.Alpha(ink, layer)
                : Color.Lerp(color, ink, layer);
        }
    }
}
