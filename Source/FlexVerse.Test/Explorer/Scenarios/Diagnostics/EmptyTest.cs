// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    [Screen("Scenarios/Diagnostics/Empty")]
    public partial class EmptyTest : Test
    {
        public override IEnumerable<Widget> BuildRenderTree() => [new EmptyWidget()];

        private partial class EmptyWidget : Widget { }
    }
}
