// parents: @.frostline/graph/m-explorer.xml

using System;
using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    public sealed class RimRewindPopup
    {
        public int Identity { get; }
        public RimRewindChoice Anchor { get; }
        public string[] Options { get; }
        public int Selected { get; }
        public Action<int> Choose { get; }
        public Rect AnchorBounds { get; }
        public Rect HostBounds { get; }

        public RimRewindPopup(
            int identity,
            RimRewindChoice anchor,
            string[] options,
            int selected,
            Action<int> choose,
            Rect anchorBounds,
            Rect hostBounds
        )
        {
            Identity = identity;
            Anchor = anchor;
            Options = options;
            Selected = selected;
            Choose = choose;
            AnchorBounds = anchorBounds;
            HostBounds = hostBounds;
        }
    }
}
