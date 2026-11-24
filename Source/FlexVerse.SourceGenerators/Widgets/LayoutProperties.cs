// parents: @.frostline/graph/m-widget-emitter.xml

using System;
using System.Collections.Generic;
using FlexVerse.SourceGenerators.Widgets.Models;

namespace FlexVerse.SourceGenerators.Widgets
{
    internal static class LayoutProperties
    {
        internal enum ApplyKind
        {
            Direct,
            DimensionAuto,
            Dimension,
            FlexBasis,
            Edge,
        }

        internal sealed record Route(ApplyKind Kind, string Method = "", string Edge = "");

        private static readonly Dictionary<string, Route> Routes = CreateRoutes();

        /// <summary>Performs the Contains operation.</summary>
        public static bool Contains(string name) => Routes.ContainsKey(name);

        /// <summary>Performs the Applies To operation.</summary>
        public static bool AppliesTo(PropertyModel prop) =>
            prop.Kind == PropertyKind.Prop
            && prop.SlotOwner == "FlexVerse.Widget"
            && Contains(prop.Name);

        /// <summary>Performs the Get Route operation.</summary>
        public static Route GetRoute(string name) =>
            Routes.TryGetValue(name, out var route)
                ? route
                : throw new InvalidOperationException(
                    $"Missing Flexbox layout mapping for '{name}'"
                );

        private static Dictionary<string, Route> CreateRoutes()
        {
            var routes = new Dictionary<string, Route>(StringComparer.Ordinal);
            foreach (
                var name in new[]
                {
                    "Direction",
                    "FlexDirection",
                    "AlignItems",
                    "AlignSelf",
                    "AlignContent",
                    "JustifyContent",
                    "FlexWrap",
                    "Overflow",
                    "PositionType",
                    "Display",
                    "FlexGrow",
                    "FlexShrink",
                    "AspectRatio",
                }
            )
                routes.Add(name, new Route(ApplyKind.Direct));
            foreach (var name in new[] { "Width", "Height" })
                routes.Add(name, new Route(ApplyKind.DimensionAuto));
            foreach (var name in new[] { "MinWidth", "MinHeight", "MaxWidth", "MaxHeight" })
                routes.Add(name, new Route(ApplyKind.Dimension));
            routes.Add("FlexBasis", new Route(ApplyKind.FlexBasis));
            foreach (var method in new[] { "Margin", "Padding", "Border", "Position" })
            foreach (
                var edge in new[]
                {
                    "All",
                    "Left",
                    "Top",
                    "Right",
                    "Bottom",
                    "Start",
                    "End",
                    "Horizontal",
                    "Vertical",
                }
            )
            {
                if (method == "Position" && edge == "All")
                    continue;
                routes.Add(method + edge, new Route(ApplyKind.Edge, method, edge));
            }
            return routes;
        }
    }
}
