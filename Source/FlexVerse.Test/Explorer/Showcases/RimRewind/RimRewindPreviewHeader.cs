// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindPreviewHeader : View
    {
        [Prop]
        public Behavior<RimRewindProfile> Profile { get; set; }

        [Prop]
        public Behavior<float> Depth { get; set; }

        [Prop]
        public Behavior<int> Category { get; set; }

        [Prop]
        public Behavior<int> Phase { get; set; }

        [Prop]
        public Behavior<bool> Running { get; set; }

        [Prop]
        public Behavior<Action> OnPlay { get; set; }

        public RimRewindPreviewHeader()
        {
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = Group.Combine(Phase, Category, BuildTitle),
                    Color = RimRewindSkin.Text,
                    TextAlign = TextAnchor.MiddleLeft,
                    Height = 26f.Point(),
                    FlexGrow = 1f,
                },
                new RimRewindButton
                {
                    Label = Running.Select(running => running ? "Rewinding…" : "Rewind Preview"),
                    Width = 152f.Point(),
                    Height = 40f.Point(),
                    MarginRight = 16f.Point(),
                    Compact = true,
                    Selected = Running,
                    Disabled = Group.Combine(
                        Profile,
                        Depth,
                        Running,
                        (profile, depth, running) =>
                            running
                            || depth <= 0f
                            || Mathf.Min(depth, profile[RimRewindSetting.HistoryDepth])
                                > profile.AvailableDepth
                    ),
                    Tooltip = "Play the selected span on the preview track",
                    OnActivate = OnPlay,
                },
                new Label
                {
                    Content = Group.Combine(Profile, Depth, BuildCostCaption),
                    Color = Group.Combine(
                        Profile,
                        Depth,
                        (profile, depth) =>
                            Mathf.Min(depth, profile[RimRewindSetting.HistoryDepth])
                            > profile.AvailableDepth
                                ? RimRewindSkin.Amber
                                : RimRewindSkin.Accent
                    ),
                    Font = GameFont.Small,
                    Height = 22f.Point(),
                    TextWrap = false,
                    TextAlign = TextAnchor.MiddleRight,
                },
            ];

        private static string BuildTitle(int phase, int category) =>
            phase switch
            {
                1 => "Returning to the present",
                2 => "Rewinding time",
                3 => "Timeline altered",
                _ => category == 2 ? "Cost of Returning" : "History Preview",
            };

        private static string BuildCostCaption(RimRewindProfile profile, float selected)
        {
            var depth = Mathf.Clamp(selected, 0f, profile[RimRewindSetting.HistoryDepth]);
            var cost = profile[RimRewindSetting.Resource] switch
            {
                0 => "No limit",
                2 => "1 charge",
                _ =>
                    $"{profile.RimRewindCost(depth):0.#} / {profile[RimRewindSetting.EnergyCapacity]:0} energy",
            };
            return $"−{depth:0.#} s  /  {cost}";
        }
    }
}
