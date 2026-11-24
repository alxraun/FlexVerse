// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using FlexVerse;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class CheckboxCheckedScenario : Scenario
    {
        public static new string Description => "The owner accepts proposed checked values.";

        [State]
        private Behavior<bool> CheckedValue { get; set; } = true;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new Checkbox
                {
                    Checked = CheckedValue,
                    OnValueChange = (Action<bool>)(v => CheckedValue = v),
                },
            ];
    }
}
