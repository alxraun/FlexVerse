// parents: @.frostline/graph/p-flextools.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexTools.Widgets
{
    /// <summary>Card that groups a titled section of settings controls.</summary>
    public partial class ToolSettingsSection : View
    {
        /// <summary>Gets or sets the section title.</summary>
        [Prop]
        public Behavior<string> Title { get; set; }

        /// <summary>Gets or sets the section content widgets.</summary>
        [Prop]
        public Behavior<WidgetList> Content { get; set; }

        /// <summary>Initializes the section card styling.</summary>
        public ToolSettingsSection()
        {
            PaddingAll = 16f.Point();
            MarginBottom = 10f.Point();
            BackgroundColor = SettingsTheme.Surface;
            BorderAll = 1f.Point();
            BorderColor = SettingsTheme.Border;
            BorderRadius = 7f;
        }

        /// <summary>Builds the title label and content area.</summary>
        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    TextAlign = TextAnchor.MiddleLeft,
                    Content = Title,
                    Font = GameFont.Tiny,
                    Color = SettingsTheme.Muted,
                    MarginBottom = 10f.Point(),
                },
                new View { Children = Content, MinWidth = 0f.Point() },
            ];
    }
}
