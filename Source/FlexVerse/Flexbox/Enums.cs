// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

namespace FlexVerse.Flexbox
{
    /// <summary>Identifies a layout dimension of a node.</summary>
    public enum Dimension
    {
        Width,
        Height,
    }

    /// <summary>Lists opt-in experimental layout features.</summary>
    public enum ExperimentalFeature
    {
        WebFlexBasis,
    }

    /// <summary>Defines severity levels for layout log messages.</summary>
    public enum LogLevel
    {
        Error,
        Warn,
        Info,
        Debug,
        Verbose,
        Fatal,
    }

    /// <summary>Distinguishes default nodes from text nodes.</summary>
    public enum NodeType
    {
        Default,
        Text,
    }

    /// <summary>Selects which sections to include in debug layout output.</summary>
    public enum PrintOptions
    {
        Layout,
        Style,
        Children,
    }
}

namespace FlexVerse
{
    /// <summary>Defines cross-axis alignment for flex children.</summary>
    public enum Align
    {
        Auto,
        FlexStart,
        Center,
        FlexEnd,
        Stretch,
        Baseline,
        SpaceBetween,
        SpaceAround,
    }

    /// <summary>Defines text layout direction, including inherited values.</summary>
    public enum Directions
    {
        Inherit = 0,
        LTR,
        RTL,
        NeverUsed_1 = -1,
    }

    /// <summary>Controls whether a node participates in flex layout.</summary>
    public enum Displays
    {
        Flex,
        None,
    }

    /// <summary>Defines the main-axis direction of a flex container.</summary>
    public enum FlexDirections
    {
        Column,
        ColumnReverse,
        Row,
        RowReverse,
    }

    /// <summary>Defines main-axis distribution of flex children.</summary>
    public enum Justify
    {
        FlexStart,
        Center,
        FlexEnd,
        SpaceBetween,
        SpaceAround,
    }

    /// <summary>Defines how content overflowing a node is handled.</summary>
    public enum Overflows
    {
        Visible,
        Hidden,
        Scroll,
    }

    /// <summary>Defines whether a node is positioned relatively or absolutely.</summary>
    public enum PositionTypes
    {
        Relative,
        Absolute,
    }

    /// <summary>Defines line wrapping behavior for flex children.</summary>
    public enum Wrap
    {
        NoWrap,
        Wrap,
        WrapReverse,
    }

    /// <summary>Defines how a measured dimension is constrained.</summary>
    public enum MeasureMode : int
    {
        Undefined = 0,
        Exactly,
        AtMost,
        NeverUsed_1 = -1,
    }

    /// <summary>Identifies a box edge for margins, padding and borders.</summary>
    public enum Edge : int
    {
        Left,
        Top,
        Right,
        Bottom,
        Start,
        End,
        Horizontal,
        Vertical,
        All,
    }

    /// <summary>Defines the unit of a length value.</summary>
    public enum Unit
    {
        Undefined,
        Point,
        Percent,
        Auto,
    }
}
