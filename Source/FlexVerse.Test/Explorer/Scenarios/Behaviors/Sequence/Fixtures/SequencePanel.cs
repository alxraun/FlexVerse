// parents: @.frostline/graph/m-explorer.xml

using System.Collections.Generic;
using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>Shared experiment workbench; scenarios supply only their contract and controls.</summary>
    public partial class SequencePanel : Widget
    {
        [Prop]
        public Behavior Sequence { get; set; }

        [Prop]
        public Behavior<string> Kicker { get; set; }

        [Prop]
        public Behavior<string> Title { get; set; }

        [Prop]
        public Behavior<string> Instruction { get; set; }

        [Prop]
        public Behavior<string> Expected { get; set; }

        [Prop]
        public Behavior<string> Failure { get; set; }

        [Prop]
        public Behavior<Widget> StageAccessory { get; set; }

        [Prop]
        public Behavior<Widget> Controls { get; set; }

        [Prop]
        public Behavior<bool> ShowEvents { get; set; } = false;

        [Prop]
        public Behavior<SequenceScene> SceneSource { get; set; }

        [State]
        private Behavior<SequenceScene> Scene { get; set; }

        public override void OnMount()
        {
            base.OnMount();
            if (SceneSource?.Value is { } suppliedScene)
                Scene = suppliedScene;
            else if (Sequence is SequenceBehavior<Vector2> sequence)
            {
                var scene = new SequenceScene();
                scene.Initialize(sequence);
                Scene = scene;
            }
        }

        public SequencePanel()
        {
            FlexDirection = FlexDirections.Column;
            AlignItems = Align.Stretch;
            Width = 100f.Percent();
            Height = 100f.Percent();
            PaddingAll = 16f.Point();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            if (Scene.Value is null)
                return [];
            return
            [
                BuildHeading(),
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    FlexGrow = 1,
                    FlexShrink = 1,
                    MinHeight = 0f.Point(),
                    Children = new WidgetList { BuildStage(), BuildInspector() },
                },
            ];
        }

        private Widget BuildHeading() =>
            new View
            {
                MarginBottom = 16f.Point(),
                Children = new WidgetList
                {
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = Kicker,
                        Color = SequenceStyle.Accent,
                        Font = GameFont.Tiny,
                    },
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = Title,
                        Font = GameFont.Medium,
                        MarginTop = 4f.Point(),
                    },
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = Instruction,
                        Color = SequenceStyle.Muted,
                        MarginTop = 4f.Point(),
                    },
                },
            };

        private Widget BuildStage() =>
            new SequenceStage
            {
                Children = new WidgetList
                {
                    new SequenceViewport { Scene = Scene },
                    StageAccessory?.Value,
                },
            };

        private Widget BuildInspector() =>
            new SequenceRail
            {
                Children = new WidgetList
                {
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = "LIVE STATE",
                        Color = SequenceStyle.Muted,
                        Font = GameFont.Tiny,
                    },
                    new SequenceStatusLine { Sequence = Sequence, Scene = Scene },
                    Controls?.Value,
                    BuildContract("EXPECTED", Expected, SequenceStyle.Success),
                    BuildContract("FAILURE SIGNAL", Failure, SequenceStyle.Warning),
                    ShowEvents.Value
                        ? new SequenceEventLog { Sequence = Sequence, FlexGrow = 1 }
                        : null,
                },
            };

        private static Widget BuildContract(string title, Behavior<string> content, Color accent) =>
            new View
            {
                BackgroundColor = SequenceStyle.Surface,
                PaddingAll = 12f.Point(),
                MarginTop = 12f.Point(),
                Children = new WidgetList
                {
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = title,
                        Color = accent,
                        Font = GameFont.Tiny,
                        MarginBottom = 6f.Point(),
                    },
                    new Label
                    {
                        TextAlign = TextAnchor.MiddleLeft,
                        Content = content,
                        Color = SequenceStyle.Text,
                    },
                },
            };
    }
}
