// parents: @.frostline/graph/m-explorer.xml

using LudeonTK;

namespace FlexVerse.Test.Explorer
{
    public static class Actions
    {
        [DebugAction(
            "FlexVerse.Test",
            "FlexVerse Explorer Window",
            actionType = DebugActionType.Action,
            allowedGameStates = AllowedGameStates.Invalid
        )]
        private static void ToggleExplorerWindow()
        {
            Explorer.ToggleWindow();
        }
    }
}
