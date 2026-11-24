// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindThresholdLegend : View
    {
        [Prop]
        public Behavior<RimRewindProfile> Profile { get; set; }

        public RimRewindThresholdLegend()
        {
            FlexDirection = FlexDirections.Row;
            JustifyContent = Justify.SpaceBetween;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                BuildThreshold(RimRewindSetting.FirstThreshold, "Cost +50%"),
                BuildThreshold(RimRewindSetting.SecondThreshold, "Recovery ×2"),
                BuildThreshold(RimRewindSetting.ThirdThreshold, "Only 10 s"),
            ];

        private Widget BuildThreshold(RimRewindSetting setting, string label) =>
            RimRewindSkin.BuildCaption(
                Profile.Select(profile => $"{profile[setting]:P0} · {label}")
            );
    }
}
