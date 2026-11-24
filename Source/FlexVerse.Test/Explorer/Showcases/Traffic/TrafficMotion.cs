// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>Stable style outputs observe replaceable tween slots through Watch.</summary>
    public partial class TrafficMotion
    {
        [FVP]
        private Behavior<Vector2> Origin { get; set; }

        [FVP]
        public Behavior<float> Activity { get; private set; } = 0f;
        private Vector2 _plannedCell;

        [FVP]
        public Behavior<Length> Left { get; private set; }

        [FVP]
        public Behavior<Length> Top { get; private set; }

        [FVP]
        public Behavior<Length> Width { get; private set; }

        [FVP]
        public Behavior<Length> Height { get; private set; }

        public void Initialize(int column, int row)
        {
            var frame = TrafficLayouts.Frame(column, row);
            _plannedCell = new Vector2(column, row);
            Origin = frame.position;
            Activity = Spring.State(0f, tension: 70f, friction: 17f);
            Left = (frame.x * 100f).Percent();
            Top = (frame.y * 100f).Percent();
            Width = (frame.width * 100f).Percent();
            Height = (frame.height * 100f).Percent();
            Watch.Origin.OnUpdate += _ => ProjectOrigin();
        }

        private void ProjectOrigin()
        {
            Left.Target = (Origin.Value.x * 100f).Percent();
            Top.Target = (Origin.Value.y * 100f).Percent();
        }

        public void Signal(float intensity) => Activity.Target = intensity;

        public ITimelineAction Drive(params (int Column, int Row)[] route)
        {
            var actions = new System.Collections.Generic.List<ITimelineAction>
            {
                Timeline.Call(() => Signal(1f)),
            };
            foreach (var point in route)
            {
                var destination = new Vector2(point.Column, point.Row);
                var distance =
                    Mathf.Abs(destination.x - _plannedCell.x)
                    + Mathf.Abs(destination.y - _plannedCell.y);
                if (distance < 0.01f)
                    continue;
                var frame = TrafficLayouts.Frame(point.Column, point.Row);
                actions.Add(
                    Timeline.Set(
                        Refs.Origin,
                        Animate.To(frame.position, 0.18f + distance * 0.2f, TrafficLayouts.Ease)
                    )
                );
                _plannedCell = destination;
            }
            actions.Add(Timeline.Call(() => Signal(0f)));
            return Timeline.Sequence(actions.ToArray());
        }
    }
}
