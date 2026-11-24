// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    /// <summary>Prints a node's layout and style for debugging.</summary>
    public delegate void PrintFunc(Node node);

    /// <summary>Logs a formatted message from the layout engine.</summary>
    public delegate int LoggerFunc(
        Config config,
        Node node,
        LogLevel level,
        string format,
        params object[] args
    );
}

namespace FlexVerse
{
    /// <summary>Measures a node and returns its computed size.</summary>
    public delegate Size MeasureFunc(
        Node node,
        float width,
        MeasureMode widthMode,
        float height,
        MeasureMode heightMode
    );

    /// <summary>Returns a node's baseline position for the given size.</summary>
    public delegate float BaselineFunc(Node node, float width, float height);
}
