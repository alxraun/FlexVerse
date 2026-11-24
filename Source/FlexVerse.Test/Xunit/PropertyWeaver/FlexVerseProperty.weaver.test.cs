// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Linq;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace FlexVerse.Test.PropertyWeaver
{
    /// <summary>
    /// Runs the real Fody ModuleWeaver over a compiled non-widget fixture to
    /// prove [FVP] auto-properties are redirected to their generated
    /// _Get/_Set counterparts exactly like widget [Prop] properties.
    /// </summary>
    public class FlexVersePropertyWeaverTests : IDisposable
    {
        private readonly WeaveFixture weave = new("flexverse-fvp-weaver-");

        public void Dispose() => weave.Dispose();

        private const string FixtureWithCounterpart = """
            using FlexVerse;

            namespace FVWeave
            {
                public class FvpFixture
                {
                    [FVP] public Behavior<float> Health { get; set; }
                    public Behavior<float> _Health { get; set; }
                }
            }
            """;

        private const string FixtureWithoutCounterpart = """
            using FlexVerse;

            namespace FVWeave
            {
                public class FvpOrphanFixture
                {
                    [FVP] public Behavior<float> Health { get; set; }
                }
            }
            """;

        [Fact]
        public void WovenGetter_IsRedirectedToCounterpartViaSingleCall()
        {
            var assembly = weave.Compile(FixtureWithCounterpart, "FvpGetterILFixture");
            weave.RunWeaver(assembly);

            using var module = weave.OpenModule(assembly);
            var getter = WeaveFixture.GetProperty(module, "FvpFixture", "Health").GetMethod;

            Assert.Equal(3, getter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, getter.Body.Instructions[0].OpCode);
            var call = getter.Body.Instructions[1];
            Assert.Equal(OpCodes.Call, call.OpCode);
            Assert.Contains("::get__Health(", call.Operand.ToString());
            Assert.Equal(OpCodes.Ret, getter.Body.Instructions[2].OpCode);
        }

        [Fact]
        public void WovenSetter_IsRedirectedToCounterpartWithLoadedValue()
        {
            var assembly = weave.Compile(FixtureWithCounterpart, "FvpSetterILFixture");
            weave.RunWeaver(assembly);

            using var module = weave.OpenModule(assembly);
            var setter = WeaveFixture.GetProperty(module, "FvpFixture", "Health").SetMethod;

            Assert.Equal(4, setter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, setter.Body.Instructions[0].OpCode);
            Assert.Equal(OpCodes.Ldarg_1, setter.Body.Instructions[1].OpCode);
            Assert.Equal(OpCodes.Call, setter.Body.Instructions[2].OpCode);
            Assert.Contains("::set__Health(", setter.Body.Instructions[2].Operand.ToString());
            Assert.Equal(OpCodes.Ret, setter.Body.Instructions[3].OpCode);
        }

        [Fact]
        public void FixtureWithoutGeneratedCounterpart_IsReportedAsError()
        {
            var assembly = weave.Compile(FixtureWithoutCounterpart, "FvpOrphanILFixture");
            var result = weave.RunWeaver(assembly);

            Assert.True(
                result.Errors.Any(m =>
                    m.Contains("Could not find the source-generated property counterpart")
                    && m.Contains("FvpOrphanFixture::Health")
                ),
                "Expected a missing-counterpart error, got: ["
                    + string.Join(" | ", result.Errors)
                    + "]"
            );
        }
    }
}
