// parents: @.frostline/graph/m-explorer.xml

using UnityEngine;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>One persistent cell identity, shared by falling, locked and collapsing states.</summary>
    public partial class TetrisMinoMotion
    {
        [FVP]
        public Behavior<Vector2> Position { get; private set; }

        [FVP]
        public Behavior<Length> Left { get; private set; }

        [FVP]
        public Behavior<Length> Top { get; private set; }

        [FVP]
        public Behavior<float> Opacity { get; private set; } = 1f;

        [FVP]
        public Behavior<float> Flash { get; private set; } = 0f;

        [FVP]
        public Behavior<float> Emphasis { get; private set; } = 0f;
        public int Id { get; private set; }
        public TetrisKind Kind { get; private set; }
        public TetrisPoint Cell { get; private set; }
        public bool IsGhost { get; private set; }

        public void Initialize(
            int id,
            TetrisKind kind,
            TetrisPoint cell,
            bool ghost = false,
            bool active = false
        )
        {
            Id = id;
            Kind = kind;
            Cell = cell;
            IsGhost = ghost;
            Position = new Vector2(cell.X, cell.Y);
            Left = (cell.X * 100f / TetrisWell.Columns).Percent();
            Top = (cell.Y * 100f / TetrisWell.Rows).Percent();
            Opacity = ghost ? 0.22f : 1f;
            Emphasis = active ? 1f : 0f;

            Watch.Position.OnUpdate += _ => ProjectPosition();
        }

        private void ProjectPosition()
        {
            Left.Target = (Position.Value.x * 100f / TetrisWell.Columns).Percent();
            Top.Target = (Position.Value.y * 100f / TetrisWell.Rows).Percent();
        }

        public void Aim(TetrisPoint cell, TetrisKind kind)
        {
            Kind = kind;
            Position = Spring.To(new Vector2(cell.X, cell.Y), tension: 60f, friction: 16f);
        }

        public void ShowGhost() => Opacity = 0.22f;

        public void AssignCell(TetrisPoint cell) => Cell = cell;

        public ITimelineAction Move(TetrisPoint cell, float duration, EasingFunction easing) =>
            Timeline.Set(Refs.Position, Animate.To(new Vector2(cell.X, cell.Y), duration, easing));

        public ITimelineAction Rotate(
            TetrisKind kind,
            TetrisPose from,
            TetrisPose to,
            int index,
            float duration,
            bool clockwise
        )
        {
            var pivot =
                kind == TetrisKind.I ? 1.5f
                : kind == TetrisKind.O ? 0.5f
                : 1f;
            var startCenter = new Vector2(from.X + pivot, from.Y + pivot);
            var endCenter = new Vector2(to.X + pivot, to.Y + pivot);
            var cell = TetrisShapes.Cell(kind, from, index);
            var arm = new Vector2(cell.X, cell.Y) - startCenter;
            var frames = new Keyframe<Vector2>[5];
            for (var step = 0; step < frames.Length; step++)
            {
                var progress = step / 4f;
                var angle = (clockwise ? 1f : -1f) * Mathf.PI * 0.5f * progress;
                var rotated = new Vector2(
                    arm.x * Mathf.Cos(angle) - arm.y * Mathf.Sin(angle),
                    arm.x * Mathf.Sin(angle) + arm.y * Mathf.Cos(angle)
                );
                frames[step] = new Keyframe<Vector2>
                {
                    Time = progress * duration,
                    Value = Vector2.Lerp(startCenter, endCenter, progress) + rotated,
                };
            }
            var destination = TetrisShapes.Cell(kind, to, index);
            frames[4] = new Keyframe<Vector2>
            {
                Time = duration,
                Value = new Vector2(destination.X, destination.Y),
            };
            return Timeline.Set(Refs.Position, Animate.Keyframes(duration, frames));
        }

        public ITimelineAction Settle(TetrisPoint cell) =>
            Timeline.Set(
                Refs.Position,
                Spring.To(new Vector2(cell.X, cell.Y), tension: 85f, friction: 18f)
            );

        public ITimelineAction Lock() =>
            Timeline.Parallel(
                Timeline.Set(Refs.Emphasis, Animate.To(0f, 0.32f, Easing.SineEaseInOut)),
                Timeline.Set(
                    Refs.Flash,
                    Animate.Keyframes(
                        0.32f,
                        new Keyframe<float> { Time = 0f, Value = 0f },
                        new Keyframe<float> { Time = 0.08f, Value = 0.7f },
                        new Keyframe<float> { Time = 0.32f, Value = 0f }
                    )
                )
            );

        public ITimelineAction Erase(float delay) =>
            Timeline.Sequence(
                Timeline.Wait(delay),
                Timeline.Set(
                    Refs.Flash,
                    Animate.Keyframes(
                        0.36f,
                        new Keyframe<float> { Time = 0f, Value = 0f },
                        new Keyframe<float> { Time = 0.1f, Value = 1f },
                        new Keyframe<float> { Time = 0.22f, Value = 0.2f },
                        new Keyframe<float> { Time = 0.36f, Value = 1f }
                    )
                ),
                Timeline.Set(Refs.Opacity, Animate.To(0f, 0.22f, Easing.CubicEaseIn))
            );

        public ITimelineAction Fade() => Timeline.Set(Refs.Opacity, Animate.To(0f, 0.4f));
    }
}
