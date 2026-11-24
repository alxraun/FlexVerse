// parents: @.frostline/graph/m-explorer.xml

using Verse;

namespace FlexVerse.Test.Explorer
{
    public partial class DebugData : IExposable
    {
        public string lastExplorerScreenPath;

        public void ExposeData()
        {
            Scribe_Values.Look(ref lastExplorerScreenPath, "lastExplorerScreenPath");
        }
    }
}
