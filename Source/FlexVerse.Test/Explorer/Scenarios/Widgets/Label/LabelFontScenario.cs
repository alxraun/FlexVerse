// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class LabelFontScenario : Scenario
    {
        public static new string Description =>
            "Compare the three game fonts and their natural heights.";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Label
                {
                    Content = "Tiny, Small, Medium: each label measures its own height.",
                    Width = 560f.Point(),
                    MarginBottom = 12f.Point(),
                },
                new View
                {
                    AlignItems = Align.FlexStart,
                    Children = new WidgetList(
                        Enum.GetValues(typeof(GameFont))
                            .Cast<GameFont>()
                            .Select(font => new View
                            {
                                BackgroundColor = Color.gray,
                                Children = new WidgetList
                                {
                                    new Label { Content = $"{font}: Sample text", Font = font },
                                },
                            })
                    ),
                },
            ];
    }
}
