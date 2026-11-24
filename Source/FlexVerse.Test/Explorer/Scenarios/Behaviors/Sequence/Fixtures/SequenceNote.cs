// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Explanatory note: what to look at and what the correct observation
    /// is, in muted styling under the interactive elements.
    /// </summary>
    public partial class SequenceNote : Label
    {
        private static readonly Color MutedColor = new(0.6f, 0.65f, 0.6f);

        public SequenceNote()
        {
            Color = MutedColor;
            TextAlign = TextAnchor.MiddleLeft;
            MarginTop = 8f.Point();
        }
    }
}
