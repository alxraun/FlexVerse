// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using FlexVerse;
using RimWorld;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class FocusSubmitCancelScenario : Scenario
    {
        public static new string Description =>
            "Two TextFields: click to focus, type natively, Enter submits, Escape cancels.";

        [State]
        private Behavior<string> FirstName { get; set; } = "";

        [State]
        private Behavior<string> LastName { get; set; } = "";

        [State]
        private Behavior<Color> StatusColor { get; set; } = Color.gray;

        [State]
        private Behavior<string> Status { get; set; } =
            "idle - click a field, type, press Enter or Escape";

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
                    new Label { Content = Status, Color = StatusColor },
                    new TextField
                    {
                        Placeholder = "First name",
                        Value = FirstName,
                        OnValueChange = (Action<string>)(v => FirstName = v),
                        OnSubmit = (Action)Submit,
                        OnCancel = (Action)Cancel,
                    },
                    new TextField
                    {
                        Placeholder = "Last name",
                        Value = LastName,
                        OnValueChange = (Action<string>)(v => LastName = v),
                        OnSubmit = (Action)Submit,
                        OnCancel = (Action)Cancel,
                    },
                },
            };
        }

        private void Submit()
        {
            Status = $"submitted: {FirstName.Value} {LastName.Value}";
            StatusColor = Color.green;
            Messages.Message("TextField submitted", MessageTypeDefOf.NeutralEvent);
        }

        private void Cancel()
        {
            Status = "cancelled";
            StatusColor = Color.yellow;
            Messages.Message("TextField cancelled", MessageTypeDefOf.NeutralEvent);
        }
    }
}
