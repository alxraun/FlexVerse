// parents: @.frostline/graph/m-flexbox.xml

using System.Collections.Generic;

namespace FlexVerse
{
    /// <summary>Provides length and widget-list authoring extensions. (M-FLEXBOX)</summary>
    public static class Extensions
    {
        /// <summary>Converts a float into a point length.</summary>
        public static Length Point(this float value) => new(value, Unit.Point);

        /// <summary>Converts an int into a point length.</summary>
        public static Length Point(this int value) => new(value, Unit.Point);

        /// <summary>Converts a float into a percent length.</summary>
        public static Length Percent(this float value) => new(value, Unit.Percent);

        /// <summary>Converts an int into a percent length.</summary>
        public static Length Percent(this int value) => new(value, Unit.Percent);

        /// <summary>Materializes a widget sequence into a widget list.</summary>
        public static WidgetList ToWidgetList(this IEnumerable<Widget> source)
        {
            if (source is null)
            {
                return null;
            }

            if (source is WidgetList widgetList)
            {
                return widgetList;
            }

            return [.. source];
        }
    }
}
