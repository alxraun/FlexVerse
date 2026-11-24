// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class StaticValueScenario : Scenario
    {
        [State]
        private Behavior<string> Content { get; set; } = "Hello!";

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ActualBox
                {
                    PaddingHorizontal = 60f.Point(),
                    PaddingVertical = 30f.Point(),
                    Label = Content,
                    OnClick = (Action)(
                        () => Content.Target = Content.Target == "Hello!" ? "Goodbye!" : "Hello!"
                    ),
                },
            ];
    }
}
