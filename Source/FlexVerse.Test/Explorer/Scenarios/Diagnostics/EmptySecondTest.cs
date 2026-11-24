// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Diagnostics/EmptySecond")]
    public partial class EmptySecondTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() => [new EmptySecondWidget()];

        private partial class EmptySecondWidget : Widget { }
    }
}
