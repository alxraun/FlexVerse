// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class GridView : View
    {
        [Prop]
        public Behavior<bool> ShowGrid { get; set; } = true;

        [Prop]
        public Behavior<bool> ShowAxes { get; set; } = true;

        public GridView()
        {
            BackgroundColor = new Color(0.2f, 0.2f, 0.25f);
        }

        public override void OnGUI(Rect calculatedRect)
        {
            base.OnGUI(calculatedRect);

            if (ShowGrid.Value)
            {
                DrawGrid(calculatedRect);
            }

            if (ShowAxes.Value)
            {
                DrawAxes(calculatedRect);
            }
        }

        private void DrawGrid(Rect rect)
        {
            const float GridStep = 20f;
            var gridColor = new Color(1f, 1f, 1f, 0.1f);
            var gridColorThick = new Color(1f, 1f, 1f, 0.15f);

            var originalColor = GUI.color;
            GUI.color = gridColor;

            for (float x = rect.x; x < rect.xMax; x += GridStep)
            {
                if (Mathf.RoundToInt(x / GridStep) % 5 == 0)
                {
                    GUI.color = gridColorThick;
                    Widgets.DrawLineVertical(x, rect.y, rect.height);
                    GUI.color = gridColor;
                }
                else
                {
                    Widgets.DrawLineVertical(x, rect.y, rect.height);
                }
            }

            for (float y = rect.y; y < rect.yMax; y += GridStep)
            {
                if (Mathf.RoundToInt(y / GridStep) % 5 == 0)
                {
                    GUI.color = gridColorThick;
                    Widgets.DrawLineHorizontal(rect.x, y, rect.width);
                    GUI.color = gridColor;
                }
                else
                {
                    Widgets.DrawLineHorizontal(rect.x, y, rect.width);
                }
            }

            GUI.color = originalColor;
        }

        private void DrawAxes(Rect rect)
        {
            float centerX = rect.x + rect.width / 2f;
            float centerY = rect.y + rect.height / 2f;
            var axesColor = new Color(1f, 1f, 1f, 0.3f);

            var originalColor = GUI.color;
            GUI.color = axesColor;

            Widgets.DrawLineHorizontal(rect.x, centerY, rect.width);
            Widgets.DrawLineVertical(centerX, rect.y, rect.height);

            GUI.color = originalColor;
        }
    }
}
