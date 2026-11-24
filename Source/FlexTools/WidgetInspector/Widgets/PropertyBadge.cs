// parents: @.frostline/graph/m-dbg-ui.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Colored badge showing a property category glyph.</summary>
    public partial class PropertyBadge : Widget
    {
        /// <summary>Gets or sets the property category.</summary>
        [Prop]
        public Behavior<string> Kind { get; set; }

        /// <summary>Builds the styled category badge.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            string label = Kind.Value switch
            {
                "Prop" => "P",
                "State" => "S",
                "FVP" => "F",
                "InheritedContext" => "I",
                _ => "?",
            };
            Color color = Kind.Value switch
            {
                "Prop" => new Color(0.95f, 0.24f, 0.36f),
                "State" => new Color(0.06f, 0.72f, 0.50f),
                _ => Color.gray,
            };

            return
            [
                new View
                {
                    Key = "PropertyBadge_Container",

                    PaddingAll = 2f.Point(),
                    PaddingHorizontal = 4f.Point(),
                    JustifyContent = Justify.Center,
                    AlignItems = Align.Center,
                    BorderAll = 1f.Point(),
                    BorderColor = new Color(color.r, color.g, color.b, 0.2f),
                    BackgroundColor = new Color(color.r, color.g, color.b, 0.1f),
                    MarginRight = 6f.Point(),
                    AlignSelf = Align.Center,
                    Children = new WidgetList([
                        new Label
                        {
                            Key = "PropertyBadge_Label",
                            Content = label,
                            Font = GameFont.Tiny,
                            Color = color,
                        },
                    ]),
                },
            ];
        }
    }
}
