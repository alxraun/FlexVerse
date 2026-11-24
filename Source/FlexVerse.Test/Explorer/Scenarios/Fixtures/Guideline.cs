// parents: @.frostline/graph/m-explorer.xml

using FlexVerse;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Tests
{
    public partial class Guideline : Widget
    {
        public enum GuidelineMode
        {
            Horizontal,
            Vertical,
            TwoPoint,
        }

        [Prop]
        public Behavior<GuidelineMode> Mode { get; set; } = GuidelineMode.TwoPoint;

        [Prop]
        public Behavior<Color> Color { get; set; } = UnityEngine.Color.red;

        [Prop]
        public Behavior<float> Thickness { get; set; } = 1f;

        [Prop]
        public Behavior<float> Length { get; set; } = 100f;

        [Prop]
        public Behavior<Vector2> Start { get; set; } = Vector2.zero;

        [Prop]
        public Behavior<Vector2> End { get; set; } = new Vector2(100, 100);

        public Guideline()
        {
            PositionType = PositionTypes.Absolute;
        }

        protected override MeasureFunc MeasureFunc =>
            (node, width, widthMode, height, heightMode) =>
            {
                switch (Mode.Value)
                {
                    case GuidelineMode.Horizontal:
                        return new Size(Length.Value, Thickness.Value);
                    case GuidelineMode.Vertical:
                        return new Size(Thickness.Value, Length.Value);
                    case GuidelineMode.TwoPoint:
                        var measuredWidth = Mathf.Abs(End.Value.x - Start.Value.x);
                        var measuredHeight = Mathf.Abs(End.Value.y - Start.Value.y);
                        return new Size(measuredWidth, measuredHeight);
                    default:
                        return new Size(0, 0);
                }
            };

        public override void OnMount()
        {
            base.OnMount();
            if (Mode.Value == GuidelineMode.TwoPoint)
            {
                PositionTop = Mathf.Min(Start.Value.y, End.Value.y).Point();
                PositionLeft = Mathf.Min(Start.Value.x, End.Value.x).Point();
            }
        }

        public override void OnGUI(Rect calculatedRect)
        {
            var originalColor = GUI.color;
            GUI.color = Color.Value;

            switch (Mode.Value)
            {
                case GuidelineMode.Horizontal:
                {
                    var startPos = new Vector2(
                        calculatedRect.x,
                        calculatedRect.y + calculatedRect.height / 2f
                    );
                    var endPos = new Vector2(
                        calculatedRect.xMax,
                        calculatedRect.y + calculatedRect.height / 2f
                    );
                    Widgets.DrawLine(startPos, endPos, Color.Value, Thickness.Value);
                    break;
                }
                case GuidelineMode.Vertical:
                {
                    var startPos = new Vector2(
                        calculatedRect.x + calculatedRect.width / 2f,
                        calculatedRect.y
                    );
                    var endPos = new Vector2(
                        calculatedRect.x + calculatedRect.width / 2f,
                        calculatedRect.yMax
                    );
                    Widgets.DrawLine(startPos, endPos, Color.Value, Thickness.Value);
                    break;
                }
                case GuidelineMode.TwoPoint:
                {
                    var minX = Mathf.Min(Start.Value.x, End.Value.x);
                    var minY = Mathf.Min(Start.Value.y, End.Value.y);

                    var relativeStart = Start.Value - new Vector2(minX, minY);
                    var relativeEnd = End.Value - new Vector2(minX, minY);

                    Widgets.DrawLine(
                        relativeStart + calculatedRect.position,
                        relativeEnd + calculatedRect.position,
                        Color.Value,
                        Thickness.Value
                    );
                    break;
                }
            }

            GUI.color = originalColor;
        }
    }
}
