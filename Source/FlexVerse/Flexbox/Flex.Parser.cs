// parents: @.frostline/graph/m-flexbox.xml

// Vendored from https://github.com/ru-ace/Flexbox (a pure C# port of
// Facebook's Yoga), BSD 3-Clause. Full license text: THIRD-PARTY-NOTICES.md
// at the repository root.

using System;
using System.Collections.Generic;

namespace FlexVerse.Flexbox
{
    /// <summary>Implements style parsing and string conversions for layout values. (M-FLEXBOX)</summary>
    public partial class Flex
    {
        #region XXXX_ToSring

        /// <summary>Converts an Align value to its CSS-style string.</summary>
        public static string AlignToString(Align value)
        {
            switch (value)
            {
                case Align.Auto:
                    return "auto";
                case Align.FlexStart:
                    return "flex-start";
                case Align.Center:
                    return "center";
                case Align.FlexEnd:
                    return "flex-end";
                case Align.Stretch:
                    return "stretch";
                case Align.Baseline:
                    return "baseline";
                case Align.SpaceBetween:
                    return "space-between";
                case Align.SpaceAround:
                    return "space-around";
            }
            return "unknown";
        }

        /// <summary>Converts a Dimension value to its CSS-style string.</summary>
        public static string DimensionToString(Dimension value)
        {
            switch (value)
            {
                case Dimension.Width:
                    return "width";
                case Dimension.Height:
                    return "height";
            }
            return "unknown";
        }

        /// <summary>Converts a Directions value to its CSS-style string.</summary>
        public static string DirectionToString(Directions value)
        {
            switch (value)
            {
                case Directions.Inherit:
                    return "inherit";
                case Directions.LTR:
                    return "ltr";
                case Directions.RTL:
                    return "rtl";
            }
            return "unknown";
        }

        /// <summary>Converts a Displays value to its CSS-style string.</summary>
        public static string DisplayToString(Displays value)
        {
            switch (value)
            {
                case Displays.Flex:
                    return "flex";
                case Displays.None:
                    return "none";
            }
            return "unknown";
        }

        /// <summary>Converts an Edge value to its CSS-style string.</summary>
        public static string EdgeToString(Edge value)
        {
            switch (value)
            {
                case Edge.Left:
                    return "left";
                case Edge.Top:
                    return "top";
                case Edge.Right:
                    return "right";
                case Edge.Bottom:
                    return "bottom";
                case Edge.Start:
                    return "start";
                case Edge.End:
                    return "end";
                case Edge.Horizontal:
                    return "horizontal";
                case Edge.Vertical:
                    return "vertical";
                case Edge.All:
                    return "all";
            }
            return "unknown";
        }

        /// <summary>Converts an ExperimentalFeature value to its CSS-style string.</summary>
        public static string ExperimentalFeatureToString(ExperimentalFeature value)
        {
            switch (value)
            {
                case ExperimentalFeature.WebFlexBasis:
                    return "web-flex-basis";
            }
            return "unknown";
        }

        /// <summary>Converts a FlexDirections value to its CSS-style string.</summary>
        public static string FlexDirectionToString(FlexDirections value)
        {
            switch (value)
            {
                case FlexDirections.Column:
                    return "column";
                case FlexDirections.ColumnReverse:
                    return "column-reverse";
                case FlexDirections.Row:
                    return "row";
                case FlexDirections.RowReverse:
                    return "row-reverse";
            }
            return "unknown";
        }

        /// <summary>Converts a Justify value to its CSS-style string.</summary>
        public static string JustifyToString(Justify value)
        {
            switch (value)
            {
                case Justify.FlexStart:
                    return "flex-start";
                case Justify.Center:
                    return "center";
                case Justify.FlexEnd:
                    return "flex-end";
                case Justify.SpaceBetween:
                    return "space-between";
                case Justify.SpaceAround:
                    return "space-around";
            }
            return "unknown";
        }

        /// <summary>Converts a LogLevel value to its CSS-style string.</summary>
        public static string LogLevelToString(LogLevel value)
        {
            switch (value)
            {
                case LogLevel.Error:
                    return "error";
                case LogLevel.Warn:
                    return "warn";
                case LogLevel.Info:
                    return "info";
                case LogLevel.Debug:
                    return "debug";
                case LogLevel.Verbose:
                    return "verbose";
                case LogLevel.Fatal:
                    return "fatal";
            }
            return "unknown";
        }

