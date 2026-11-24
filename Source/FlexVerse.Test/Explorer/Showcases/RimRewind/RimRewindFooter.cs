// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindFooter : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindFooter()
        {
            Height = 56f.Point();
            MarginTop = 12f.Point();
            PaddingHorizontal = 16f.Point();
            BackgroundColor = RimRewindSkin.Surface;
            BorderRadius = RimRewindSkin.CornerRadius;
            BorderTop = 0f.Point();
            BorderColor = RimRewindSkin.Line;
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButton
                {
                    Label = "Reset section",
                    Width = 164f.Point(),
                    Compact = true,
                    Quiet = true,
                    Disabled = _screen.Query.Select(query => !string.IsNullOrWhiteSpace(query)),
                    Tooltip = "Restore default values for the selected section",
                    OnActivate = new Action(_screen.HandleResetCategory),
                },
                new Label
                {
                    Content = Group.Combine(_screen.Status, _screen.Workspace, BuildStatus),
                    Color = _screen.Workspace.Select(workspace =>
                        workspace.Pending > 0 ? RimRewindSkin.Amber : RimRewindSkin.Muted
                    ),
                    Font = GameFont.Small,
                    TextAlign = TextAnchor.MiddleRight,
                    FlexGrow = 1f,
                    MarginHorizontal = 16f.Point(),
                    TextWrap = false,
                    TextOverflow = TextOverflowMode.Ellipsis,
                },
                new RimRewindButton
                {
                    Label = "Revert",
                    Width = 118f.Point(),
                    MarginRight = 8f.Point(),
                    Disabled = _screen.Workspace.Select(workspace => workspace.Pending == 0),
                    OnActivate = new Action(_screen.HandleRevert),
                },
                new RimRewindButton
                {
                    Label = "Apply",
                    Width = 140f.Point(),
                    Primary = true,
                    Disabled = _screen.Workspace.Select(workspace => workspace.Pending == 0),
                    OnActivate = new Action(_screen.HandleApply),
                },
            ];

        private static string BuildStatus(string status, RimRewindWorkspace workspace) =>
            status.Length > 0 ? status
            : workspace.Pending == 0 ? "No pending changes"
            : $"{workspace.Pending} settings changed";
    }
}
