// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    public partial class BentoRect : Widget
    {
        [Prop]
        public Behavior<Color> Ink { get; set; }

        public BentoRect()
        {
            MinWidth = 0f.Point();
            MinHeight = 0f.Point();
        }

        public override void OnGUI(Rect rect) => Widgets.DrawBoxSolid(rect, Ink.Value);
    }
}
