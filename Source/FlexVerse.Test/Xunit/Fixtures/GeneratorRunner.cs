// parents: @.frostline/graph/m-xunit.xml

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using FlexVerse.SourceGenerators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Xunit;

namespace FlexVerse.Test
{
    /// <summary>
    /// Single canonical in-process runner for FlexVerseGenerator: parses the
    /// fixture source, compiles against the FlexVerse fixture references,
    /// runs the generator, and returns the joined .g.cs sources plus the
    /// merged compilation and generator diagnostics.
    /// </summary>
    internal static class GeneratorRunner
    {
        public static GeneratorResult Run(string source, string? assemblyName = null)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(
                source,
                new CSharpParseOptions(LanguageVersion.Latest)
            );
            var references = TestPaths
                .FlexVerseFixtureRefs()
                .Select(r => MetadataReference.CreateFromFile(r))
                .Cast<MetadataReference>()
                .ToList();

            var compilation = CSharpCompilation.Create(
                assemblyName ?? "FlexVerse.Generator.Fixtures",
                new[] { syntaxTree },
                references,
                new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var driver = CSharpGeneratorDriver.Create(
                new[] { new FlexVerseGenerator().AsSourceGenerator() }
            );
            var driverResult = driver.RunGeneratorsAndUpdateCompilation(
                compilation,
                out var updatedCompilation,
                out _
            );

            var generatedSource = string.Join(
                "\n",
                driverResult
                    .GetRunResult()
                    .Results.SelectMany(r => r.GeneratedSources)
                    .Where(gs => gs.HintName.EndsWith(".g.cs"))
                    .Select(gs => gs.SourceText.ToString())
                    .Where(sourceText => sourceText.Length > 0)
            );

            var diagnostics = updatedCompilation
                .GetDiagnostics()
                .Concat(driverResult.GetRunResult().Results.SelectMany(r => r.Diagnostics))
                .ToList();
            return new GeneratorResult
            {
                Diagnostics = diagnostics.ToImmutableArray(),
                GeneratedSource = generatedSource,
                UpdatedCompilation = updatedCompilation,
            };
        }
    }

    internal sealed class GeneratorResult
    {
        public required ImmutableArray<Diagnostic> Diagnostics { get; init; }

        public required string GeneratedSource { get; init; }

        /// <summary>Generator-updated compilation for emit-and-load tests.</summary>
        public required Compilation UpdatedCompilation { get; init; }

        public List<Diagnostic> Errors =>
            Diagnostics.Where(d => d.Severity == DiagnosticSeverity.Error).ToList();

        /// <summary>
        /// Deconstructs to the historical (diagnostics, generatedSource)
        /// shape so generator tests read as before.
        /// </summary>
        public void Deconstruct(
            out ImmutableArray<Diagnostic> diagnostics,
            out string generatedSource
        )
        {
            diagnostics = Diagnostics;
            generatedSource = GeneratedSource;
        }

        public void AssertNoErrors()
        {
            var errors = Errors;
            Assert.True(
                errors.Count == 0,
                "Expected zero compilation errors, got:\n"
                    + string.Join("\n", errors.Select(d => $"{d.Id}: {d.GetMessage()}"))
            );
        }
    }
}
