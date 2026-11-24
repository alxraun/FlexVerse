// parents: @.frostline/graph/m-appearance-drawers.xml

using UnityEngine;
using Verse;

namespace FlexVerse
{
    internal static class BackgroundDrawer
    {
        internal static void Draw(
            Rect bounds,
            Color background,
            Texture backgroundImage,
            BackgroundSize backgroundSize,
            Vector2 backgroundPosition,
            Color backgroundTint,
            CornerRadii radii
        )
        {
            if (bounds.width <= 0f || bounds.height <= 0f)
                return;

            if (background.a > 0f)
            {
                if (radii.IsSquare)
                    Widgets.DrawBoxSolid(bounds, background);
                else
                    ImageDrawer.DrawTint(
                        bounds,
                        AppearanceMasks.GetFill(bounds, radii),
                        background
                    );
            }

            if (backgroundImage != null)
                ImageDrawer.Draw(
                    bounds,
                    backgroundImage,
                    backgroundSize,
                    backgroundPosition,
                    backgroundTint,
                    radii
                );
        }
    }
}
