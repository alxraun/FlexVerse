// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace FlexVerse.Test.PropertyWeaver
{
    /// <summary>
    /// Runs the real Fody ModuleWeaver over real fixture assemblies compiled
    /// on disk (standard Fody test pattern), then reopens the woven assembly
    /// with Mono.Cecil and asserts on the woven members/IL.
    /// </summary>
    public class PropertyWeaverTests : IDisposable
    {
        private readonly WeaveFixture weave = new("flexverse-weaver-");

        public void Dispose() => weave.Dispose();

        private const string FixtureWithCounterpart = """
            using FlexVerse;

            namespace FVWeave
            {
                public class SampleFixture
                {
                    [Prop] public Behavior<float> MyProp { get; set; }
                    public Behavior<float> _MyProp { get; set; }
                }
            }
            """;

        private const string FixtureWithoutCounterpart = """
            using FlexVerse;

            namespace FVWeave
            {
                public class OrphanFixture
                {
                    [Prop] public Behavior<float> Orphan { get; set; }
                }
            }
            """;

        private const string PlainFixture = """
            namespace FVWeave
            {
                public class PlainFixture
                {
                    public int Answer { get; set; }
                }
            }
            """;

        [Fact]
        public void WovenGetter_IsRedirectedToCounterpartViaSingleCall()
        {
            var assembly = weave.Compile(FixtureWithCounterpart, "GetterILFixture", true);
            weave.RunWeaver(assembly);

            using var module = weave.OpenModule(assembly);
            var getter = WeaveFixture.GetProperty(module, "SampleFixture", "MyProp").GetMethod;

            Assert.Equal(3, getter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, getter.Body.Instructions[0].OpCode);
            var call = getter.Body.Instructions[1];
            Assert.Equal(OpCodes.Call, call.OpCode);
            Assert.Contains("::get__MyProp(", call.Operand.ToString());
            Assert.Equal(OpCodes.Ret, getter.Body.Instructions[2].OpCode);
        }

        [Fact]
        public void WovenSetter_IsRedirectedToCounterpartWithLoadedValue()
        {
            var assembly = weave.Compile(FixtureWithCounterpart, "SetterILFixture", true);
            weave.RunWeaver(assembly);

            using var module = weave.OpenModule(assembly);
            var setter = WeaveFixture.GetProperty(module, "SampleFixture", "MyProp").SetMethod;

            Assert.Equal(4, setter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, setter.Body.Instructions[0].OpCode);
            Assert.Equal(OpCodes.Ldarg_1, setter.Body.Instructions[1].OpCode);
            Assert.Equal(OpCodes.Call, setter.Body.Instructions[2].OpCode);
            Assert.Contains("::set__MyProp(", setter.Body.Instructions[2].Operand.ToString());
            Assert.Equal(OpCodes.Ret, setter.Body.Instructions[3].OpCode);
        }

        [Fact]
        public void FixtureWithoutGeneratedCounterpart_IsReportedAsError_AndLeftUnwoven()
        {
            var assembly = weave.Compile(FixtureWithoutCounterpart, "OrphanILFixture", true);
            var result = weave.RunWeaver(assembly);

            Assert.True(
                result.Errors.Any(m =>
                    m.Contains("Could not find the source-generated property counterpart")
                    && m.Contains("OrphanFixture::Orphan")
                ),
                "Expected a missing-counterpart error, got: ["
                    + string.Join(" | ", result.Errors)
                    + "] warnings: ["
                    + string.Join(" | ", result.Warnings)
                    + "]"
            );

            using var module = weave.OpenModule(assembly);
            var getter = WeaveFixture.GetProperty(module, "OrphanFixture", "Orphan").GetMethod;

            Assert.Equal(3, getter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldfld, getter.Body.Instructions[1].OpCode);
        }

        [Fact]
        public void FixtureWithoutFlexVerseReference_AbortsWeavingWithMissingTypeError()
        {
            var assembly = weave.Compile(PlainFixture, "PlainFixture", false);
            var result = weave.RunWeaver(assembly);

            Assert.Contains(
                result.Errors,
                m => m.Contains("Could not find required type 'FlexVerse.Widget'")
            );

            using var module = weave.OpenModule(assembly);
            var getter = WeaveFixture.GetProperty(module, "PlainFixture", "Answer").GetMethod;
            Assert.Equal(OpCodes.Ldfld, getter.Body.Instructions[1].OpCode);
        }

        [Fact]
        public void NonTargetProperties_InClassesWithoutFlexProperties_AreLeftUntouched()
        {
            var assembly = weave.Compile(FixtureWithCounterpart, "MixedFixture", true);
            weave.RunWeaver(assembly);

            using var module = weave.OpenModule(assembly);
            var counterGetter = WeaveFixture
                .GetProperty(module, "SampleFixture", "_MyProp")
                .GetMethod;

            Assert.Equal(3, counterGetter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldfld, counterGetter.Body.Instructions[1].OpCode);
        }
    }
}
