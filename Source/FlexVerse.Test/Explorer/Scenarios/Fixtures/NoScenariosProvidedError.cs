// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class NoScenariosProvidedError : Scenario
    {
        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new Label { Content = "No scenarios provided" };
        }
    }
}
