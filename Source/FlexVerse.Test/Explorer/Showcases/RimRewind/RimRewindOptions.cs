// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Globalization;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public enum RimRewindSetting
    {
        HistoryDepth,
        SnapshotInterval,
        FineHistory,
        MemoryBudget,
        MemoryPolicy,
        HistoryScope,
        PersistHistory,
        AutoAnchors,
        Activation,
        Binding,
        WheelSpeed,
        FineStep,
        MinimumDepth,
        ReleaseAction,
        PauseOnOpen,
        Resource,
        EnergyCapacity,
        BaseCost,
        CostPerSecond,
        RegenInterval,
        RecoveryDelay,
        ChargeCount,
        ChargeRefill,
        ChargeDepth,
        Instability,
        InstabilityGain,
        InstabilityDecay,
        FirstThreshold,
        SecondThreshold,
        ThirdThreshold,
        SideEffects,
        DisorientationDuration,
        MoodDuration,
        Randomness,
        KeepJournal,
        RevealThreats,
        GhostTrails,
        GhostDuration,
        DeathAction,
        DeathLead,
        RescueLimit,
        InjuryAction,
        InjuryLead,
        BuildingAction,
        BuildingLead,
        CaravanAction,
        CaravanLead,
        RememberDeclined,
        ManualAnchors,
        RaidAnchors,
        MaxAnchors,
        Branches,
        BranchCount,
        BranchLifetime,
        BranchCleanup,
        ProtectAnchors,
        EffectIntensity,
        Afterimages,
        RimRewindVolume,
        MusicDucking,
        Camera,
        ReducedMotion,
        EventMarkers,
        MarkerLabels,
        UnavailableHistory,
    }

    public enum RimRewindControl
    {
        Toggle,
        Slider,
        Choice,
    }

    public sealed class RimRewindOption
    {
        public RimRewindSetting Id { get; }
        public int Category { get; }
        public string Group { get; }
        public string Title { get; }
        public string Description { get; }
        public RimRewindControl Control { get; }
        public float Default { get; }
        public float Min { get; }
        public float Max { get; }
        public float Step { get; }
        public string Unit { get; }
        public string[] Choices { get; }
        public bool Advanced { get; }
        public Func<RimRewindProfile, string> Unavailable { get; }
        public Func<RimRewindProfile, bool> Visible { get; }

        public RimRewindOption(
            RimRewindSetting id,
            int category,
            string group,
            string title,
            string description,
            RimRewindControl control,
            float value,
            float min = 0f,
            float max = 1f,
            float step = 1f,
            string unit = "",
            string[] choices = null,
            bool advanced = false,
            Func<RimRewindProfile, string> unavailable = null,
            Func<RimRewindProfile, bool> visible = null
        )
        {
            Id = id;
            Category = category;
            Group = group;
            Title = title;
            Description = description;
            Control = control;
            Default = value;
            Min = min;
            Max = max;
            Step = step;
            Unit = unit;
            Choices = choices ?? Array.Empty<string>();
            Advanced = advanced;
            Unavailable = unavailable;
            Visible = visible;
        }

        public float Normalize(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                return Default;
            var max = Control == RimRewindControl.Choice ? Choices.Length - 1 : Max;
            return Math.Max(
                Min,
                Math.Min(max, Min + (float)Math.Round((value - Min) / Step) * Step)
            );
        }

        public (float Min, float Max) Bounds(RimRewindProfile profile)
        {
            var min = Min;
            var max = Max;
            if (
                Id
                is RimRewindSetting.FineHistory
                    or RimRewindSetting.MinimumDepth
                    or RimRewindSetting.ChargeDepth
            )
                max = Math.Min(max, profile[RimRewindSetting.HistoryDepth]);
            if (Id == RimRewindSetting.FirstThreshold)
                max = profile[RimRewindSetting.SecondThreshold] - .05f;
            if (Id == RimRewindSetting.SecondThreshold)
            {
                min = profile[RimRewindSetting.FirstThreshold] + .05f;
                max = profile[RimRewindSetting.ThirdThreshold] - .05f;
            }
            if (Id == RimRewindSetting.ThirdThreshold)
                min = profile[RimRewindSetting.SecondThreshold] + .05f;
            return (min, max);
        }

        public string Format(float value) =>
            Control switch
            {
                RimRewindControl.Toggle => value > 0f ? "On" : "Off",
                RimRewindControl.Choice => Choices[(int)Normalize(value)],
                _ => Unit == "%"
                    ? $"{value * 100f:0}%"
                    : value.ToString("0.##", CultureInfo.InvariantCulture)
                        + (Unit.Length > 0 ? " " + Unit : ""),
            };
    }

    public static class RimRewindOptions
    {
        public static readonly string[] Categories =
        [
            "Time History",
            "Controls",
            "Ability Cost",
            "Instability",
            "Fate & Memory",
            "Second Chance",
            "Points & Branches",
            "Perception",
        ];

        public static readonly string[] Descriptions =
        [
            "How much of the past stays available, and how precisely it can be rewritten.",
            "Freeze the present, find the right moment, and decide whether to go back.",
            "Every second chance may come at a price. Choose the rules for your colony.",
            "Frequent interference leaves a mark. Decide how fragile time will be.",
            "The world returns to the past. You decide what the colony may remember of it.",
            "Back up important events. Each rule triggers independently of the others.",
            "Pin an important moment, or keep a future you had to give up.",
            "Tune the feel of rewinding: visuals, sound, camera and the timeline.",
        ];

        public static readonly string[] Presets =
        [
            "Second Chance",
            "Butterfly Effect",
            "Sandbox",
            "Custom",
        ];

        private static string Needs(
            RimRewindProfile profile,
            RimRewindSetting setting,
            string reason
        ) => profile.Enabled(setting) ? null : reason;

        public static readonly RimRewindOption[] All =
        [
            Number(
                RimRewindSetting.HistoryDepth,
                0,
                "Recording the Past",
                "History Depth",
                "Available past in simulation seconds. Game speed does not change this span.",
                120,
                15,
                600,
                5,
                "s"
            ),
            Number(
                RimRewindSetting.SnapshotInterval,
                0,
                "Recording the Past",
                "Snapshot Interval",
                "Distance between states. A shorter interval gives finer moment selection.",
                .5f,
                .1f,
                5,
                .1f,
                "s"
            ),
            Number(
                RimRewindSetting.FineHistory,
                0,
                "Recording the Past",
                "Fine Window",
                "The last seconds are recorded at 0.1 s intervals, regardless of the main rate.",
                10,
                0,
                60,
                1,
                "s"
            ),
            Number(
                RimRewindSetting.MemoryBudget,
                0,
                "State Storage",
                "Memory Budget",
                "Maximum archive size. Anchored points and branches share the same budget.",
                512,
                128,
                2048,
                128,
                "MB"
            ),
            Choice(
                RimRewindSetting.MemoryPolicy,
                0,
                "State Storage",
                "When the archive fills",
                "Choose what to sacrifice when no room is left for new states.",
                0,
                ["Compact old history", "Shrink history depth", "Stop recording"]
            ),
            Choice(
                RimRewindSetting.HistoryScope,
                0,
                "State Storage",
                "Recording Scope",
                "All maps allow rewinding events outside the current settlement.",
                1,
                ["Current map only", "Whole colony"],
                advanced: true
            ),
            Toggle(
                RimRewindSetting.PersistHistory,
                0,
                "State Storage",
                "History in Saves",
                "Include the available past in the colony save.",
                false,
                advanced: true
            ),
            Toggle(
                RimRewindSetting.AutoAnchors,
                0,
                "State Storage",
                "Automatic Anchor Points",
                "Allow protected points to be created before important events.",
                true
            ),
            Choice(
                RimRewindSetting.Activation,
                1,
                "Entering the Past",
                "Ability Activation",
                "Holding lets you peek into the past quickly. Toggling keeps the panel open.",
                0,
                ["Hold key", "Press to toggle"]
            ),
            Choice(
                RimRewindSetting.Binding,
                1,
                "Entering the Past",
                "Ability Key",
                "Key combination that opens the timeline.",
                1,
                ["R", "Left Alt + R", "Mouse 4", "Mouse 5"]
            ),
            Toggle(
                RimRewindSetting.PauseOnOpen,
                1,
                "Entering the Past",
                "Pause on Open",
                "Time to pick a moment while the present stands still.",
                true
            ),
            Number(
                RimRewindSetting.WheelSpeed,
                1,
                "Choosing a Moment",
                "Mouse Wheel Step",
                "Rewind depth per single wheel movement.",
                5,
                1,
                30,
                1,
                "s"
            ),
            Number(
                RimRewindSetting.FineStep,
                1,
                "Choosing a Moment",
                "Fine Step with Shift",
                "Used while holding Shift on the timeline.",
                .5f,
                .1f,
                3,
                .1f,
                "s",
                advanced: true
            ),
            Number(
                RimRewindSetting.MinimumDepth,
                1,
                "Choosing a Moment",
                "Minimum Depth",
                "Guards against accidentally rewinding a barely noticeable span.",
                1,
                .5f,
                15,
                .5f,
                "s",
                advanced: true
            ),
            Choice(
                RimRewindSetting.ReleaseAction,
                1,
                "Choosing a Moment",
                "On Key Release",
                "Confirmation shows the selected moment and the final cost.",
                1,
                ["Close preview", "Ask for confirmation", "Rewind immediately"]
            ),
            Choice(
                RimRewindSetting.Resource,
                2,
                "Ability Rule",
                "Limit Model",
                "Energy depends on rewind depth. One charge pays for one use.",
                1,
                ["No limit", "Energy", "Charges"]
            ),
            Number(
                RimRewindSetting.EnergyCapacity,
                2,
                "Energy",
                "Energy Reserve",
                "Maximum ability reserve.",
                100,
                20,
                300,
                10,
                "pts",
                visible: p => p[RimRewindSetting.Resource] == 1
            ),
            Number(
                RimRewindSetting.BaseCost,
                2,
                "Energy",
                "Base Cost",
                "Charged once per confirmed rewind.",
                8,
                0,
                50,
                1,
                "pts",
                visible: p => p[RimRewindSetting.Resource] == 1
            ),
            Number(
                RimRewindSetting.CostPerSecond,
                2,
                "Energy",
                "Cost per Second",
                "Added to the base cost for each second of depth.",
                .4f,
                0,
                3,
                .1f,
                "pts",
                visible: p => p[RimRewindSetting.Resource] == 1
            ),
            Number(
                RimRewindSetting.RegenInterval,
                2,
                "Energy Recovery",
                "One point every",
                "Recovery only runs during normal simulation.",
                6,
                1,
                30,
                1,
                "s",
                visible: p => p[RimRewindSetting.Resource] == 1
            ),
            Number(
                RimRewindSetting.RecoveryDelay,
                2,
                "Energy Recovery",
                "Delay after Rewind",
                "The reserve does not refill until the delay expires.",
                12,
                0,
                60,
                1,
                "s",
                visible: p => p[RimRewindSetting.Resource] == 1
            ),
            Number(
                RimRewindSetting.ChargeCount,
                2,
                "Charges",
                "Max Charges",
                "Number of consecutive uses without recovery.",
                3,
                1,
                10,
                1,
                "",
                visible: p => p[RimRewindSetting.Resource] == 2
            ),
            Number(
                RimRewindSetting.ChargeRefill,
                2,
                "Charges",
                "Charge Recovery",
                "Simulation time to restore one spent charge.",
                180,
                30,
                600,
                15,
                "s",
                visible: p => p[RimRewindSetting.Resource] == 2
            ),
            Number(
                RimRewindSetting.ChargeDepth,
                2,
                "Charges",
                "Depth per Use",
                "Limits the available span regardless of the full archive depth.",
                30,
                5,
                120,
                5,
                "s",
                visible: p => p[RimRewindSetting.Resource] == 2
            ),
            Toggle(
                RimRewindSetting.Instability,
                3,
                "Interference Trace",
                "Temporal Instability",
                "Repeated rewinds make later interference costlier and harder.",
                true
            ),
            Number(
                RimRewindSetting.InstabilityGain,
                3,
                "Interference Trace",
                "Per Use",
                "Instability accumulates outside the rewound history.",
                .15f,
                .05f,
                .5f,
                .05f,
                "%",
                unavailable: InstabilityRequired
            ),
            Number(
                RimRewindSetting.InstabilityDecay,
                3,
                "Interference Trace",
                "-1% every",
                "Calm passage of time gradually restores stability.",
                4,
                1,
                30,
                1,
                "s",
                unavailable: InstabilityRequired
            ),
            Number(
                RimRewindSetting.FirstThreshold,
                3,
                "Instability Thresholds",
                "Cost Increase",
                "From this threshold the energy cost rises by 50%.",
                .35f,
                .1f,
                .8f,
                .05f,
                "%",
                true,
                InstabilityRequired
            ),
            Number(
                RimRewindSetting.SecondThreshold,
                3,
                "Instability Thresholds",
                "Slower Recovery",
                "The next threshold doubles the resource recovery time.",
                .65f,
                .2f,
                .9f,
                .05f,
                "%",
                true,
                InstabilityRequired
            ),
            Number(
                RimRewindSetting.ThirdThreshold,
                3,
                "Instability Thresholds",
                "Deep Rewind Lock",
                "Only the last 10 seconds remain available. Thresholds always keep their order.",
                .9f,
                .3f,
                1,
                .05f,
                "%",
                true,
                InstabilityRequired
            ),
            Choice(
                RimRewindSetting.SideEffects,
                3,
                "Effects on Colonists",
                "Interference Effects",
                "Colonists who live through the altered event receive the effects.",
                1,
                ["Ability limits only", "Disorientation", "\"Déjà Vu\" mood", "Both effects"],
                unavailable: InstabilityRequired
            ),
            Number(
                RimRewindSetting.DisorientationDuration,
                3,
                "Effects on Colonists",
                "Disorientation Duration",
                "Briefly reduces movement accuracy after returning.",
                8,
                1,
                30,
                1,
                "s",
                unavailable: InstabilityRequired,
                visible: p => p[RimRewindSetting.SideEffects] is 1 or 3
            ),
            Number(
                RimRewindSetting.MoodDuration,
                3,
                "Effects on Colonists",
                "Déjà Vu Duration",
                "How long a colonist remembers the feeling of another future.",
                120,
                30,
                600,
                30,
                "s",
                unavailable: InstabilityRequired,
                visible: p => p[RimRewindSetting.SideEffects] is 2 or 3
            ),
            Choice(
                RimRewindSetting.Randomness,
                4,
                "Rules of Fate",
                "Chance Repetition",
                "With the same rolls, the future changes only if actions change.",
                0,
                ["Same actions, same outcome", "New outcome after rewind"]
            ),
            Toggle(
                RimRewindSetting.KeepJournal,
                4,
                "Memory of the Future",
                "Abandoned-Timeline Log",
                "Keep messages from the discarded future in a separate log section.",
                true
            ),
            Toggle(
                RimRewindSetting.RevealThreats,
                4,
                "Memory of the Future",
                "Remember Seen Threats",
                "Mark threats the player already knows with a special outline, without revealing the map.",
                false
            ),
            Toggle(
                RimRewindSetting.GhostTrails,
                4,
                "Memory of the Future",
                "Ghost Trails",
                "Show pawn movements from the abandoned timeline.",
                true
            ),
            Number(
                RimRewindSetting.GhostDuration,
                4,
                "Memory of the Future",
                "Trail Duration",
                "How long movement memories remain on the map.",
                10,
                2,
                30,
                1,
                "s",
                unavailable: p => Needs(p, RimRewindSetting.GhostTrails, "Enable ghost trails")
            ),
            Choice(
                RimRewindSetting.DeathAction,
                5,
                "Colonist Death",
                "Action on Death",
                "Only player colonists count. The event must fall inside the archive.",
                1,
                ["Do nothing", "Offer rewind", "Rewind automatically"]
            ),
            Number(
                RimRewindSetting.DeathLead,
                5,
                "Colonist Death",
                "Lead Time before Event",
                "Go back before the fatal event, leaving time for another decision.",
                8,
                1,
                30,
                1,
                "s",
                unavailable: DeathRequired
            ),
            Number(
                RimRewindSetting.RescueLimit,
                5,
                "Colonist Death",
                "Triggers per Day",
                "The help counter survives rewinds. Zero disables the daily limit.",
                3,
                0,
                10,
                1,
                "",
                unavailable: DeathRequired
            ),
            Choice(
                RimRewindSetting.InjuryAction,
                5,
                "Other Events",
                "Severe Injury",
                "Loss of an organ, or an injury with an immediate threat to life.",
                0,
                ["Ignore", "Offer rewind"]
            ),
            Number(
                RimRewindSetting.InjuryLead,
                5,
                "Other Events",
                "Lead Time before Injury",
                "The nearest recorded state before this moment is chosen.",
                5,
                1,
                20,
                1,
                "s",
                visible: p => p.Enabled(RimRewindSetting.InjuryAction)
            ),
            Choice(
                RimRewindSetting.BuildingAction,
                5,
                "Other Events",
                "Loss of Key Structure",
                "For structures the player has marked as protected.",
                0,
                ["Ignore", "Offer rewind"]
            ),
            Number(
                RimRewindSetting.BuildingLead,
                5,
                "Other Events",
                "Lead Time before Destruction",
                "Leave time to change the fate of a protected structure.",
                10,
                1,
                30,
                1,
                "s",
                visible: p => p.Enabled(RimRewindSetting.BuildingAction)
            ),
            Choice(
                RimRewindSetting.CaravanAction,
                5,
                "Other Events",
                "Loss of Caravan",
                "Requires whole-colony recording in the Time History category.",
                0,
                ["Ignore", "Offer rewind"],
                unavailable: p =>
                    p.Enabled(RimRewindSetting.HistoryScope)
                        ? null
                        : "Select whole-colony recording"
            ),
            Number(
                RimRewindSetting.CaravanLead,
                5,
                "Other Events",
                "Lead Time before Caravan Loss",
                "Time to choose a different decision on the world map.",
                15,
                5,
                60,
                5,
                "s",
                visible: p => p.Enabled(RimRewindSetting.CaravanAction)
            ),
            Toggle(
                RimRewindSetting.RememberDeclined,
                5,
                "Repeat Offers",
                "Remember Player Refusal",
                "Do not offer help again for an event whose rewind you already declined.",
                true
            ),
            Toggle(
                RimRewindSetting.ManualAnchors,
                6,
                "Anchor Points",
                "Manual Points",
                "Allow pinning the selected moment on the timeline.",
                true
            ),
            Toggle(
                RimRewindSetting.RaidAnchors,
                6,
                "Anchor Points",
                "Before Threat Starts",
                "Automatically anchor the state before a raid or other major event.",
                true,
                unavailable: p =>
                    Needs(
                        p,
                        RimRewindSetting.AutoAnchors,
                        "Enable automatic points in Time History"
                    )
            ),
            Number(
                RimRewindSetting.MaxAnchors,
                6,
                "Anchor Points",
                "Max Points",
                "Manual and automatic points share one limit.",
                3,
                1,
                10,
                1,
                ""
            ),
            Toggle(
                RimRewindSetting.ProtectAnchors,
                6,
                "Anchor Points",
                "Protect from Pruning",
                "Unanchored past is freed first. If none remains, recording stops.",
                true,
                advanced: true
            ),
            Toggle(
                RimRewindSetting.Branches,
                6,
                "Abandoned Future",
                "Keep Alternative Branches",
                "After a rewind, keep the abandoned future so you can return to it.",
                false
            ),
            Number(
                RimRewindSetting.BranchCount,
                6,
                "Abandoned Future",
                "Max Branches",
                "Each branch takes space in the shared archive budget.",
                3,
                1,
                8,
                1,
                "",
                unavailable: BranchesRequired
            ),
            Number(
                RimRewindSetting.BranchLifetime,
                6,
                "Abandoned Future",
                "Retention Time",
                "Normal simulation time after which a branch may be removed.",
                300,
                60,
                1800,
                60,
                "s",
                unavailable: BranchesRequired
            ),
            Choice(
                RimRewindSetting.BranchCleanup,
                6,
                "Abandoned Future",
                "Pruning Order",
                "Applied when the branch limit or memory budget is reached.",
                0,
                ["Oldest first", "Shortest first", "Ask the player"],
                unavailable: BranchesRequired
            ),
            Toggle(
                RimRewindSetting.ReducedMotion,
                7,
                "Visuals",
                "Minimum Motion",
                "Reduces distortion, disables afterimage silhouettes and camera motion.",
                false
            ),
            Number(
                RimRewindSetting.EffectIntensity,
                7,
                "Visuals",
                "Time Distortion",
                "Strength of the visual effect while rewinding.",
                .35f,
                0,
                1,
                .05f,
                "%"
            ),
            Toggle(
                RimRewindSetting.Afterimages,
                7,
                "Visuals",
                "Afterimage Silhouettes",
                "Brief images of the previous positions of objects.",
                true,
                unavailable: MotionRequired
            ),
            Choice(
                RimRewindSetting.Camera,
                7,
                "Visuals",
                "Camera after Rewind",
                "Choose where to look after returning to the past.",
                1,
                ["Restore previous position", "Follow the selected pawn", "Keep current position"],
                unavailable: MotionRequired
            ),
            Number(
                RimRewindSetting.RimRewindVolume,
                7,
                "Sound",
                "Rewind Volume",
                "Ability sound volume relative to the overall game level.",
                .65f,
                0,
                1,
                .05f,
                "%"
            ),
            Number(
                RimRewindSetting.MusicDucking,
                7,
                "Sound",
                "Music Ducking",
                "How much quieter music becomes while choosing a moment.",
                .55f,
                0,
                1,
                .05f,
                "%"
            ),
            Choice(
                RimRewindSetting.EventMarkers,
                7,
                "Timeline",
                "Event Markers",
                "How many events are marked on the past timeline.",
                0,
                ["Major events", "All events", "Anchor points only"]
            ),
            Toggle(
                RimRewindSetting.MarkerLabels,
                7,
                "Timeline",
                "Event Labels",
                "Show names next to markers when there is enough room.",
                true
            ),
            Toggle(
                RimRewindSetting.UnavailableHistory,
                7,
                "Timeline",
                "Unreachable Past",
                "Mark the span that cannot be reached with the current resource.",
                true,
                advanced: true
            ),
        ];

        private static readonly System.Collections.Generic.Dictionary<
            RimRewindSetting,
            RimRewindOption
        > ById = All.ToDictionary(option => option.Id);

        public static RimRewindOption Get(RimRewindSetting id) => ById[id];

        public static bool Matches(RimRewindOption option, string query) =>
            string.IsNullOrWhiteSpace(query)
            || query
                .Trim()
                .Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries)
                .All(word =>
                    (
                        option.Title
                        + " "
                        + option.Description
                        + " "
                        + option.Group
                        + " "
                        + Categories[option.Category]
                    ).IndexOf(word, StringComparison.OrdinalIgnoreCase) >= 0
                );

        private static string InstabilityRequired(RimRewindProfile p) =>
            Needs(p, RimRewindSetting.Instability, "Enable temporal instability");

        private static string BranchesRequired(RimRewindProfile p) =>
            Needs(p, RimRewindSetting.Branches, "Enable keeping alternative branches");

        private static string DeathRequired(RimRewindProfile p) =>
            Needs(p, RimRewindSetting.DeathAction, "Select an action on colonist death");

        private static string MotionRequired(RimRewindProfile p) =>
            p.Enabled(RimRewindSetting.ReducedMotion) ? "Disabled by Minimum Motion" : null;

        private static RimRewindOption Toggle(
            RimRewindSetting id,
            int category,
            string group,
            string title,
            string description,
            bool value,
            bool advanced = false,
            Func<RimRewindProfile, string> unavailable = null
        ) =>
            new(
                id,
                category,
                group,
                title,
                description,
                RimRewindControl.Toggle,
                value ? 1 : 0,
                advanced: advanced,
                unavailable: unavailable
            );

        private static RimRewindOption Number(
            RimRewindSetting id,
            int category,
            string group,
            string title,
            string description,
            float value,
            float min,
            float max,
            float step,
            string unit,
            bool advanced = false,
            Func<RimRewindProfile, string> unavailable = null,
            Func<RimRewindProfile, bool> visible = null
        ) =>
            new(
                id,
                category,
                group,
                title,
                description,
                RimRewindControl.Slider,
                value,
                min,
                max,
                step,
                unit,
                advanced: advanced,
                unavailable: unavailable,
                visible: visible
            );

        private static RimRewindOption Choice(
            RimRewindSetting id,
            int category,
            string group,
            string title,
            string description,
            int value,
            string[] choices,
            bool advanced = false,
            Func<RimRewindProfile, string> unavailable = null
        ) =>
            new(
                id,
                category,
                group,
                title,
                description,
                RimRewindControl.Choice,
                value,
                choices: choices,
                advanced: advanced,
                unavailable: unavailable
            );
    }
}
