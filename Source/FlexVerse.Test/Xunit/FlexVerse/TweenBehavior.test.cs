// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using System.IO;
using FlexVerse;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Exercises FlexVerse.Easing delegates whose implementation is pure
    /// arithmetic. Easings that route through UnityEngine.Mathf (Pow, Sin, Cos,
    /// Sqrt) are deliberately NOT executed here: the mod's Unity assemblies are
    /// reference assemblies that must never run under the test host.
    /// </summary>
    public class TweenBehaviorTests
    {
        public static IEnumerable<object[]> PureArithmeticEasings()
        {
            yield return new object[] { "Linear", Easing.Linear };
            yield return new object[] { "QuadEaseIn", Easing.QuadEaseIn };
            yield return new object[] { "QuadEaseOut", Easing.QuadEaseOut };
            yield return new object[] { "CubicEaseIn", Easing.CubicEaseIn };
            yield return new object[] { "BackEaseIn", Easing.BackEaseIn };
        }

        public static IEnumerable<object[]> MonotonicEaseInEasings()
        {
            yield return new object[] { "Linear", Easing.Linear };
            yield return new object[] { "QuadEaseIn", Easing.QuadEaseIn };
            yield return new object[] { "CubicEaseIn", Easing.CubicEaseIn };
        }

        [Theory]
        [MemberData(nameof(PureArithmeticEasings))]
        public void AllPureArithmeticEasings_MapZeroToZeroAndOneToOne(
            string name,
            EasingFunction easing
        )
        {
            Assert.True(Math.Abs(easing(0f)) <= 0.00001f, $"{name} must map 0 to 0");
            Assert.True(Math.Abs(easing(1f) - 1f) <= 0.00001f, $"{name} must map 1 to 1");
        }

        [Theory]
        [MemberData(nameof(MonotonicEaseInEasings))]
        public void EaseInFamily_IsNonDecreasingAcrossProgressSweep(
            string name,
            EasingFunction easing
        )
        {
            const int steps = 64;
            for (var i = 1; i <= steps; i++)
            {
                var previous = easing((i - 1) / (float)steps);
                var current = easing(i / (float)steps);
                Assert.True(
                    current >= previous,
                    $"{name} decreased between {(i - 1) / (float)steps:0.###} and {i / (float)steps:0.###}: {previous} -> {current}"
                );
            }
        }

        [Fact]
        public void Linear_IsIdentityFunction()
        {
            Assert.Equal(0.25f, Easing.Linear(0.25f), 5f);
            Assert.Equal(0.5f, Easing.Linear(0.5f), 5f);
            Assert.Equal(0.75f, Easing.Linear(0.75f), 5f);
        }

        [Fact]
        public void QuadEaseIn_MatchesSquaredProgressExactly()
        {
            foreach (var progress in new[] { 0.1f, 0.25f, 0.5f, 0.9f })
            {
                Assert.Equal(progress * progress, Easing.QuadEaseIn(progress), 5f);
            }
        }

        [Fact]
        public void QuadEaseOut_MatchesOneMinusSquaredComplementExactly()
        {
            foreach (var progress in new[] { 0f, 0.1f, 0.25f, 0.5f, 0.75f, 0.9f, 1f })
            {
                var expected = 1f - (1f - progress) * (1f - progress);
                Assert.Equal(expected, Easing.QuadEaseOut(progress), 5f);
            }
        }

        [Fact]
        public void QuadEaseOut_IsFastEarlyAndSlowLate()
        {
            Assert.True(
                Easing.QuadEaseOut(0.25f) > 0.25f,
                $"QuadEaseOut(0.25) = {Easing.QuadEaseOut(0.25f)} should exceed linear progress."
            );

            Assert.Equal(Easing.QuadEaseIn(0.75f), Easing.QuadEaseOut(0.25f), 5f);
        }

        [Fact]
        public void CubicEaseIn_MatchesCubedProgressExactly()
        {
            foreach (var progress in new[] { 0.1f, 0.5f, 0.9f })
            {
                Assert.Equal(progress * progress * progress, Easing.CubicEaseIn(progress), 5f);
            }
        }

        [Fact]
        public void BackEaseIn_UndershootsBelowZeroEarly_ByDesign()
        {
            Assert.True(Easing.BackEaseIn(0.1f) < 0f, "BackEaseIn should undershoot at 0.1.");
            Assert.Equal(0f, Easing.BackEaseIn(0f), 5f);
            Assert.Equal(1f, Easing.BackEaseIn(1f), 5f);
        }
    }
}
