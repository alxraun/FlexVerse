// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using System;
using FlexVerse.Flexbox;

namespace FlexVerse
{
    /// <summary>Represents a length value with a unit such as points, percent or auto.</summary>
    public readonly struct Length(float v, Unit u) : IEquatable<Length>
    {
        /// <summary>Holds the numeric value of the length.</summary>
        public readonly float value = v;

        /// <summary>Holds the unit applied to the value.</summary>
        public readonly Unit unit = u;

        /// <summary>Represents an automatic length resolved by layout.</summary>
        public static Length Auto => new(float.NaN, Unit.Auto);

        /// <summary>Represents an undefined length.</summary>
        public static Length Undefined => new(float.NaN, Unit.Undefined);

        /// <summary>Copies every length value from one array to another.</summary>
        public static void CopyValue(Length[] dest, Length[] src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                dest[i] = src[i];
            }
        }

        /// <summary>Indicates whether this length equals the other length.</summary>
        public bool Equals(Length other)
        {
            return unit == other.unit && Flex.FloatsEqual(value, other.value);
        }

        /// <summary>Indicates whether this length equals the given object.</summary>
        public override bool Equals(object obj)
        {
            return obj is Length other && Equals(other);
        }

        /// <summary>Returns a hash code combining value and unit.</summary>
        public override int GetHashCode()
        {
            return HashCode.Combine(value, unit);
        }

        /// <summary>Compares two lengths for equality.</summary>
        public static bool operator ==(Length left, Length right)
        {
            return left.Equals(right);
        }

        /// <summary>Compares two lengths for inequality.</summary>
        public static bool operator !=(Length left, Length right)
        {
            return !left.Equals(right);
        }

        /// <summary>Returns a human-readable representation of the length.</summary>
        public override string ToString()
        {
            return unit switch
            {
                Unit.Auto => "Unit.Auto",
                Unit.Percent => $"{value}%",
                Unit.Point => $"{value}",
                Unit.Undefined => "Unit.Undefined",
                _ => $"{value} ({unit})",
            };
        }
    }
}
