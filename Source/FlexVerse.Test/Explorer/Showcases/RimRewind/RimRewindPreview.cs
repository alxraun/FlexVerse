// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindPreview : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<RimRewindProfile> Profile { get; set; }

        [State]
        private Behavior<float> Depth { get; set; }

        [Prop]
        public Behavior<int> Category { get; set; } = 0;

        [State]
        private Behavior<float> Cursor { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<float> Echo { get; set; } = Spring.State(0f, 80f, 16f);

        [State]
        private Behavior<int> Phase { get; set; } = 0;

        [State]
        private Behavior<bool> Running { get; set; } = false;

        [State]
        private Behavior Playback { get; set; }

        [State]
        private Behavior ConfigurationEffect { get; set; }
        private RimRewind _screen;
        private float _selectedRatio;

        public RimRewindPreview()
        {
            PaddingAll = 12f.Point();
            BackgroundColor = RimRewindPalette.SurfaceLow;
            BorderAll = 0f.Point();
            BorderColor = RimRewindSkin.Line;
            BorderRadius = RimRewindSkin.CornerRadius;
            MarginBottom = 12f.Point();
            Tooltip = "Energy, spent charges and instability persist after the rewind.";
        }

        public override void OnMount()
        {
            _screen = Session.Value;
            Profile = _screen.Draft;
            Depth = _screen.PreviewDepth;
            ConfigurationEffect = Effect.Create(HandleStop, Profile, Depth);
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindPreviewHeader
                {
                    Profile = Profile,
                    Depth = Depth,
                    Category = Category,
                    Phase = Phase,
                    Running = Running,
                    OnPlay = new Action(HandlePlay),
                },
                BuildTrack(),
                Category.Value == 2
                    ? new RimRewindSlider
                    {
                        Value = Group.Combine(
                            Profile,
                            Depth,
                            (profile, depth) =>
                                Mathf.Clamp(depth, 0f, profile[RimRewindSetting.HistoryDepth])
                        ),
                        MinValue = 0f,
                        MaxValue = Profile.Select(profile =>
                            profile[RimRewindSetting.HistoryDepth]
                        ),
                        Height = 40f.Point(),
                        Step = .5f,
                        OnValueChange = new Action<float>(HandleDepth),
                    }
                    : null,
                RimRewindSkin.BuildCaption(Profile.Select(BuildSummary)),
            ];

        private Widget BuildTrack() =>
            Category.Value == 2
                ? new RimRewindCostCurve
                {
                    Profile = Profile,
                    Depth = Depth,
                    Playback = Cursor,
                    Playing = Running,
                    Pulse = Echo,
                    Height = 48f.Point(),
                }
                : new RimRewindHistoryTrack
                {
                    Profile = Profile,
                    Depth = Depth,
                    Playback = Cursor,
                    Playing = Running,
                    Pulse = Echo,
                    OnDepthChange = new Action<float>(HandleDepth),
                    Height = 72f.Point(),
                    Tooltip =
                        "Pick a moment on the track. The teal span is reachable with a full resource reserve.",
                };

        private static string BuildSummary(RimRewindProfile profile) =>
            $"Available rewind: {profile.AvailableDepth:0.#} s   ·   Archive: {profile[RimRewindSetting.MemoryBudget]:0} MB   ·   "
            + (
                profile.Enabled(RimRewindSetting.Branches)
                    ? $"Branches: up to {profile[RimRewindSetting.BranchCount]:0}"
                    : "Branches disabled"
            );

        private void HandleDepth(float depth) => _screen.PreviewDepth.Target = depth;

        private void HandlePlay()
        {
            _selectedRatio = Mathf.Clamp01(
                Depth.Value / Profile.Value[RimRewindSetting.HistoryDepth]
            );
            Playback = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Call(HandleBegin),
                    Timeline.WaitUntil(() => !Cursor.IsUpdating),
                    Timeline.Call(HandleRewind),
                    Timeline.WaitUntil(() => !Cursor.IsUpdating),
                    Timeline.Call(HandleEcho),
                    Timeline.Wait(.18f),
                    Timeline.Call(() => Echo.Target = 0f),
                    Timeline.WaitUntil(() => !Echo.IsUpdating),
                    Timeline.Wait(.5f),
                    Timeline.Call(HandleComplete)
                )
            );
        }

        private void HandleBegin()
        {
            Running.Target = true;
            Phase.Target = 1;
            Cursor.Target = 0f;
        }

        private void HandleRewind()
        {
            Phase.Target = 2;
            Cursor.Target = _selectedRatio;
        }

        private void HandleEcho()
        {
            Phase.Target = 3;
            Echo.Target = 1f;
        }

        private void HandleComplete()
        {
            Running.Target = false;
            Phase.Target = 0;
        }

        private void HandleStop()
        {
            if (Playback is TimelineBehavior timeline)
                timeline.Stop();
            HandleComplete();
            Echo.Target = 0f;
        }
    }
}
