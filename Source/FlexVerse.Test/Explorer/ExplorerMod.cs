// parents: @.frostline/graph/m-explorer.xml

using HarmonyLib;
using Verse;

namespace FlexVerse.Test.Explorer
{
    public partial class ExplorerMod : Mod
    {
        public static ExplorerSettings Settings { get; private set; }

        public ExplorerMod(ModContentPack content)
            : base(content)
        {
            Settings = GetSettings<ExplorerSettings>();

            if (!IsAutomatedRun())
            {
                FlexVerse.Log.Enabled = false;
                new Harmony("flexverse.test.explorer").PatchAll(typeof(ExplorerMod).Assembly);
            }
        }

        internal static bool IsAutomatedRun()
        {
            foreach (var arg in System.Environment.GetCommandLineArgs())
            {
                if (arg.StartsWith("-pickle-run"))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
