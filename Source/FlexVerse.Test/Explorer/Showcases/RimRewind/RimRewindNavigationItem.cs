// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindNavigationItem : View
    {
        [Prop]
        public Behavior<int> Index { get; set; }

        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindNavigationItem()
        {
            Height = 44f.Point();
            MarginBottom = 4f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButton
                {
                    Label = _screen.Workspace.Select(BuildLabel),
                    Quiet = true,
                    Radius = RimRewindSkin.CornerRadius,
                    Height = 44f.Point(),
                    TextAlign = TextAnchor.MiddleLeft,
                    Selected = Group.Combine(
                        _screen.Category,
                        _screen.Query,
                        (category, query) =>
                            category == Index.Value && string.IsNullOrWhiteSpace(query)
                    ),
                    OnActivate = new Action(HandleSelection),
                },
            ];

        private string BuildLabel(RimRewindWorkspace workspace)
        {
            var pending = RimRewindOptions.All.Count(option =>
                option.Category == Index.Value
                && workspace.Draft.Differs(workspace.Applied, option.Id)
            );
            return $"{Index.Value + 1:00}   {RimRewindOptions.Categories[Index.Value]}"
                + (pending > 0 ? $"  • {pending}" : "");
        }

        private void HandleSelection() => _screen.HandleCategory(Index.Value);
    }
}
