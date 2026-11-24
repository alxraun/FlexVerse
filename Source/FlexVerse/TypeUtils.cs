// parents: @.frostline/graph/m-behavior.xml

using System;
using UnityEngine;

namespace FlexVerse
{
    /// <summary>Supplies type-generic interpolation and value arithmetic. (M-BEHAVIOR)</summary>
    public static class TypeUtils
    {
        /// <summary>Interpolates between two values of a supported type.</summary>
        public static T Interpolate<T>(T from, T to, float progress)
        {
            switch ((from, to))
            {
                case (int intFrom, int intTo):
                    return (T)
                        (object)Mathf.RoundToInt(Mathf.LerpUnclamped(intFrom, intTo, progress));
                case (float floatFrom, float floatTo):
                    return (T)(object)Mathf.LerpUnclamped(floatFrom, floatTo, progress);
                case (Vector2 vec2From, Vector2 vec2To):
                    return (T)(object)Vector2.LerpUnclamped(vec2From, vec2To, progress);
                case (Color colorFrom, Color colorTo):
                    return (T)(object)Color.LerpUnclamped(colorFrom, colorTo, progress);
                case (Length valueFrom, Length valueTo):
                    var fromVal = GetNumericValue(valueFrom);
                    var toVal = GetNumericValue(valueTo);
                    float interpolatedValue = Mathf.LerpUnclamped(fromVal, toVal, progress);
                    return (T)(object)new Length(interpolatedValue, valueTo.unit);
                default:
                    throw new NotImplementedException(
                        $"Interpolation for type {typeof(T)} is not implemented."
                    );
            }
        }

        /// <summary>Interpolates without clamping the progress parameter.</summary>
        public static T InterpolateUnclamped<T>(T from, T to, float progress)
        {
            return Interpolate(from, to, progress);
        }

        /// <summary>Adds two values of a supported type.</summary>
        public static T Add<T>(T a, T b)
        {
            switch ((a, b))
            {
                case (int intA, int intB):
                    return (T)(object)(intA + intB);
                case (float floatA, float floatB):
                    return (T)(object)(floatA + floatB);
                case (Vector2 vecA, Vector2 vecB):
                    return (T)(object)(vecA + vecB);
                case (Color colA, Color colB):
                    return (T)(object)(colA + colB);
                case (Length lenA, Length lenB):
                    var targetUnit = lenA.unit;
                    if (lenA.unit != lenB.unit)
                    {
                        if (lenA.unit == Unit.Point)
                            targetUnit = lenB.unit;
                        else if (lenB.unit == Unit.Point)
                            targetUnit = lenA.unit;
                        else
                            throw new ArgumentException(
                                $"Cannot add Lengths with different units: {lenA.unit} and {lenB.unit}"
                            );
                    }
                    return (T)(object)new Length(lenA.value + lenB.value, targetUnit);
                default:
                    throw new NotImplementedException(
                        $"Add for type {typeof(T)} is not implemented."
                    );
            }
        }

        /// <summary>Subtracts two values of a supported type.</summary>
        public static T Subtract<T>(T a, T b)
        {
            switch ((a, b))
            {
                case (int intA, int intB):
                    return (T)(object)(intA - intB);
                case (float floatA, float floatB):
                    return (T)(object)(floatA - floatB);
                case (Vector2 vecA, Vector2 vecB):
                    return (T)(object)(vecA - vecB);
                case (Color colA, Color colB):
                    return (T)(object)(colA - colB);
                case (Length lenA, Length lenB):
                    var targetUnit = lenA.unit;
                    if (lenA.unit != lenB.unit)
                    {
                        if (lenA.unit == Unit.Point)
                            targetUnit = lenB.unit;
                        else if (lenB.unit == Unit.Point)
                            targetUnit = lenA.unit;
                        else
                            throw new ArgumentException(
                                $"Cannot subtract Lengths with different units: {lenA.unit} and {lenB.unit}"
                            );
                    }
                    return (T)(object)new Length(lenA.value - lenB.value, targetUnit);
                default:
                    throw new NotImplementedException(
                        $"Subtract for type {typeof(T)} is not implemented."
                    );
            }
        }

        /// <summary>Measures the distance between two values of a supported type.</summary>
        public static float GetDistance<T>(T from, T to)
        {
            switch ((from, to))
            {
                case (int intFrom, int intTo):
                    return Mathf.Abs(intTo - intFrom);
                case (float floatFrom, float floatTo):
                    return Mathf.Abs(floatTo - floatFrom);
                case (Vector2 vec2From, Vector2 vec2To):
                    return Vector2.Distance(vec2From, vec2To);
                case (Color colorFrom, Color colorTo):
                    return ((Vector4)colorTo - (Vector4)colorFrom).magnitude;
                case (Length valueFrom, Length valueTo):
                    var fromVal = GetNumericValue(valueFrom);
                    var toVal = GetNumericValue(valueTo);
                    return Mathf.Abs(toVal - fromVal);
                default:
                    throw new NotImplementedException(
                        $"GetDistance for type {typeof(T)} is not implemented."
                    );
            }
        }

        private static float GetNumericValue(Length v)
        {
            return (v.unit == Unit.Point || v.unit == Unit.Percent) ? v.value : default;
        }
    }
}
