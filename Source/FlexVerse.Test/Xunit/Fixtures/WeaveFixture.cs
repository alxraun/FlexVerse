// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FlexVerse.PropertyWeaver.Fody;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Mono.Cecil;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Shared scaffolding for on-disk weaver tests: owns one TempDir per test
    /// (xunit creates a fresh test-class instance per test), compiles fixture
    /// sources into real assemblies, runs the real Fody ModuleWeaver over
    /// them, and reopens woven modules with Mono.Cecil for IL asserts.
    /// </summary>
    internal sealed class WeaveFixture : IDisposable
    {
        private readonly TempDir tempDir;

        public WeaveFixture(string prefix)
        {
            tempDir = new TempDir(prefix);
        }

        public string DirectoryPath => tempDir.FullName;

        public string PathFor(string fileName) => tempDir.PathFor(fileName);

        /// <summary>
        /// Compile a fixture source into [dir]/[assemblyName].dll against
        /// .NET Framework 4.8 reference assemblies, plus FlexVerse fixture
        /// references when the fixture uses FlexVerse types.
        /// </summary>
        public string Compile(string source, string assemblyName, bool referenceFlexVerse = true)
        {
            var assemblyPath = tempDir.PathFor(assemblyName + ".dll");

            var references = TestPaths
                .Framework48Refs()
                .Select(r => (MetadataReference)MetadataReference.CreateFromFile(r))
                .ToList();
            if (referenceFlexVerse)
            {
                references.AddRange(
                    TestPaths
                        .FlexVerseFixtureRefs()
                        .Select(r => MetadataReference.CreateFromFile(r))
                );
            }

            var compilation = CSharpCompilation.Create(
                assemblyName,
                new[]
                {
                    CSharpSyntaxTree.ParseText(
                        source,
                        new CSharpParseOptions(LanguageVersion.Latest)
                    ),
                },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var emit = compilation.Emit(assemblyPath);
            Assert.True(
                emit.Success,
                "Fixture compilation failed: "
                    + string.Join("; ", emit.Diagnostics.Select(d => d.GetMessage()))
            );

            if (referenceFlexVerse)
            {
                CopyFlexVerseDll();
            }

            return assemblyPath;
        }

        /// <summary>
        /// FlexVerse.dll must sit next to fixtures for the weaver's resolver;
        /// call separately when the compilation was custom (e.g. generator
        /// pipelines that only use FlexVerseFixtureRefs).
        /// </summary>
        public void CopyFlexVerseDll()
        {
            File.Copy(TestPaths.FlexVerseDll, tempDir.PathFor("FlexVerse.dll"), true);
        }

        /// <summary>
        /// Run the real ModuleWeaver over the assembly in place and collect
        /// its Info/Warning/Error diagnostics.
        /// </summary>
        public WeaveResult RunWeaver(string assemblyPath)
        {
            var result = new WeaveResult { AssemblyPath = assemblyPath };
            var resolver = new DefaultAssemblyResolver();
            resolver.AddSearchDirectory(Path.GetDirectoryName(assemblyPath)!);
            var module = ModuleDefinition.ReadModule(
                assemblyPath,
                new ReaderParameters { AssemblyResolver = resolver }
            );

            var weaver = new TestModuleWeaver
            {
                AssemblyFilePath = assemblyPath,
                ModuleDefinition = module,
                AssemblyResolver = resolver,
            };
            weaver.InfoHandler = message => result.Info.Add(message);
            weaver.WarningHandler = message => result.Warnings.Add(message);
            weaver.ErrorHandler = message => result.Errors.Add(message);

            weaver.Execute();
            module.Write(assemblyPath);
            return result;
        }

        /// <summary>Reopen a (woven) assembly read-only for IL asserts.</summary>
        public ModuleDefinition OpenModule(string assemblyPath)
        {
            return ModuleDefinition.ReadModule(
                assemblyPath,
                new ReaderParameters { ReadWrite = false }
            );
        }

        public static PropertyDefinition GetProperty(
            ModuleDefinition module,
            string type,
            string name
        )
        {
            return module.GetType("FVWeave." + type).Properties.Single(p => p.Name == name);
        }

        public void Dispose() => tempDir.Dispose();
    }

    internal sealed class WeaveResult
    {
        public required string AssemblyPath;
        public List<string> Info = new();
        public List<string> Errors = new();
        public List<string> Warnings = new();
    }
}
