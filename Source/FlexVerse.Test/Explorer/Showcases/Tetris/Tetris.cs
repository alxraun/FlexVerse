// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;

namespace FlexVerse.Test.Explorer.Showcases
{
    [Screen("Showcases/Tetris")]
    public partial class Tetris : View
    {
        [State(ReconcileOnSet = true)]
        public Behavior<TetrisMinoMotion[]> Cells { get; set; } = Array.Empty<TetrisMinoMotion>();

        [State(ReconcileOnSet = true)]
        public Behavior<TetrisKind[]> Preview { get; set; } = Array.Empty<TetrisKind>();

        [State(ReconcileOnSet = true)]
        public Behavior<int> Lines { get; set; } = 0;

        [State(ReconcileOnSet = true)]
        public Behavior<int> Combo { get; set; } = 0;

        [State(ReconcileOnSet = true)]
        public Behavior<string> MoveName { get; set; } = "";

        [State]
        public Behavior<Texture2D> Glow { get; set; }

        [State]
        public Behavior<bool> TurnFinished { get; set; } = false;

        [State]
        public Behavior<float> PreviewTravel { get; set; } = 0f;

        [State]
        public Behavior<float> PreviewOffset { get; set; } = 0f;

        [State]
        public Behavior Playback { get; set; }

        [State]
        public Behavior Turn { get; set; }

        [State]
        public Behavior QueueMotion { get; set; }
        private TetrisWell _well;
        private readonly Queue<TetrisKind> _upcoming = new();
        private readonly List<TetrisMinoMotion> _retired = new();
        private readonly System.Random _random = new(7349);
        private TetrisMinoMotion[] _ghosts;
        private TetrisMinoMotion[] _active;
        private int _nextId;

        public Tetris()
        {
            BackgroundColor = TetrisStyle.Background;
            FlexGrow = 1f;
            Width = 100f.Percent();
            Height = 100f.Percent();
            FlexDirection = FlexDirections.Row;
            AlignItems = Align.Center;
            JustifyContent = Justify.Center;
            PaddingAll = 24f.Point();
        }

        public override void OnMount()
        {
            Glow = TetrisStyle.CreateGlow();
            PreviewOffset = Spring.State(0f, tension: 70f, friction: 17f);
            Watch.PreviewTravel.OnUpdate += _ => PreviewOffset.Target = PreviewTravel.Value;
            _ghosts = Enumerable
                .Range(0, 4)
                .Select(index => NewCell(TetrisKind.I, new TetrisPoint(index + 3, 0), ghost: true))
                .ToArray();
            SeedBoard();
            Playback = Timeline.Create(
                Timeline.While(
                    () => true,
                    Timeline.Sequence(
                        Timeline.Call(BeginTurn),
                        Timeline.WaitUntil(() => TurnFinished.Value),
                        Timeline.Wait(0.2f)
                    )
                )
            );
        }

        private TetrisMinoMotion NewCell(
            TetrisKind kind,
            TetrisPoint cell,
            bool ghost = false,
            bool active = false
        )
        {
            var motion = new TetrisMinoMotion();
            motion.Initialize(_nextId++, kind, cell, ghost, active);
            return motion;
        }

        private void SeedBoard()
        {
            _well = TetrisWell.Opening();
            var cells = new List<TetrisMinoMotion>();
            for (var y = 0; y < TetrisWell.Rows; y++)
            for (var x = 0; x < TetrisWell.Columns; x++)
                if (_well[x, y] != 0)
                    cells.Add(NewCell((TetrisKind)(_well[x, y] - 1), new TetrisPoint(x, y)));
            Cells = cells.ToArray();
            _upcoming.Clear();
            foreach (var kind in new[] { TetrisKind.I, TetrisKind.T, TetrisKind.I })
                _upcoming.Enqueue(kind);
            FillQueue();
        }

        private void FillQueue()
        {
            while (_upcoming.Count < 4)
            {
                var bag = (TetrisKind[])Enum.GetValues(typeof(TetrisKind));
                for (var i = bag.Length - 1; i > 0; i--)
                {
                    var j = _random.Next(i + 1);
                    (bag[i], bag[j]) = (bag[j], bag[i]);
                }
                foreach (var kind in bag)
                    _upcoming.Enqueue(kind);
            }
        }

