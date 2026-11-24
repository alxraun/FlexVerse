// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    /// <summary>
    /// Scenario question header: the one thing this scenario demonstrates,
    /// asked as a yes/no question the user can verify.
    /// </summary>
    public partial class SequenceHeader : Label
    {
        private static readonly Color QuestionColor = new(0.85f, 0.85f, 0.9f);

        public SequenceHeader()
        {
            Color = QuestionColor;
            Font = GameFont.Medium;
            TextAlign = TextAnchor.MiddleCenter;
            MarginBottom = 12f.Point();
        }
    }
}
