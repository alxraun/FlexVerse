// parents: @.frostline/graph/m-explorer.xml

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public sealed class TetrisPlan(
        TetrisKind kind,
        TetrisPose[] path,
        TetrisMove[] moves,
        int[] clearedRows,
        double cost
    )
    {
        public TetrisKind Kind { get; } = kind;
        public IReadOnlyList<TetrisPose> Path { get; } = path;
        public IReadOnlyList<TetrisMove> Moves { get; } = moves;
        public TetrisPose Landing => Path[Path.Count - 1];
        public int[] ClearedRows { get; } = clearedRows;
        public double Cost { get; } = cost;
    }

    /// <summary>Search legal moves, including slides and turns below overhangs; then inspect the next piece.</summary>
    public static class TetrisPlanner
    {
        private readonly struct Visit(TetrisPose parent, TetrisMove move)
        {
            public TetrisPose Parent { get; } = parent;
            public TetrisMove Move { get; } = move;
        }

        private static readonly TetrisMove[] Moves =
        [
            TetrisMove.Down,
            TetrisMove.Left,
            TetrisMove.Right,
            TetrisMove.Clockwise,
            TetrisMove.Counterclockwise,
        ];

        public static TetrisPlan? Choose(TetrisWell board, TetrisKind kind, TetrisKind next)
        {
            var candidates = Search(board, kind).OrderBy(plan => plan.Cost).Take(6).ToArray();
            TetrisPlan? best = null;
            var bestCost = double.PositiveInfinity;
            foreach (var candidate in candidates)
            {
                var projected = board.Clone();
                projected.Place(kind, candidate.Landing);
                projected.Clear(candidate.ClearedRows);
                var future = Search(projected, next)
                    .Select(plan => plan.Cost)
                    .DefaultIfEmpty(10000d)
                    .Min();
                var cost = candidate.Cost + future * 0.65d;
                if (cost >= bestCost)
                    continue;
                bestCost = cost;
                best = candidate;
            }
            return best;
        }

        private static IEnumerable<TetrisPlan> Search(TetrisWell board, TetrisKind kind)
        {
            var spawn = TetrisWell.Spawn;
            if (!board.Fits(kind, spawn))
                return [];
            var visited = new Dictionary<TetrisPose, Visit>();
            var queue = new Queue<TetrisPose>();
            var results = new List<TetrisPlan>();
            visited.Add(spawn, new Visit(spawn, TetrisMove.Down));
            queue.Enqueue(spawn);
            while (queue.Count > 0)
            {
                var pose = queue.Dequeue();
                if (
                    !board.TryMove(kind, pose, TetrisMove.Down, out _)
                    && Enumerable
                        .Range(0, 4)
                        .All(index => TetrisShapes.Cell(kind, pose, index).Y >= 0)
                )
                    results.Add(Evaluate(board, kind, pose, visited));
                foreach (var move in Moves)
                {
                    if (
                        !board.TryMove(kind, pose, move, out var target)
                        || visited.ContainsKey(target)
                    )
                        continue;
                    visited.Add(target, new Visit(pose, move));
                    queue.Enqueue(target);
                }
            }
            return results;
        }

        private static TetrisPlan Evaluate(
            TetrisWell board,
            TetrisKind kind,
            TetrisPose pose,
            Dictionary<TetrisPose, Visit> visited
        )
        {
            var projected = board.Clone();
            var rows = projected.Place(kind, pose);
            projected.Clear(rows);
            var path = new List<TetrisPose> { pose };
            var moves = new List<TetrisMove>();
            for (var cursor = pose; !cursor.Equals(TetrisWell.Spawn); )
            {
                var visit = visited[cursor];
                moves.Add(visit.Move);
                cursor = visit.Parent;
                path.Add(cursor);
            }
            path.Reverse();
            moves.Reverse();

            var cost = projected.Cost() - rows.Length * rows.Length * 10d + moves.Count * 0.002d;
            return new TetrisPlan(kind, path.ToArray(), moves.ToArray(), rows, cost);
        }
    }
}
