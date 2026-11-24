// parents: @.frostline/graph/m-dbg-ui.xml

using System.Collections.Generic;
using FlexTools.WidgetInspector.Models;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Clickable header showing a property's badge, name, and value.</summary>
    public partial class PropertyRowHeader : Button
    {
        protected override MeasureFunc MeasureFunc => null;

        /// <summary>Gets or sets the property snapshot to display.</summary>
        [Prop]
        public Behavior<DebugPropertyNode> Property { get; set; }

        /// <summary>Initializes the header layout.</summary>
        public PropertyRowHeader()
        {
            Height = 24f.Point();
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            DrawButtonBackground = false;
        }

        /// <summary>Builds the badge, name, and value labels.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new PropertyBadge { Kind = Property.Value?.Kind ?? "" },
                new Label
                {
                    Content = Property.Select(value => value?.Name ?? ""),
                    Font = GameFont.Tiny,
                    FlexGrow = 1f,
                    Color = new Color(0.68f, 0.68f, 0.7f),
                    TextAlign = TextAnchor.MiddleLeft,
                },
                new Label
                {
                    Content = Property.Select(value => value?.ValueDisplay ?? ""),
                    Font = GameFont.Tiny,
                    MaxWidth = 240f.Point(),
                    TextAlign = TextAnchor.MiddleRight,
                    Color = new Color(0.45f, 0.7f, 1f),
                },
            ];
    }
}
