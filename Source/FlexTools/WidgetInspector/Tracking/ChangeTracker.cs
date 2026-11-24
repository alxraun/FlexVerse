// parents: @.frostline/graph/m-dbg-track.xml

using System.Runtime.CompilerServices;
using UnityEngine;

namespace FlexTools.WidgetInspector.Tracking
{
    /// <summary>Tracks weakly keyed change timestamps and derives heat values.</summary>
    public sealed class ChangeTracker
    {
        private ConditionalWeakTable<object, ChangeTime> _changes = new();

        private sealed class ChangeTime
        {
            /// <summary>Realtime timestamp of the last change.</summary>
            public float Value;
        }

        /// <summary>Records a change timestamp for the target.</summary>
        public void RegisterChange(object target)
        {
            if (target != null)
                _changes.GetOrCreateValue(target).Value = Time.realtimeSinceStartup;
        }

        /// <summary>Returns the decaying heat for the target over the duration.</summary>
        public float GetHeat(object target, float duration)
        {
            if (target == null || duration <= 0f || !_changes.TryGetValue(target, out var change))
                return 0f;
            return Mathf.Clamp01(1f - (Time.realtimeSinceStartup - change.Value) / duration);
        }

        /// <summary>Clears all tracked change timestamps.</summary>
        public void Clear() => _changes = new();
    }
}
