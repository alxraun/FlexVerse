// parents: @.frostline/graph/m-dbg-ui.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Status bar showing memory usage and the hotkey hint.</summary>
    public partial class InspectorFooter : Widget
    {
        /// <summary>Builds the footer status labels.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    Key = "Inspector_Footer_Container",
                    Height = 24f.Point(),
                    FlexDirection = FlexDirections.Row,
                    AlignItems = Align.Center,
                    PaddingLeft = 12f.Point(),
                    BorderTop = 1f.Point(),
                    BorderColor = new Color(0.2f, 0.2f, 0.2f),
                    BackgroundColor = new Color(0.08f, 0.08f, 0.08f),
                    Children = new WidgetList([
                        new Label
                        {
                            Content = $"Memory: {(GC.GetTotalMemory(false) / 1024 / 1024)}MB",
                            Font = GameFont.Tiny,
                            Color = new Color(0.4f, 0.4f, 0.4f),
                            MarginRight = 16f.Point(),
                        },
                        new Label
                        {
                            Content = "F12 · Toggle inspector",
                            Font = GameFont.Tiny,
                            Color = new Color(0.4f, 0.4f, 0.4f),
                        },
                    ]),
                },
            ];
        }
    }
}
