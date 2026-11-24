// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class ActualBox : View
    {
        [Prop]
        public Behavior<string> Label { get; set; } = "";

        public ActualBox()
        {
            FlexShrink = 0f;
            BackgroundColor = new Color(0.5f, 0.5f, 0.5f);
            BorderAll = 1f.Point();
            MarginAll = 10f.Point();
        }

        public override void OnGUI(Rect calculatedRect)
        {
            base.OnGUI(calculatedRect);

            if (!string.IsNullOrEmpty(Label.Value))
            {
                var originalAnchor = Text.Anchor;
                try
                {
                    Text.Anchor = TextAnchor.MiddleCenter;
                    Widgets.Label(calculatedRect, Label.Value);
                }
                finally
                {
                    Text.Anchor = originalAnchor;
                }
            }
        }
    }
}
