// parents: @.frostline/graph/m-widget.xml

using System;

namespace FlexVerse
{
    internal readonly struct ImplicitKey(Type parentType, Type childType, int orderIndex)
        : IEquatable<ImplicitKey>
    {
        private readonly Type _parentType =
            parentType ?? throw new ArgumentNullException(nameof(parentType));
        private readonly Type _childType =
            childType ?? throw new ArgumentNullException(nameof(childType));
        private readonly int _orderIndex = orderIndex;

        internal int OrderIndex => _orderIndex;

        public bool Equals(ImplicitKey other)
        {
            return _parentType == other._parentType
                && _childType == other._childType
                && _orderIndex == other._orderIndex;
        }

        public override bool Equals(object obj)
        {
            return obj is ImplicitKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = _parentType.GetHashCode();
                hashCode = (hashCode * 397) ^ _childType.GetHashCode();
                hashCode = (hashCode * 397) ^ _orderIndex;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"ImplicitKey(Hash:{GetHashCode() & 0xFFFFF:X5},Order:{_orderIndex})";
        }

        public static bool operator ==(ImplicitKey left, ImplicitKey right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ImplicitKey left, ImplicitKey right)
        {
            return !left.Equals(right);
        }
    }
}
