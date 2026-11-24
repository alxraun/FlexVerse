// parents: @.frostline/graph/m-behavior.xml

namespace FlexVerse
{
    /// <summary>Provides the canonical empty behavior with no effect. (M-BEHAVIOR, SCP-IN-003)</summary>
    public sealed class NullBehavior : Behavior
    {
        /// <summary>Supplies the shared null-behavior instance.</summary>
        public static readonly NullBehavior Instance = new();

        private NullBehavior() { }

        /// <summary>Activates the behavior with no effect.</summary>
        public override void Activate() { }

        /// <summary>Advances the behavior with no effect.</summary>
        public override void Update(float deltaTime) { }

        /// <summary>Accepts state handoff with no effect.</summary>
        public override void UnionWith(Behavior source) { }

        /// <summary>Returns the shared null-behavior instance.</summary>
        public override Behavior CloneUntyped() => this;

        /// <summary>Reports semantic equality with any null behavior.</summary>
        public override bool SemEquals(Behavior other)
        {
            return other is NullBehavior;
        }

        /// <summary>Resets the behavior to its inactive state.</summary>
        public override void Reset()
        {
            base.Reset();
        }
    }
}
