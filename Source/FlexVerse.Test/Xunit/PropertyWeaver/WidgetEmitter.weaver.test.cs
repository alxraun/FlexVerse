// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Linq;
using FlexVerse.PropertyWeaver.Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Xunit;

namespace FlexVerse.Test.PropertyWeaver
{
    /// <summary>
    /// SCN-009, integration leg: the FULL pipeline over one [Prop] fixture —
    /// the in-process source generator emits the `_{Prop}` counterpart
    /// property, the emitted plus declared sources compile into a real fixture
    /// assembly on disk, and the real Fody ModuleWeaver redirects the
    /// auto-property accessors onto the emitted counterparts. Proves
    /// M-WIDGET-EMITTER.REQ-003 end to end (no hand-written counterparts).
    /// </summary>
    public class EmitterWeaverIntegrationTests : IDisposable
    {
        private readonly WeaveFixture weave = new("flexverse-emitter-weaver-");

        public void Dispose() => weave.Dispose();

        private const string WidgetFixture = """
            using FlexVerse;

            namespace FVWeave
            {
                public partial class EmittedWidget : Widget
                {
                    [Prop] public Behavior<float> Speed { get; set; }
                }
            }
            """;

        [Fact]
        public void GeneratedCounterpart_IsProduced_AndWeaverRedirectsAccessorsOntoIt()
        {
            var assemblyPath = weave.PathFor("EmitterWeaveFixture.dll");

            var declaredTree = CSharpSyntaxTree.ParseText(
                WidgetFixture,
                new CSharpParseOptions(LanguageVersion.Latest)
            );
            var references = TestPaths
                .FlexVerseFixtureRefs()
                .Select(r => (MetadataReference)MetadataReference.CreateFromFile(r))
                .ToList();
            var declarationCompilation = CSharpCompilation.Create(
                "EmitterWeaveFixture",
                new[] { declaredTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var driver = CSharpGeneratorDriver.Create(
                new[] { new FlexVerse.SourceGenerators.FlexVerseGenerator().AsSourceGenerator() }
            );
            var driverResult = driver.RunGeneratorsAndUpdateCompilation(
                declarationCompilation,
                out _,
                out _
            );
            var generatedTrees = driverResult
                .GetRunResult()
                .Results.SelectMany(r => r.GeneratedSources)
                .Where(gs => gs.HintName.EndsWith(".g.cs", StringComparison.Ordinal))
                .Where(gs => gs.SourceText.Length > 0)
                .Select(gs =>
                    CSharpSyntaxTree.ParseText(
                        gs.SourceText.ToString(),
                        new CSharpParseOptions(LanguageVersion.Latest)
                    )
                )
                .ToList();

            Assert.NotEmpty(generatedTrees);

            var compilation = CSharpCompilation.Create(
                "EmitterWeaveFixture",
                new SyntaxTree[] { declaredTree }.Concat(generatedTrees),
                references,
                new CSharpCompilationOptions(
                    OutputKind.DynamicallyLinkedLibrary,
                    optimizationLevel: OptimizationLevel.Release
                )
            );
            var emit = compilation.Emit(assemblyPath);
            Assert.True(
                emit.Success,
                "Declared + generated sources must compile clean: "
                    + string.Join("; ", emit.Diagnostics.Select(d => d.GetMessage()))
            );

            weave.CopyFlexVerseDll();

            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(weave.DirectoryPath);
            using var module = ModuleDefinition.ReadModule(
                assemblyPath,
                new ReaderParameters { AssemblyResolver = resolver }
            );
            var weaver = new TestModuleWeaver
            {
                AssemblyFilePath = assemblyPath,
                ModuleDefinition = module,
                AssemblyResolver = resolver,
            };
            weaver.ErrorHandler = message => Assert.Fail("weaver error: " + message);
            weaver.Execute();

            var wovenPath = weave.PathFor("EmitterWeaveFixture.woven.dll");
            module.Write(wovenPath, new WriterParameters { WriteSymbols = false });

            using var woven = ModuleDefinition.ReadModule(wovenPath);
            var type = woven.GetType("FVWeave.EmittedWidget");

            var counterpart = type.Properties.SingleOrDefault(p => p.Name == "_Speed");
            Assert.NotNull(counterpart);

            var getter = type.Properties.Single(p => p.Name == "Speed").GetMethod;
            Assert.Equal(3, getter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, getter.Body.Instructions[0].OpCode);
            Assert.Equal(OpCodes.Call, getter.Body.Instructions[1].OpCode);
            Assert.Contains("::get__Speed(", getter.Body.Instructions[1].Operand.ToString());
            Assert.Equal(OpCodes.Ret, getter.Body.Instructions[2].OpCode);

            var setter = type.Properties.Single(p => p.Name == "Speed").SetMethod;
            Assert.Equal(4, setter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldarg_0, setter.Body.Instructions[0].OpCode);
            Assert.Equal(OpCodes.Ldarg_1, setter.Body.Instructions[1].OpCode);
            Assert.Equal(OpCodes.Call, setter.Body.Instructions[2].OpCode);
            Assert.Contains("::set__Speed(", setter.Body.Instructions[2].Operand.ToString());
            Assert.Equal(OpCodes.Ret, setter.Body.Instructions[3].OpCode);

            var counterpartGetter = counterpart.GetMethod;
            Assert.Equal(3, counterpartGetter.Body.Instructions.Count);
            Assert.Equal(OpCodes.Ldfld, counterpartGetter.Body.Instructions[1].OpCode);
        }
    }
}
