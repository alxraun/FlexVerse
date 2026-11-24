// parents: @.frostline/graph/m-pickle.xml

namespace FlexVerse.Test.Pickle
{
    internal static class ScenarioMarkers
    {
        public static void Begin(string module) => Log.Info(module, "scenario:BEGIN");

        public static void End(string module) => Log.Info(module, "scenario:END");
    }
}