        /// <summary>Converts a MeasureMode value to its CSS-style string.</summary>
        public static string MeasureModeToString(MeasureMode value)
        {
            switch (value)
            {
                case MeasureMode.Undefined:
                    return "undefined";
                case MeasureMode.Exactly:
                    return "exactly";
                case MeasureMode.AtMost:
                    return "at-most";
            }
            return "unknown";
        }

        /// <summary>Converts a NodeType value to its CSS-style string.</summary>
        public static string NodeTypeToString(NodeType value)
        {
            switch (value)
            {
                case NodeType.Default:
                    return "default";
                case NodeType.Text:
                    return "text";
            }
            return "unknown";
        }

        /// <summary>Converts an Overflows value to its CSS-style string.</summary>
        public static string OverflowToString(Overflows value)
        {
            switch (value)
            {
                case Overflows.Visible:
                    return "visible";
                case Overflows.Hidden:
                    return "hidden";
                case Overflows.Scroll:
                    return "scroll";
            }
            return "unknown";
        }

        /// <summary>Converts a PositionTypes value to its CSS-style string.</summary>
        public static string PositionTypeToString(PositionTypes value)
        {
            switch (value)
            {
                case PositionTypes.Relative:
                    return "relative";
                case PositionTypes.Absolute:
                    return "absolute";
            }
            return "unknown";
        }

        /// <summary>Converts a PrintOptions value to its CSS-style string.</summary>
        public static string PrintOptionsToString(PrintOptions value)
        {
            switch (value)
            {
                case PrintOptions.Layout:
                    return "layout";
                case PrintOptions.Style:
                    return "style";
                case PrintOptions.Children:
                    return "children";
            }
            return "unknown";
        }

        /// <summary>Converts a Unit value to its CSS-style string.</summary>
        public static string UnitToString(Unit value)
        {
            switch (value)
            {
                case Unit.Undefined:
                    return "undefined";
                case Unit.Point:
                    return "point";
                case Unit.Percent:
                    return "percent";
                case Unit.Auto:
                    return "auto";
            }
            return "unknown";
        }

        /// <summary>Converts a Wrap value to its CSS-style string.</summary>
        public static string WrapToString(Wrap value)
        {
            switch (value)
            {
                case Wrap.NoWrap:
                    return "no-wrap";
                case Wrap.Wrap:
                    return "wrap";
                case Wrap.WrapReverse:
                    return "wrap-reverse";
            }
            return "unknown";
        }
        #endregion

        #region StringTo_XXXX

