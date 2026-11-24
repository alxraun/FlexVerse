// parents: @.frostline/graph/m-xunit.xml

using System;
using System.IO;

namespace FlexVerse.Test
{
    /// <summary>
    /// A unique temporary directory for on-disk fixture compilation and IL
    /// weaving, deleted recursively on Dispose. One instance per xunit test
    /// (xunit creates a fresh test-class instance per test), so every weave
    /// fixture leaves nothing behind in the system temp directory.
    /// </summary>
    internal sealed class TempDir : IDisposable
    {
        public DirectoryInfo Info { get; }

        public TempDir(string prefix)
        {
            Info = Directory.CreateTempSubdirectory(prefix);
        }

        public string FullName => Info.FullName;

        public string PathFor(string fileName) => Path.Combine(FullName, fileName);

        public void Dispose()
        {
            try
            {
                Info.Delete(recursive: true);
            }
            catch (IOException) { }
            catch (UnauthorizedAccessException) { }
        }
    }
}
