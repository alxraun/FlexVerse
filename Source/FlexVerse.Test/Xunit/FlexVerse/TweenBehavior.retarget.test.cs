using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using FlexVerse;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test
{
    public static class UnityEngineShimLoader
    {
        private static Assembly? _shim;

        [ModuleInitializer]
        internal static void Initialize()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (_, args) =>
            {
                if (new AssemblyName(args.Name).Name != "UnityEngine.CoreModule")
                {
                    return null;
                }

                return CompileShim();
            };
        }

        private static Assembly CompileShim()
        {
            if (_shim != null)
            {
                return _shim;
            }

            var coreDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
            var references = new List<MetadataReference>();
            foreach (var dll in Directory.GetFiles(coreDir, "*.dll"))
            {
                try
                {
                    references.Add(MetadataReference.CreateFromFile(dll));
                }
                catch (BadImageFormatException) { }
            }

            var compilation = CSharpCompilation.Create(
                "UnityEngine.CoreModule",
                new[] { CSharpSyntaxTree.ParseText(ShimSource) },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            using var peStream = new MemoryStream();
            var result = compilation.Emit(peStream);
            if (!result.Success)
            {
                throw new InvalidOperationException(
                    "UnityEngine.CoreModule test-double compilation failed: "
                        + string.Join("; ", result.Diagnostics)
                );
            }

            peStream.Position = 0;
            _shim = Assembly.Load(peStream.ToArray());
            return _shim;
        }

        private const string ShimSource = """
            namespace UnityEngine
            {
                public struct Vector2
                {
                    public float x;
                    public float y;

                    public Vector2(float x, float y)
                    {
                        this.x = x;
                        this.y = y;
                    }

                    public static Vector2 LerpUnclamped(Vector2 a, Vector2 b, float t)
                        => throw new System.NotSupportedException("not exercised offline");

                    public static float Distance(Vector2 a, Vector2 b)
                        => throw new System.NotSupportedException("not exercised offline");

                    public static Vector2 operator +(Vector2 a, Vector2 b)
                        => throw new System.NotSupportedException("not exercised offline");
                }

                public struct Vector4
                {
                    public float x;
                    public float y;
                    public float z;
                    public float w;

                    public float magnitude
                        => throw new System.NotSupportedException("not exercised offline");

                    public static Vector4 operator -(Vector4 a, Vector4 b)
                        => throw new System.NotSupportedException("not exercised offline");
                }

                public struct Color
                {
                    public float r;
                    public float g;
                    public float b;
                    public float a;

                    public static Color LerpUnclamped(Color a, Color b, float t)
                        => throw new System.NotSupportedException("not exercised offline");

                    public static implicit operator Vector4(Color c)
                        => new Vector4 { x = c.r, y = c.g, z = c.b, w = c.a };
                }

                public static class Debug
                {
                    public static void Log(object message)
                    {
                    }
                }

                public static class Time
                {
                    public static int frameCount => 0;
                }

                public static class Mathf
                {
                    public static float LerpUnclamped(float a, float b, float t)
                        => a + (b - a) * t;

                    public static float Clamp01(float value)
                        => value < 0f ? 0f : value > 1f ? 1f : value;

                    public static float Clamp(float value, float min, float max)
                        => value < min ? min : value > max ? max : value;

                    public static float Max(float a, float b)
                        => a > b ? a : b;

                    public static float Abs(float value)
                        => value < 0f ? -value : value;

                    public static int Abs(int value)
                        => value < 0 ? -value : value;

                    public static int RoundToInt(float value)
                        => (int)System.Math.Round(value, System.MidpointRounding.ToEven);
                }
            }
            """;
    }

    public class TweenBehaviorRetargetTests
    {
        private static TweenBehavior<float> CreateTween()
        {
            return new TweenBehavior<float>
            {
                Target = 10f,
                Duration = 1f,
                MinDuration = 0f,
                MaxDuration = 5f,
                Easing = Easing.Linear,
            };
        }

        [Fact]
        public void Retarget_ContinuesFromCurrentValue_WithoutRestartingFromScratch()
        {
            var tween = CreateTween();
            tween.Activate();

            tween.Update(0.25f);
            Assert.Equal(2.5f, tween.Value, 5f);

            tween.Target = 20f;

            tween.Update(0f);
            Assert.Equal(2.5f, tween.Value, 5f);

            tween.Update(0.875f);
            Assert.Equal(11.25f, tween.Value, 5f);
        }

        [Fact]
        public void Retarget_PreservesElapsedTimeSemantics_AndFinishesExactlyAtTarget()
        {
            var tween = CreateTween();
            var completeReports = 0;
            tween.OnComplete += () => completeReports++;

            tween.Activate();
            tween.Update(0.25f);
            tween.Target = 20f;

            tween.Update(1.75f);

            Assert.Equal(1, completeReports);
            Assert.Equal(20f, tween.Value, 5f);
            Assert.False(tween.IsUpdating);
        }

        [Fact]
        public void Retarget_SameTarget_DoesNotRestartTimeline()
        {
            var tween = CreateTween();
            var progressReports = new List<float>();
            tween.OnUpdate += p => progressReports.Add(p);

            tween.Activate();
            tween.Update(0.25f);

            tween.Target = 10f;
            tween.Update(0.75f);

            Assert.Equal(10f, tween.Value, 5f);
            Assert.False(tween.IsUpdating);
            Assert.Equal(1f, progressReports[^1], 5f);
            Assert.True(
                progressReports.Count >= 2,
                "updates must keep flowing on the original schedule"
            );
        }
    }
}
