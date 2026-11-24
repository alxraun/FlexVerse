// parents: @.frostline/graph/m-widget-models.xml

namespace FlexVerse.SourceGenerators.Widgets.Models
{
    /// <summary>Represents the Reactivity struct.</summary>
    public readonly record struct Reactivity(
        bool ReconcileOnSet,
        bool ReconcileOnUpdate,
        bool MarkDirtyOnSet,
        bool MarkDirtyOnUpdate
    )
    {
        /// <summary>Gets the affects tree.</summary>
        public bool AffectsTree => ReconcileOnSet || ReconcileOnUpdate;

        /// <summary>Gets the affects layout.</summary>
        public bool AffectsLayout => MarkDirtyOnSet || MarkDirtyOnUpdate;

        /// <summary>Gets the triggers set.</summary>
        public bool TriggersSet => ReconcileOnSet || MarkDirtyOnSet;

        /// <summary>Gets the triggers update.</summary>
        public bool TriggersUpdate => ReconcileOnUpdate || MarkDirtyOnUpdate;

        /// <summary>Gets the none.</summary>
        public static Reactivity None => new(false, false, false, false);
    }
}
