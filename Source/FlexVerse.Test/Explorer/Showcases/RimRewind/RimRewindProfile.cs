// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Immutable, typed configuration snapshots; edits never mutate the applied profile.</summary>
    public sealed class RimRewindProfile
    {
        private readonly float[] _values;

        private RimRewindProfile(float[] values) => _values = values;

        public float this[RimRewindSetting id] => _values[(int)id];

        public bool Enabled(RimRewindSetting id) => this[id] > 0f;

        public RimRewindProfile With(RimRewindSetting id, float value)
        {
            var next = (float[])_values.Clone();
            next[(int)id] = RimRewindOptions.Get(id).Normalize(value);
            float Read(RimRewindSetting key) => next[(int)key];
            void Limit(RimRewindSetting key, float min, float max) =>
                next[(int)key] = Math.Max(min, Math.Min(max, Read(key)));
            Limit(RimRewindSetting.FineHistory, 0, Read(RimRewindSetting.HistoryDepth));
            Limit(RimRewindSetting.MinimumDepth, .5f, Read(RimRewindSetting.HistoryDepth));
            Limit(RimRewindSetting.ChargeDepth, 5, Read(RimRewindSetting.HistoryDepth));
            if (id == RimRewindSetting.FirstThreshold)
                Limit(id, .1f, Read(RimRewindSetting.SecondThreshold) - .05f);
            if (id == RimRewindSetting.SecondThreshold)
                Limit(
                    id,
                    Read(RimRewindSetting.FirstThreshold) + .05f,
                    Read(RimRewindSetting.ThirdThreshold) - .05f
                );
            if (id == RimRewindSetting.ThirdThreshold)
                Limit(id, Read(RimRewindSetting.SecondThreshold) + .05f, 1f);
            return new RimRewindProfile(next);
        }

        public int DifferenceCount(RimRewindProfile other) =>
            RimRewindOptions.All.Count(option => Differs(other, option.Id));

        public bool Differs(RimRewindProfile other, RimRewindSetting id) =>
            Math.Abs(this[id] - other[id]) > .0001f;

        public static RimRewindProfile Preset(int preset)
        {
            var values = new float[Enum.GetValues(typeof(RimRewindSetting)).Length];
            foreach (var option in RimRewindOptions.All)
                values[(int)option.Id] = option.Default;
            void Set(RimRewindSetting id, float value) => values[(int)id] = value;
            if (preset == 0)
            {
                Set(RimRewindSetting.HistoryDepth, 30);
                Set(RimRewindSetting.Resource, 2);
                Set(RimRewindSetting.ChargeCount, 2);
                Set(RimRewindSetting.ChargeRefill, 300);
                Set(RimRewindSetting.Instability, 0);
                Set(RimRewindSetting.GhostTrails, 0);
                Set(RimRewindSetting.RescueLimit, 2);
                Set(RimRewindSetting.EffectIntensity, .2f);
            }
            else if (preset == 2)
            {
                Set(RimRewindSetting.HistoryDepth, 300);
                Set(RimRewindSetting.MemoryBudget, 1024);
                Set(RimRewindSetting.Resource, 0);
                Set(RimRewindSetting.Instability, 0);
                Set(RimRewindSetting.Branches, 1);
                Set(RimRewindSetting.BranchCount, 5);
                Set(RimRewindSetting.Randomness, 1);
                Set(RimRewindSetting.DeathAction, 0);
                Set(RimRewindSetting.RevealThreats, 1);
            }
            return new RimRewindProfile(values);
        }

        public RimRewindProfile ResetCategory(int category)
        {
            var values = (float[])_values.Clone();
            foreach (
                var option in RimRewindOptions.All.Where(option => option.Category == category)
            )
                values[(int)option.Id] = option.Default;

            var profile = new RimRewindProfile(values);
            return profile.With(
                RimRewindSetting.HistoryDepth,
                profile[RimRewindSetting.HistoryDepth]
            );
        }

        public float EffectiveEffect =>
            Enabled(RimRewindSetting.ReducedMotion)
                ? Math.Min(.15f, this[RimRewindSetting.EffectIntensity])
                : this[RimRewindSetting.EffectIntensity];

        public float RimRewindCost(float seconds) =>
            this[RimRewindSetting.BaseCost] + this[RimRewindSetting.CostPerSecond] * seconds;

        public float AvailableDepth =>
            this[RimRewindSetting.Resource] switch
            {
                1 => Math.Min(
                    this[RimRewindSetting.HistoryDepth],
                    this[RimRewindSetting.CostPerSecond] > 0
                            ? Math.Max(
                                0,
                                (
                                    this[RimRewindSetting.EnergyCapacity]
                                    - this[RimRewindSetting.BaseCost]
                                ) / this[RimRewindSetting.CostPerSecond]
                            )
                        : this[RimRewindSetting.EnergyCapacity] >= this[RimRewindSetting.BaseCost]
                            ? this[RimRewindSetting.HistoryDepth]
                        : 0
                ),
                2 => Math.Min(
                    this[RimRewindSetting.HistoryDepth],
                    this[RimRewindSetting.ChargeDepth]
                ),
                _ => this[RimRewindSetting.HistoryDepth],
            };
    }

    /// <summary>Both scopes keep independent applied and draft snapshots for this showcase session.</summary>
    public sealed class RimRewindWorkspace
    {
        private readonly RimRewindProfile[] _drafts;
        private readonly RimRewindProfile[] _applied;
        public int Scope { get; }
        public RimRewindProfile Draft => _drafts[Scope];
        public RimRewindProfile Applied => _applied[Scope];
        public int Pending => Draft.DifferenceCount(Applied);

        public RimRewindWorkspace()
            : this(
                0,
                [RimRewindProfile.Preset(1), RimRewindProfile.Preset(1)],
                [RimRewindProfile.Preset(1), RimRewindProfile.Preset(1)]
            ) { }

        private RimRewindWorkspace(int scope, RimRewindProfile[] drafts, RimRewindProfile[] applied)
        {
            Scope = scope;
            _drafts = drafts;
            _applied = applied;
        }

        public RimRewindWorkspace SwitchScope(int scope) => new(scope, _drafts, _applied);

        public RimRewindWorkspace Edit(RimRewindSetting id, float value) =>
            ReplaceDraft(Draft.With(id, value));

        public RimRewindWorkspace Revert() => ReplaceDraft(Applied);

        public RimRewindWorkspace ResetCategory(int category) =>
            ReplaceDraft(Draft.ResetCategory(category));

        public RimRewindWorkspace Apply()
        {
            var applied = (RimRewindProfile[])_applied.Clone();
            applied[Scope] = Draft;
            return new RimRewindWorkspace(Scope, _drafts, applied);
        }

        private RimRewindWorkspace ReplaceDraft(RimRewindProfile draft)
        {
            var drafts = (RimRewindProfile[])_drafts.Clone();
            drafts[Scope] = draft;
            return new RimRewindWorkspace(Scope, drafts, _applied);
        }
    }
}
