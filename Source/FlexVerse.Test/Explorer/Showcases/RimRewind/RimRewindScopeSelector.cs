// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindScopeSelector : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindScopeSelector()
        {
            Width = 286f.Point();
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                RimRewindSkin.BuildCaption("Settings Scope"),
                new View
                {
                    FlexDirection = FlexDirections.Row,
                    Children = new WidgetList
                    {
                        BuildScope(0, "This colony"),
                        BuildScope(1, "New colonies"),
                    },
                },
            ];

        private Widget BuildScope(int scope, string label) =>
            new RimRewindButton
            {
                Label = label,
                Compact = true,
                FlexGrow = 1f,
                FlexBasis = 0f.Point(),
                MarginRight = 4f.Point(),
                Selected = _screen.Workspace.Select(workspace => workspace.Scope == scope),
                OnActivate = new Action(() => _screen.HandleScope(scope)),
            };
    }
}
