// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FocusKeyBubblingScenario : Scenario
    {
        public static new string Description =>
            "A focused TextField bubbles KeyDown to its parent View, which records the key.";

        public override IEnumerable<Widget> BuildRenderTree()
        {
            return [new FkbParent()];
        }

        public partial class FkbParent : Widget
        {
            [State]
            private Behavior<string> LastKey { get; set; } = "none";

            public override void OnKeyDown(FlexEvent e)
            {
                if (e.Character != '\0')
                {
                    LastKey = $"'{e.Character}' ({e.KeyCode})";
                }
                else
                {
                    LastKey = e.KeyCode.ToString();
                }
            }

            public override IEnumerable<Widget> BuildRenderTree()
            {
                yield return new View
                {
                    PaddingAll = 20f.Point(),
                    BorderAll = 1f.Point(),
                    BorderColor = Color.gray,
                    FlexDirection = FlexDirections.Column,
                    FlexShrink = 0,

                    Children = new WidgetList
                    {
                        new Label { Content = $"Last key seen by parent: {LastKey}" },
                        new TextField { Placeholder = "Click me and type" },
                    },
                };
            }
        }
    }
}
