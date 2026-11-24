// parents: @.frostline/graph/m-dbg-track.xml

using FlexVerse;

namespace FlexTools.WidgetInspector.Isolation
{
    /// <summary>Classifies engines that must stay outside inspection.</summary>
    public static class InspectorIsolation
    {
        /// <summary>Returns whether the engine is the inspector's own or a FlexTools-rooted tree.</summary>
        public static bool IsIsolated(WidgetEngine engine) =>
            engine.Key == InspectorWindow.UiContextKey
            || engine.GetRoot()?.GetType().Assembly == typeof(InspectorIsolation).Assembly;
    }
}
