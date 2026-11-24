// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ScenarioHost : Widget
    {
        [Prop]
        public Behavior<List<Type>> Scenarios { get; set; }

        [Prop]
        public Behavior<string> Prefix { get; set; }

        [Prop]
        public Behavior<string> Suffix { get; set; }

        [State]
        private Behavior<Type> ActiveScenario { get; set; }

        public ScenarioHost()
        {
            AlignSelf = Align.Stretch;
        }

        public override void OnMount()
        {
            if (ActiveScenario.Value == null && Scenarios.Value?.Any() == true)
            {
                ActiveScenario = Scenarios.Value.First();
            }
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            yield return new View
            {
                FlexDirection = FlexDirections.Column,
                Width = 100f.Percent(),
                Height = 100f.Percent(),
                Children = new WidgetList
                {
                    new ScenarioSwitcher
                    {
                        Scenarios = Scenarios,
                        OnSelect = new Action<Type>(type => ActiveScenario.Target = type),
                        Prefix = Prefix,
                        Suffix = Suffix,
                    },
                    ActiveScenario.Value is not null
                        ? (Widget)Activator.CreateInstance(ActiveScenario.Value)
                        : new NoScenariosProvidedError { },
                },
            };
        }
    }
}