        private void BeginTurn()
        {
            ReleaseRetired();
            TurnFinished.Target = false;
            FillQueue();
            var kind = _upcoming.Dequeue();
            var plan = TetrisPlanner.Choose(_well, kind, _upcoming.Peek());
            if (plan == null)
            {
                MoveName = "NEW STACK";
                Turn = Timeline.Create(
                    Timeline.Sequence(
                        Timeline.Parallel(Cells.Value.Select(cell => cell.Fade()).ToArray()),
                        Timeline.Call(RetireBoard),
                        Timeline.Wait(0.25f),
                        Timeline.Call(SeedBoard),
                        Timeline.Call(() => TurnFinished.Target = true)
                    )
                );
                return;
            }
            _active = Enumerable
                .Range(0, 4)
                .Select(index =>
                    NewCell(kind, TetrisShapes.Cell(kind, TetrisWell.Spawn, index), active: true)
                )
                .ToArray();
            for (var i = 0; i < _active.Length; i++)
                _active[i].AssignCell(TetrisShapes.Cell(kind, plan.Landing, i));
            Cells = Cells.Value.Concat(_active).ToArray();
            Preview = _upcoming.Take(3).ToArray();
            QueueMotion = Timeline.Create(
                Timeline.Set(
                    Refs.PreviewTravel,
                    Animate.FromTo(52f, 0f, 0.45f, Easing.CubicEaseOut)
                )
            );
            MoveName = Describe(plan);
            foreach (var ghost in _ghosts)
                ghost.ShowGhost();
            AimGhost(kind, TetrisWell.Spawn);
            Turn = Timeline.Create(
                Timeline.Sequence(
                    Timeline.Wait(0.25f),
                    BuildFlight(plan),
                    Timeline.Parallel(_active.Select(cell => cell.Settle(cell.Cell)).ToArray()),
                    Timeline.Call(() => _well.Place(plan.Kind, plan.Landing)),
                    Timeline.Parallel(
                        _active
                            .Select(cell => cell.Lock())
                            .Concat(_ghosts.Select(ghost => ghost.Fade()))
                            .ToArray()
                    ),
                    BuildClear(plan.ClearedRows),
                    Timeline.Call(() => TurnFinished.Target = true)
                )
            );
        }

        private ITimelineAction BuildFlight(TetrisPlan plan)
        {
            var actions = new List<ITimelineAction>();
            for (var step = 1; step < plan.Path.Count; step++)
            {
                var start = plan.Path[step - 1];
                var move = plan.Moves[step - 1];
                if (move == TetrisMove.Down)
                    while (step < plan.Moves.Count && plan.Moves[step] == TetrisMove.Down)
                        step++;
                var target = plan.Path[step];
                var rotation = target.Rotation != start.Rotation;
                var duration =
                    rotation ? 0.26f
                    : move == TetrisMove.Down ? 0.13f + (target.Y - start.Y) * 0.027f
                    : 0.14f;
                EasingFunction easing =
                    move == TetrisMove.Down ? Easing.QuadEaseIn : Easing.SineEaseInOut;
                actions.Add(Timeline.Call(() => AimGhost(plan.Kind, target)));
                actions.Add(
                    Timeline.Parallel(
                        _active
                            .Select(
                                (cell, index) =>
                                    rotation
                                        ? cell.Rotate(
                                            plan.Kind,
                                            start,
                                            target,
                                            index,
                                            duration,
                                            move == TetrisMove.Clockwise
                                        )
                                        : cell.Move(
                                            TetrisShapes.Cell(plan.Kind, target, index),
                                            duration,
                                            easing
                                        )
                            )
                            .ToArray()
                    )
                );
                if (rotation)
                    actions.Add(Timeline.Wait(0.075f));
            }
            return Timeline.Sequence(actions.ToArray());
        }

        private void AimGhost(TetrisKind kind, TetrisPose pose)
        {
            var landing = _well.Landing(kind, pose);
            for (var i = 0; i < _ghosts.Length; i++)
                _ghosts[i].Aim(TetrisShapes.Cell(kind, landing, i), kind);
        }

        private ITimelineAction BuildClear(int[] rows)
        {
            if (rows.Length == 0)
                return Timeline.Call(() => Combo = 0);
            var erased = Cells.Value.Where(cell => Array.IndexOf(rows, cell.Cell.Y) >= 0).ToArray();
            var falling = Cells
                .Value.Except(erased)
                .Select(cell => (Cell: cell, Distance: rows.Count(row => row > cell.Cell.Y)))
                .Where(item => item.Distance > 0)
                .ToArray();
            return Timeline.Sequence(
                Timeline.Wait(0.12f),
                Timeline.Parallel(
                    erased
                        .Select(cell =>
                            cell.Erase(
                                cell.Cell.X * 0.018f + Array.IndexOf(rows, cell.Cell.Y) * 0.06f
                            )
                        )
                        .ToArray()
                ),
                Timeline.Call(() => CommitClear(rows, erased)),
                Timeline.Parallel(
                    falling
                        .Select(item =>
                            Timeline.Sequence(
                                Timeline.Wait((TetrisWell.Rows - item.Cell.Cell.Y) * 0.009f),
                                item.Cell.Move(
                                    new TetrisPoint(
                                        item.Cell.Cell.X,
                                        item.Cell.Cell.Y + item.Distance
                                    ),
                                    0.18f + item.Distance * 0.07f,
                                    Easing.QuartEaseIn
                                )
                            )
                        )
                        .ToArray()
                ),
                Timeline.Wait(0.18f)
            );
        }

