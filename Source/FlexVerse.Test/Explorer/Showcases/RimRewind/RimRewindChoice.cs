// parents: @.frostline/graph/m-explorer.xml
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindChoice : Widget
    {
        [Prop]
        public Behavior<int> Value { get; set; } = 0;

        [Prop]
        public Behavior<string[]> Options { get; set; } = Array.Empty<string>();

        [Prop]
        public Behavior<bool> Disabled { get; set; } = false;

        [Prop]
        public Behavior<string> Tooltip { get; set; } = "";

        [Prop]
        public Behavior<Action<int>> OnValueChange { get; set; }

        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }
        private RimRewind _screen;

        public RimRewindChoice()
        {
            Height = 40f.Point();
            FlexDirection = FlexDirections.Column;
        }

        public override void OnMount() => _screen = Session.Value;

        public override IEnumerable<Widget> BuildRenderTree() =>
            [
                new RimRewindButton
                {
                    Label = Group.Combine(Value, Options, BuildLabel),
                    Selected = _screen.Popup.Select(popup => popup?.Anchor == this),
                    Disabled = Disabled,
                    Outlined = true,
                    Radius = RimRewindSkin.CornerRadius,
                    TextAlign = TextAnchor.MiddleLeft,
                    Tooltip = Tooltip,
                    OnActivate = new Action(HandleOpen),
                },
            ];

        private static string BuildLabel(int value, string[] options) =>
            options.Length == 0 ? "" : options[Mathf.Clamp(value, 0, options.Length - 1)] + "  ▾";

        private void HandleOpen()
        {
            if (!Disabled.Value && Options.Value.Length > 0)
                _screen.HandleOpenMenu(this, Options.Value, Value.Value, HandleSelection);
        }

        public void RestoreNativeFocus()
        {
            var children = Engine.GetChildren(this);
            for (var i = 0; i < children.Count; i++)
                if (children[i] is RimRewindButton button)
                    button.RequestNativeFocus();
        }

        private void HandleSelection(int index) => OnValueChange.Value?.Invoke(index);
    }
}