        /// <summary>Parses a CSS-style string into an Align value.</summary>
        public static bool StringToAlign(string value, out Align result)
        {
            switch (value)
            {
                case "auto":
                    result = Align.Auto;
                    return true;
                case "flex-start":
                    result = Align.FlexStart;
                    return true;
                case "center":
                    result = Align.Center;
                    return true;
                case "flex-end":
                    result = Align.FlexEnd;
                    return true;
                case "stretch":
                    result = Align.Stretch;
                    return true;
                case "baseline":
                    result = Align.Baseline;
                    return true;
                case "space-between":
                    result = Align.SpaceBetween;
                    return true;
                case "space-around":
                    result = Align.SpaceAround;
                    return true;
            }
            result = Align.Auto;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Dimension value.</summary>
        public static bool StringToDimension(string value, out Dimension result)
        {
            switch (value)
            {
                case "width":
                    result = Dimension.Width;
                    return true;
                case "height":
                    result = Dimension.Height;
                    return true;
            }
            result = Dimension.Width;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Directions value.</summary>
        public static bool StringToDirection(string value, out Directions result)
        {
            switch (value)
            {
                case "inherit":
                    result = Directions.Inherit;
                    return true;
                case "ltr":
                    result = Directions.LTR;
                    return true;
                case "rtl":
                    result = Directions.RTL;
                    return true;
            }
            result = Directions.Inherit;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Displays value.</summary>
        public static bool StringToDisplay(string value, out Displays result)
        {
            switch (value)
            {
                case "flex":
                    result = Displays.Flex;
                    return true;
                case "none":
                    result = Displays.None;
                    return true;
            }
            result = Displays.Flex;
            return false;
        }

        /// <summary>Parses a CSS-style string into an Edge value.</summary>
        public static bool StringToEdge(string value, out Edge result)
        {
            switch (value)
            {
                case "left":
                    result = Edge.Left;
                    return true;
                case "top":
                    result = Edge.Top;
                    return true;
                case "right":
                    result = Edge.Right;
                    return true;
                case "bottom":
                    result = Edge.Bottom;
                    return true;
                case "start":
                    result = Edge.Start;
                    return true;
                case "end":
                    result = Edge.End;
                    return true;
                case "horizontal":
                    result = Edge.Horizontal;
                    return true;
                case "vertical":
                    result = Edge.Vertical;
                    return true;
                case "all":
                    result = Edge.All;
                    return true;
            }
            result = Edge.Left;
            return false;
        }

        /// <summary>Parses a CSS-style string into an ExperimentalFeature value.</summary>
        public static bool StringToExperimentalFeature(string value, out ExperimentalFeature result)
        {
            switch (value)
            {
                case "web-flex-basis":
                    result = ExperimentalFeature.WebFlexBasis;
                    return true;
            }
            result = ExperimentalFeature.WebFlexBasis;
            return false;
        }

        /// <summary>Parses a CSS-style string into a FlexDirections value.</summary>
        public static bool StringToFlexDirection(string value, out FlexDirections result)
        {
            switch (value)
            {
                case "column":
                    result = FlexDirections.Column;
                    return true;
                case "column-reverse":
                    result = FlexDirections.ColumnReverse;
                    return true;
                case "row":
                    result = FlexDirections.Row;
                    return true;
                case "row-reverse":
                    result = FlexDirections.RowReverse;
                    return true;
            }
            result = FlexDirections.Column;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Justify value.</summary>
        public static bool StringToJustify(string value, out Justify result)
        {
            switch (value)
            {
                case "flex-start":
                    result = Justify.FlexStart;
                    return true;
                case "center":
                    result = Justify.Center;
                    return true;
                case "flex-end":
                    result = Justify.FlexEnd;
                    return true;
                case "space-between":
                    result = Justify.SpaceBetween;
                    return true;
                case "space-around":
                    result = Justify.SpaceAround;
                    return true;
            }
            result = Justify.FlexStart;
            return false;
        }

        /// <summary>Parses a CSS-style string into a LogLevel value.</summary>
        public static bool StringToLogLevel(string value, out LogLevel result)
        {
            switch (value)
            {
                case "error":
                    result = LogLevel.Error;
                    return true;
                case "warn":
                    result = LogLevel.Warn;
                    return true;
                case "info":
                    result = LogLevel.Info;
                    return true;
                case "debug":
                    result = LogLevel.Debug;
                    return true;
                case "verbose":
                    result = LogLevel.Verbose;
                    return true;
                case "fatal":
                    result = LogLevel.Fatal;
                    return true;
            }
            result = LogLevel.Error;
            return false;
        }

        /// <summary>Parses a CSS-style string into a MeasureMode value.</summary>
        public static bool StringToMeasureMode(string value, out MeasureMode result)
        {
            switch (value)
            {
                case "undefined":
                    result = MeasureMode.Undefined;
                    return true;
                case "exactly":
                    result = MeasureMode.Exactly;
                    return true;
                case "at-most":
                    result = MeasureMode.AtMost;
                    return true;
            }
            result = MeasureMode.Undefined;
            return false;
        }

        /// <summary>Parses a CSS-style string into a NodeType value.</summary>
        public static bool StringToNodeType(string value, out NodeType result)
        {
            switch (value)
            {
                case "default":
                    result = NodeType.Default;
                    return true;
                case "text":
                    result = NodeType.Text;
                    return true;
            }
            result = NodeType.Default;
            return false;
        }

        /// <summary>Parses a CSS-style string into an Overflows value.</summary>
        public static bool StringToOverflow(string value, out Overflows result)
        {
            switch (value)
            {
                case "visible":
                    result = Overflows.Visible;
                    return true;
                case "hidden":
                    result = Overflows.Hidden;
                    return true;
                case "scroll":
                    result = Overflows.Scroll;
                    return true;
            }
            result = Overflows.Visible;
            return false;
        }

        /// <summary>Parses a CSS-style string into a PositionTypes value.</summary>
        public static bool StringToPositionType(string value, out PositionTypes result)
        {
            switch (value)
            {
                case "relative":
                    result = PositionTypes.Relative;
                    return true;
                case "absolute":
                    result = PositionTypes.Absolute;
                    return true;
            }
            result = PositionTypes.Relative;
            return false;
        }

        /// <summary>Parses a CSS-style string into a PrintOptions value.</summary>
        public static bool StringToPrintOptions(string value, out PrintOptions result)
        {
            switch (value)
            {
                case "layout":
                    result = PrintOptions.Layout;
                    return true;
                case "style":
                    result = PrintOptions.Style;
                    return true;
                case "children":
                    result = PrintOptions.Children;
                    return true;
            }
            result = PrintOptions.Layout;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Unit value.</summary>
        public static bool StringToUnit(string value, out Unit result)
        {
            switch (value)
            {
                case "undefined":
                    result = Unit.Undefined;
                    return true;
                case "point":
                    result = Unit.Point;
                    return true;
                case "percent":
                    result = Unit.Percent;
                    return true;
                case "auto":
                    result = Unit.Auto;
                    return true;
            }
            result = Unit.Undefined;
            return false;
        }

        /// <summary>Parses a CSS-style string into a Wrap value.</summary>
        public static bool StringToWrap(string value, out Wrap result)
        {
            switch (value)
            {
                case "nowrap":
                    result = Wrap.NoWrap;
                    return true;
                case "wrap":
                    result = Wrap.Wrap;
                    return true;
                case "wrap-reverse":
                    result = Wrap.WrapReverse;
                    return true;
            }
            result = Wrap.NoWrap;
            return false;
        }

        #endregion

        readonly static Dictionary<string, Length> valueDictionaryCache = [];

        /// <summary>Parses a length value such as "12px" or "50%" into a Length.</summary>
        public static bool ParseValueFromString(string text, out Length result)
        {
            bool parsed = true;
            if (text == "auto")
            {
                result = new Length(float.NaN, Unit.Auto);
                return true;
            }

            if (valueDictionaryCache.TryGetValue(text, out var cachedValue))
            {
                result = cachedValue;
                return true;
            }

            string dig = text;
            Unit uu = Unit.Point;

            if (text.EndsWith("%"))
            {
                dig = text.Substring(0, text.Length - 1);
                uu = Unit.Percent;
            }
            else if (text.EndsWith("px"))
            {
                dig = text.Substring(0, text.Length - 2);
                uu = Unit.Point;
            }

            Length res;
            if (
                float.TryParse(
                    dig,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out var parsedValue
                )
            )
            {
                res = new Length(parsedValue, uu);
            }
            else
            {
                parsed = false;
                res = Length.Undefined;
            }
            valueDictionaryCache.Add(text, res);
            result = res;
            return parsed;
        }

        /// <summary>Parses one to four length values into per-edge values.</summary>
        public static bool ParseFourValueFromString(string text, out Length[] result)
        {
            result = null;
            var vStr = text.Split([' '], StringSplitOptions.RemoveEmptyEntries);
            if (vStr.Length > 4)
                return false;

            Length[] res = new Length[vStr.Length];
            for (int i = 0; i < res.Length; i++)
                if (!ParseValueFromString(vStr[i], out res[i]))
                    return false;

            switch (res.Length)
            {
                case 0:
                    result = null;
                    break;
                case 1:
                    result =
                    [
                        res[0],
                        res[0],
                        res[0],
                        res[0],
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                    ];
                    break;
                case 2:
                    result =
                    [
                        res[1],
                        res[0],
                        res[1],
                        res[0],
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                    ];
                    break;
                case 3:
                    result =
                    [
                        res[1],
                        res[0],
                        res[1],
                        res[2],
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                    ];
                    break;
                case 4:
                    result =
                    [
                        res[3],
                        res[0],
                        res[1],
                        res[2],
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                        Length.Undefined,
                    ];
                    break;
                default:
                    result = null;
                    break;
            }
            return result != null;
        }

        static bool ParseBreakWork(string input, out string head, out string tail)
        {
            head = "";
            tail = "";
            var t = input.Split(['-'], StringSplitOptions.RemoveEmptyEntries);
            switch (t.Length)
            {
                case 1:
                    head = t[0];
                    return true;
                case 2:
                case 3:
                    head = t[0];
                    tail = t[1];
                    if (head == "border" && tail == "width")
                        tail = "";
                    return true;
            }
            return false;
        }

        /// <summary>Renders the string value of a style attribute.</summary>
        public static string RenderStyleAttrValue(Style style, string attrKey)
        {
            switch (attrKey)
            {
                case "direction":
                    break;
                case "flex-direction":
                    break;
                case "justify-content":
                    break;
                case "align-content":
                    break;
                case "align-items":
                    break;
                case "align-self":
                    break;
                case "flex-wrap":
                    break;
                case "overflow":
                    break;
                case "display":
                    break;
                case "flex-grow":
                    break;
                case "flex-shrink":
                    break;
                case "flex-basis":
                    break;
                case "position":
                    break;
                case "width":
                    break;
                case "height":
                    break;
                case "min-width":
                    break;
                case "min-height":
                    break;
                case "max-width":
                    break;
                case "max-height":
                    break;
            }

            return "";
        }

        /// <summary>Parses a style attribute value onto the given style.</summary>
        public static bool ParseStyleAttr(Style style, string attrKey, string attrValue)
        {
            bool parsed = true;
            switch (attrKey)
            {
                case "direction":
                    parsed = StringToDirection(attrValue, out style.Direction);
                    break;
                case "flex-direction":
                    parsed = StringToFlexDirection(attrValue, out style.FlexDirection);
                    break;
                case "justify-content":
                    parsed = StringToJustify(attrValue, out style.JustifyContent);
                    break;
                case "align-content":
                    parsed = StringToAlign(attrValue, out style.AlignContent);
                    break;
                case "align-items":
                    parsed = StringToAlign(attrValue, out style.AlignItems);
                    break;
                case "align-self":
                    parsed = StringToAlign(attrValue, out style.AlignSelf);
                    break;
                case "flex-wrap":
                    parsed = StringToWrap(attrValue, out style.FlexWrap);
                    break;
                case "overflow":
                    parsed = StringToOverflow(attrValue, out style.Overflow);
                    break;
                case "display":
                    parsed = StringToDisplay(attrValue, out style.Display);
                    break;
                case "flex-grow":
                    parsed = float.TryParse(
                        attrValue,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out style.FlexGrow
                    );
                    break;
                case "flex-shrink":
                    parsed = float.TryParse(
                        attrValue,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out style.FlexShrink
                    );
                    break;
                case "flex-basis":
                    parsed = ParseValueFromString(attrValue, out style.FlexBasis);
                    break;
                case "position":
                    parsed = StringToPositionType(attrValue, out style.PositionType);
                    break;
                case "left":
                    parsed = ParseValueFromString(attrValue, out style.Position[(int)Edge.Left]);
                    break;
                case "top":
                    parsed = ParseValueFromString(attrValue, out style.Position[(int)Edge.Top]);
                    break;
                case "right":
                    parsed = ParseValueFromString(attrValue, out style.Position[(int)Edge.Right]);
                    break;
                case "bottom":
                    parsed = ParseValueFromString(attrValue, out style.Position[(int)Edge.Bottom]);
                    break;
                case "width":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.Dimensions[(int)Dimension.Width]
                    );
                    break;
                case "height":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.Dimensions[(int)Dimension.Height]
                    );
                    break;
                case "min-width":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.MinDimensions[(int)Dimension.Width]
                    );
                    break;
                case "min-height":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.MinDimensions[(int)Dimension.Height]
                    );
                    break;
                case "max-width":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.MaxDimensions[(int)Dimension.Width]
                    );
                    break;
                case "max-height":
                    parsed = ParseValueFromString(
                        attrValue,
                        out style.MaxDimensions[(int)Dimension.Height]
                    );
                    break;
                case "margin":
                case "margin-left":
                case "margin-right":
                case "margin-top":
                case "margin-bottom":
                case "padding":
                case "padding-left":
                case "padding-right":
                case "padding-top":
                case "padding-bottom":
                case "border-width":
                case "border-left-width":
                case "border-right-width":
                case "border-top-width":
                case "border-bottom-width":

                    if (ParseBreakWork(attrKey, out string head, out string tail))
                    {
                        if (tail == "")
                        {
                            switch (head)
                            {
                                case "margin":
                                    parsed = ParseFourValueFromString(attrValue, out style.Margin);
                                    break;
                                case "padding":
                                    parsed = ParseFourValueFromString(attrValue, out style.Padding);
                                    break;
                                case "border":
                                    parsed = ParseFourValueFromString(attrValue, out style.Border);
                                    break;
                                default:
                                    parsed = false;
                                    break;
                            }
                        }
                        else if (StringToEdge(tail, out Edge edge))
                        {
                            switch (head)
                            {
                                case "margin":
                                    parsed = ParseValueFromString(
                                        attrValue,
                                        out style.Margin[(int)edge]
                                    );
                                    break;
                                case "padding":
                                    parsed = ParseValueFromString(
                                        attrValue,
                                        out style.Padding[(int)edge]
                                    );
                                    break;
                                case "border":
                                    parsed = ParseValueFromString(
                                        attrValue,
                                        out style.Border[(int)edge]
                                    );
                                    break;
                                default:
                                    parsed = false;
                                    break;
                            }
                        }
                        else
                            parsed = false;
                    }
                    else
                        parsed = false;
                    break;
                default:
                    parsed = false;
                    break;
            }

            return parsed;
        }
    }
}