        private void CommitClear(int[] rows, TetrisMinoMotion[] erased)
        {
            _well.Clear(rows);
            _retired.AddRange(erased);
            var survivors = Cells.Value.Except(erased).ToArray();
            foreach (var cell in survivors)
                cell.AssignCell(
                    new TetrisPoint(cell.Cell.X, cell.Cell.Y + rows.Count(row => row > cell.Cell.Y))
                );
            Cells = survivors;
            Lines = Lines.Value + rows.Length;
            Combo = Combo.Value + 1;
        }

        private static string Describe(TetrisPlan plan)
        {
            if (plan.ClearedRows.Length == 4)
                return "TETRIS";
            var lowTurn = Enumerable
                .Range(0, plan.Moves.Count)
                .Any(index =>
                    plan.Path[index].Y >= TetrisWell.Rows - 5
                    && plan.Moves[index] is TetrisMove.Clockwise or TetrisMove.Counterclockwise
                );
            if (lowTurn)
                return plan.Kind == TetrisKind.T ? "TUCK TURN" : "WALL TURN";
            return plan.ClearedRows.Length switch
            {
                3 => "TRIPLE",
                2 => "DOUBLE",
                1 => "LINE CLEAR",
                _ => "BUILD",
            };
        }

        private void RetireBoard()
        {
            _retired.AddRange(Cells.Value);
            Cells = Array.Empty<TetrisMinoMotion>();
        }

        private void ReleaseRetired()
        {
            foreach (var cell in _retired)
                cell.ReleaseFvp();
            _retired.Clear();
        }

        public override void OnUnmount()
        {
            ReleaseRetired();
            foreach (var cell in Cells.Value)
                cell.ReleaseFvp();
            if (_ghosts != null)
                foreach (var ghost in _ghosts)
                    ghost.ReleaseFvp();
            if (Glow.Value != null)
                UnityEngine.Object.Destroy(Glow.Value);
        }

        public override IEnumerable<Widget> BuildRenderTree() =>
            [BuildQueue(), BuildBoard(), BuildReadout()];

        private Widget BuildBoard() =>
            new View
            {
                Height = 100f.Percent(),
                MaxWidth = 65f.Percent(),
                AspectRatio = (float)TetrisWell.Columns / TetrisWell.Rows,
                PaddingAll = 8f.Point(),
                BorderAll = 2f.Point(),
                BorderColor = TetrisStyle.Frame,
                BackgroundColor = TetrisStyle.Surface,
                Children = new WidgetList
                {
                    new TetrisCanvas
                    {
                        Width = 100f.Percent(),
                        FlexGrow = 1f,
                        Children = BuildCells(),
                    },
                },
            };

        private WidgetList BuildCells() =>
            new(
                (_ghosts ?? Array.Empty<TetrisMinoMotion>())
                    .Concat(Cells.Value)
                    .Select(cell => new TetrisMino
                    {
                        Key = cell.Id,
                        Motion = cell,
                        Glow = Glow,
                        PositionType = PositionTypes.Absolute,
                        PositionLeft = cell.Left,
                        PositionTop = cell.Top,
                        Width = (100f / TetrisWell.Columns).Percent(),
                        Height = (100f / TetrisWell.Rows).Percent(),
                    })
            );

        private Widget BuildQueue() =>
            new View
            {
                Width = 140f.Point(),
                MarginRight = 28f.Point(),
                Children = new WidgetList
                {
                    Caption("NEXT"),
                    new View
                    {
                        PositionTop = PreviewOffset.Select(value => value.Point()),
                        Children = new WidgetList(
                            Preview.Value.Select(
                                (kind, index) =>
                                    new TetrisPreview
                                    {
                                        Key = (index, kind),
                                        Kind = kind,
                                        Height = 76f.Point(),
                                        Width = 100f.Percent(),
                                    }
                            )
                        ),
                    },
                },
            };

        private Widget BuildReadout() =>
            new View
            {
                Width = 140f.Point(),
                MarginLeft = 28f.Point(),
                Children = new WidgetList
                {
                    Caption("TETRIS"),
                    new View { Height = 28f.Point() },
                    Caption("LINES"),
                    new Label
                    {
                        Content = Lines.Value.ToString("D3"),
                        Font = GameFont.Medium,
                        Height = 38f.Point(),
                    },
                    new View { Height = 22f.Point() },
                    Caption(MoveName.Value),
                    Caption(Combo.Value > 1 ? $"CHAIN {Combo.Value}" : ""),
                },
            };

        private static Widget Caption(string text) =>
            new Label
            {
                Content = text,
                Font = GameFont.Tiny,
                TextWrap = false,
                Color = new Color(0.6f, 0.67f, 0.76f),
                Height = 22f.Point(),
            };
    }
}
