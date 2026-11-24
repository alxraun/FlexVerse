// parents: @.frostline/graph/m-xunit.xml
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FlexVerse.Test
{
    /// <summary>
    /// Locates on-disk artifacts (built FlexVerse.dll and NuGet reference-assembly
    /// packages) needed to compile in-memory fixtures for the generator and weaver
    /// tests. Everything is read from the repository's own .cache/pkg/nuget cache, so
    /// no additional network or machine state is required.
    /// </summary>
    internal static class TestPaths
    {
        private static readonly string Configuration =
#if DEBUG
            "Debug"
#else
            "Release"
#endif
        ;

        public static string RepoRoot { get; } = FindRepoRoot();

        public static string NuGetRoot => Path.Combine(RepoRoot, ".cache", "pkg", "nuget");

        public static string FlexVerseDll =>
            Path.Combine(
                RepoRoot,
                "Source",
                "FlexVerse",
                "bin",
                Configuration,
                "net48",
                "FlexVerse.dll"
            );

        public static string SourceGeneratorsDll =>
            Path.Combine(
                RepoRoot,
                "Source",
                "FlexVerse.SourceGenerators",
                "bin",
                Configuration,
                "netstandard2.0",
                "FlexVerse.SourceGenerators.dll"
            );

        /// <summary>
        /// All .NET Framework 4.8 reference assemblies from the
        /// Microsoft.NETFramework.ReferenceAssemblies.net48 package (mscorlib,
        /// System, System.Core, ...).
        /// </summary>
        public static IReadOnlyList<string> Framework48Refs() =>
            Directory
                .EnumerateFiles(
                    SinglePackageDir("microsoft.netframework.referenceassemblies.net48"),
                    "*.dll",
                    SearchOption.AllDirectories
                )
                .Where(p =>
                    p.Contains($"{Path.DirectorySeparatorChar}v4.8{Path.DirectorySeparatorChar}")
                )
                .Where(p =>
                    !p.EndsWith("Thunk.dll", StringComparison.OrdinalIgnoreCase)
                    && !p.EndsWith("Wrapper.dll", StringComparison.OrdinalIgnoreCase)
                )
                .ToList();

        /// <summary>
        /// All reference DLLs shipped by Krafs.Rimworld.Ref (RimWorld/Verse and
        /// UnityEngine reference assemblies). Used only as *metadata* for
        /// compilation fixtures - never executed.
        /// </summary>
        public static IReadOnlyList<string> RimWorldRefs()
        {
            var dir = Path.Combine(SinglePackageDir("krafs.rimworld.ref"), "ref");
            return Directory.EnumerateFiles(dir, "*.dll", SearchOption.AllDirectories).ToList();
        }

        public static IReadOnlyList<string> HarmonyRefs() =>
            Directory
                .EnumerateFiles(
                    SinglePackageDir("lib.harmony.ref"),
                    "0Harmony.dll",
                    SearchOption.AllDirectories
                )
                .ToList();

        /// <summary>
        /// The full metadata reference set for compiling fixtures that use
        /// FlexVerse types (Widget, Behavior&lt;T&gt;, [Prop]/[State]).
        /// </summary>
        public static IReadOnlyList<string> FlexVerseFixtureRefs()
        {
            var refs = new List<string>();
            refs.AddRange(Framework48Refs());
            refs.AddRange(RimWorldRefs());
            refs.AddRange(HarmonyRefs());
            refs.Add(FlexVerseDll);
            return refs;
        }

        private static string SinglePackageDir(string packageId)
        {
            var packageRoot = Path.Combine(NuGetRoot, packageId);
            var versions = Directory
                .EnumerateDirectories(packageRoot)
                .Where(d => !Path.GetFileName(d).StartsWith("."))
                .OrderByDescending(d => d, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (versions.Count == 0)
                throw new InvalidOperationException(
                    $"NuGet package '{packageId}' not found under {packageRoot}."
                );
            return versions[0];
        }

        private static string FindRepoRoot()
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            while (dir != null)
            {
                if (
                    File.Exists(
                        Path.Combine(dir.FullName, "Source", "FlexVerse", "FlexVerse.csproj")
                    )
                )
                    return dir.FullName;
                dir = dir.Parent;
            }
            throw new InvalidOperationException(
                $"Could not locate the FlexVerse repository root above {AppContext.BaseDirectory}."
            );
        }
    }
}
