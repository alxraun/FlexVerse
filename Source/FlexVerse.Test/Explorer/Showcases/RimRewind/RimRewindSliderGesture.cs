// parents: @.frostline/graph/m-explorer.xml
using System;

namespace FlexVerse.Test.Explorer.Showcases
{
    public readonly struct RimRewindSliderGesture : IEquatable<RimRewindSliderGesture>
    {
        public enum Kind
        {
            None,
            Press,
            Grab,
            Drag,
            Release,
        }

        public readonly Kind Action;
        public readonly float Position;
        public readonly int Revision;

        public RimRewindSliderGesture(Kind action, float position, int revision)
        {
            Action = action;
            Position = position;
            Revision = revision;
        }

        public bool Equals(RimRewindSliderGesture other) =>
            Action == other.Action && Position.Equals(other.Position) && Revision == other.Revision;

        public override bool Equals(object other) =>
            other is RimRewindSliderGesture gesture && Equals(gesture);

        public override int GetHashCode() => Revision;
    }
}
