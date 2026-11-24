// parents: @.frostline/graph/m-dbg-ui.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.WidgetInspector.Widgets
{
    /// <summary>Displays the inspector title and live session indicator.</summary>
    public partial class InspectorTitleBar : Widget
    {
        /// <summary>Initializes the title bar layout.</summary>
        public InspectorTitleBar()
        {
            Width = 100f.Percent();
            FlexDirection = FlexDirections.Column;
            FlexShrink = 0;
        }

        /// <summary>Builds the title row with logo and status badge.</summary>
        public override IEnumerable<Widget> BuildRenderTree()
        {
            return
            [
                new View
                {
                    Key = "Inspector_TitleBar_Container",
                    Width = 100f.Percent(),
                    Height = 40f.Point(),
                    FlexDirection = FlexDirections.Row,
                    AlignItems = Align.Center,
                    JustifyContent = Justify.SpaceBetween,
                    PaddingLeft = 16f.Point(),
                    PaddingRight = 16f.Point(),
                    BorderBottom = 1f.Point(),
                    BorderColor = new Color(0.2f, 0.2f, 0.2f),
                    Children = new WidgetList([
                        new View
                        {
                            FlexDirection = FlexDirections.Row,
                            AlignItems = Align.Center,
                            Children = new WidgetList([
                                new View
                                {
                                    Width = 20f.Point(),
                                    Height = 20f.Point(),
                                    BackgroundColor = new Color(0.145f, 0.388f, 0.922f), // Blue-600
                                    AlignItems = Align.Center,
                                    JustifyContent = Justify.Center,
                                    MarginRight = 8f.Point(),
                                    Children = new WidgetList([
                                        new Label
                                        {
                                            Content = "FV",
                                            Font = GameFont.Tiny,
                                            Color = Color.white,
                                        },
                                    ]),
                                },
                                new Label
                                {
                                    Content = "INSPECTOR",
                                    Font = GameFont.Small,
                                    Color = new Color(0.9f, 0.9f, 0.9f),
                                },
                            ]),
                        },
                        new View
                        {
                            FlexDirection = FlexDirections.Row,
                            AlignItems = Align.Center,
                            Children = new WidgetList([
                                new View
                                {
                                    Width = 8f.Point(),
                                    Height = 8f.Point(),
                                    BackgroundColor = new Color(0.1f, 0.8f, 0.1f),
                                    MarginRight = 6f.Point(),
                                },
                                new Label
                                {
                                    Content = "LIVE SESSION",
                                    Font = GameFont.Tiny,
                                    Color = new Color(0.1f, 0.8f, 0.1f),
                                },
                            ]),
                        },
                    ]),
                },
            ];
        }
    }
}
