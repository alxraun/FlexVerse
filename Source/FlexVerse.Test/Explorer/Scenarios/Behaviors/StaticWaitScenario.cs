// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class StaticWaitScenario : Scenario
    {
        [State]
        private Behavior<string> Status { get; set; } = "Click to start";

        public override void OnMount()
        {
            var behavior = Status;
            behavior.OnUpdate += (progress) => Status.Target = $"{progress:P0}";
            behavior.OnComplete += () => Status.Target = "Complete!";
        }

        private void StartWait()
        {
            if (!Status.IsUpdating)
                ((StaticBehavior<string>)Status).Duration = 1f;
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new ActualBox
                {
                    PaddingHorizontal = 60f.Point(),
                    PaddingVertical = 30f.Point(),
                    Label = Status,
                    OnClick = (Action)(() => StartWait()),
                },
            ];
    }
}
