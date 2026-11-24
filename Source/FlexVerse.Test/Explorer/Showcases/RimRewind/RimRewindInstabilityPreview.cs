// parents: @.frostline/graph/m-explorer.xml
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindInstabilityPreview : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<RimRewindProfile> Profile { get; set; }

        public RimRewindInstabilityPreview()
        {
            PaddingAll = 16f.Point();
            BackgroundColor = RimRewindSkin.Surface;
            BorderAll = 0f.Point();
            BorderColor = RimRewindSkin.Line;
            BorderRadius = RimRewindSkin.CornerRadius;
            MarginBottom = 16f.Point();
        }

        public override void OnMount() => Profile = Session.Value.Draft;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                RimRewindSkin.BuildCaption("After three consecutive rewinds"),
                RimRewindSkin.BuildHeading(
                    Profile.Select(profile => $"Instability {BuildSample(profile):P0}")
                ),
                new RimRewindInstabilityMeter
                {
                    Level = Spring.To(BuildSample(Profile.Value), 80f, 16f),
                    First = Profile.Select(profile => profile[RimRewindSetting.FirstThreshold]),
                    Second = Profile.Select(profile => profile[RimRewindSetting.SecondThreshold]),
                    Third = Profile.Select(profile => profile[RimRewindSetting.ThirdThreshold]),
                },
                new RimRewindThresholdLegend { Profile = Profile },
            ];

        private static float BuildSample(RimRewindProfile profile) =>
            profile.Enabled(RimRewindSetting.Instability)
                ? UnityEngine.Mathf.Clamp01(profile[RimRewindSetting.InstabilityGain] * 3f)
                : 0f;
    }
}
