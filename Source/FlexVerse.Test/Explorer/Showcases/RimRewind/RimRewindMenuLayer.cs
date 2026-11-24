// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class RimRewindMenuLayer : View
    {
        [InheritedContext]
        public Behavior<RimRewind> Session { get; set; }

        [State]
        private Behavior<RimRewindPopup> Popup { get; set; }

        [State]
        private Behavior<Vector2> HostSize { get; set; } = Vector2.zero;
        private RimRewind _screen;

        public RimRewindMenuLayer()
        {
            PositionType = PositionTypes.Absolute;
            PositionLeft = 0f.Point();
            PositionTop = 0f.Point();
            Width = 100f.Percent();
            Height = 100f.Percent();
            ZIndex = 1000;
            BackgroundColor = new Color(0f, 0f, 0f, .12f);
        }

        public override void OnMount()
        {
            _screen = Session.Value;
            Popup = _screen.Popup;
            Watch.HostSize.OnUpdate += _ => _screen.HandleMenuLayout();
        }

        public override IEnumerable<Widget> BuildRenderTree()
        {
            var popup = Popup.Value;
            if (popup == null)
                return [];
            var root = popup.HostBounds;
            var anchor = popup.AnchorBounds;
            var width = Mathf.Min(Mathf.Max(280f, anchor.width), Mathf.Max(1f, root.width - 24f));
            var height = Mathf.Min(
                popup.Options.Length * 40f + 16f,
                Mathf.Max(1f, root.height - 24f)
            );
            var left = Mathf.Clamp(
                anchor.x - root.x,
                12f,
                Mathf.Max(12f, root.width - width - 12f)
            );
            var below = anchor.yMax - root.y + 5f;
            var top = below + height <= root.height - 12f ? below : anchor.y - root.y - height - 5f;
            return
            [
                new RimRewindMenuPanel
                {
                    Key = popup.Identity,
                    Popup = popup,
                    PositionType = PositionTypes.Absolute,
                    PositionLeft = left.Point(),
                    PositionTop = Mathf
                        .Clamp(top, 12f, Mathf.Max(12f, root.height - height - 12f))
                        .Point(),
                    Width = width.Point(),
                    Height = height.Point(),
                },
            ];
        }

        public override void OnGUI(Rect rect)
        {
            base.OnGUI(rect);
            if (HostSize.Value != rect.size)
                HostSize.Target = rect.size;
        }

        public override void OnMouseDown(FlexEvent e)
        {
            if (!ReferenceEquals(e.Target, this))
                return;
            _screen.HandleCloseMenu();
            e.StopPropagation();
        }

        public override void OnScrollWheel(FlexEvent e)
        {
            if (ReferenceEquals(e.Target, this))
                _screen.HandleCloseMenu();
            e.StopPropagation();
        }
    }
}
