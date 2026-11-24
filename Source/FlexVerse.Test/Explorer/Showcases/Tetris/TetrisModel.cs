// parents: @.frostline/graph/m-explorer.xml

#nullable enable

using System;
using System.Collections.Generic;
using System.Linq;

namespace FlexVerse.Test.Explorer.Showcases
{
    public enum TetrisKind
    {
        I,
        T,
        L,
        J,
        S,
        Z,
        O,
    }

    public enum TetrisMove
    {
        Left,
        Right,
        Down,
        Clockwise,
        Counterclockwise,
    }

    public readonly struct TetrisPoint(int x, int y)
    {
        public int X { get; } = x;
        public int Y { get; } = y;
    }

    public readonly struct TetrisPose(int x, int y, int rotation) : IEquatable<TetrisPose>
    {
        public int X { get; } = x;
        public int Y { get; } = y;
        public int Rotation { get; } = rotation;

        public bool Equals(TetrisPose other) =>
            X == other.X && Y == other.Y && Rotation == other.Rotation;

        public override bool Equals(object? obj) => obj is TetrisPose other && Equals(other);

        public override int GetHashCode() => ((X + 4) * 31 + Y + 4) * 4 + Rotation;
    }

    public static class TetrisShapes
    {
        private static readonly TetrisPoint[][][] Shapes =
        [
            Rotate(4, new(0, 1), new(1, 1), new(2, 1), new(3, 1)),
            Rotate(3, new(1, 0), new(0, 1), new(1, 1), new(2, 1)),
            Rotate(3, new(2, 0), new(0, 1), new(1, 1), new(2, 1)),
            Rotate(3, new(0, 0), new(0, 1), new(1, 1), new(2, 1)),
            Rotate(3, new(1, 0), new(2, 0), new(0, 1), new(1, 1)),
            Rotate(3, new(0, 0), new(1, 0), new(1, 1), new(2, 1)),
            Rotate(2, new(0, 0), new(1, 0), new(0, 1), new(1, 1)),
        ];

        public static IReadOnlyList<TetrisPoint> Cells(TetrisKind kind, int rotation) =>
            Shapes[(int)kind][rotation];

        public static TetrisPoint Cell(TetrisKind kind, TetrisPose pose, int index)
        {
            var local = Shapes[(int)kind][pose.Rotation][index];
            return new TetrisPoint(pose.X + local.X, pose.Y + local.Y);
        }

        private static TetrisPoint[][] Rotate(int size, params TetrisPoint[] initial)
        {
            var rotations = new TetrisPoint[4][];
            rotations[0] = initial;
            for (var rotation = 1; rotation < 4; rotation++)
                rotations[rotation] = rotations[rotation - 1]
                    .Select(point => new TetrisPoint(size - 1 - point.Y, point.X))
                    .ToArray();
            return rotations;
        }
    }

    /// <summary>Pure board rules. Animation never substitutes for collision or line-clear rules.</summary>
    public sealed class TetrisWell
    {
        public const int Columns = 10;
        public const int Rows = 18;
        private readonly int[,] _grid;

        public TetrisWell() => _grid = new int[Columns, Rows];

        private TetrisWell(int[,] grid) => _grid = grid;

        public int this[int x, int y] => _grid[x, y];

        public TetrisWell Clone() => new((int[,])_grid.Clone());

        public static TetrisPose Spawn => new(3, -2, 0);

        public static TetrisWell Opening()
        {
            var board = new TetrisWell();
            for (var y = 14; y < Rows; y++)
            for (var x = 0; x < 9; x++)
                board._grid[x, y] = 1 + (x + y) % 7;
            for (var x = 0; x < 9; x++)
            {
                if (x != 4)
                    board._grid[x, 13] = 1 + x % 7;
                if (x < 3 || x > 5)
                    board._grid[x, 12] = 1 + (x + 2) % 7;
            }
            board._grid[5, 11] = 2;
            return board;
        }

        public bool Fits(TetrisKind kind, TetrisPose pose)
        {
            for (var i = 0; i < 4; i++)
            {
                var point = TetrisShapes.Cell(kind, pose, i);
                if (point.X < 0 || point.X >= Columns || point.Y < -4 || point.Y >= Rows)
                    return false;
                if (point.Y >= 0 && _grid[point.X, point.Y] != 0)
                    return false;
            }
            return true;
        }

        public bool TryMove(
            TetrisKind kind,
            TetrisPose pose,
            TetrisMove move,
            out TetrisPose result
        )
        {
            var rotation = (pose.Rotation + (move == TetrisMove.Clockwise ? 1 : 3)) % 4;
            result = move switch
            {
                TetrisMove.Left => new(pose.X - 1, pose.Y, pose.Rotation),
                TetrisMove.Right => new(pose.X + 1, pose.Y, pose.Rotation),
                TetrisMove.Down => new(pose.X, pose.Y + 1, pose.Rotation),
                _ => new(pose.X, pose.Y, rotation),
            };
            if (Fits(kind, result))
                return true;
            if (move is TetrisMove.Left or TetrisMove.Right or TetrisMove.Down)
                return false;

            foreach (var kick in Kicks)
            {
                var candidate = new TetrisPose(pose.X + kick.X, pose.Y + kick.Y, rotation);
                if (!Fits(kind, candidate))
                    continue;
                result = candidate;
                return true;
            }
            return false;
        }

        private static readonly TetrisPoint[] Kicks =
        [
            new(-1, 0),
            new(1, 0),
            new(-2, 0),
            new(2, 0),
            new(0, -1),
            new(-1, -1),
            new(1, -1),
            new(0, -2),
        ];

        public TetrisPose Landing(TetrisKind kind, TetrisPose pose)
        {
            while (TryMove(kind, pose, TetrisMove.Down, out var next))
                pose = next;
            return pose;
        }

        public int[] Place(TetrisKind kind, TetrisPose pose)
        {
            if (!Fits(kind, pose))
                throw new InvalidOperationException("Tetris placement collides with the stack.");
            for (var i = 0; i < 4; i++)
            {
                var point = TetrisShapes.Cell(kind, pose, i);
                if (point.Y < 0)
                    throw new InvalidOperationException("Tetris placement is above the board.");
            }
            for (var i = 0; i < 4; i++)
            {
                var point = TetrisShapes.Cell(kind, pose, i);
                _grid[point.X, point.Y] = (int)kind + 1;
            }
            return FullRows();
        }

        public int[] FullRows() =>
            Enumerable
                .Range(0, Rows)
                .Where(y => Enumerable.Range(0, Columns).All(x => _grid[x, y] != 0))
                .ToArray();

        public void Clear(int[] rows)
        {
            var write = Rows - 1;
            for (var read = Rows - 1; read >= 0; read--)
            {
                if (Array.IndexOf(rows, read) >= 0)
                    continue;
                for (var x = 0; x < Columns; x++)
                    _grid[x, write] = _grid[x, read];
                write--;
            }
            for (; write >= 0; write--)
            for (var x = 0; x < Columns; x++)
                _grid[x, write] = 0;
        }

        public double Cost()
        {
            var heights = new int[Columns];
            var holes = 0;
            var coveredDepth = 0;
            for (var x = 0; x < Columns; x++)
            {
                var covered = 0;
                for (var y = 0; y < Rows; y++)
                {
                    if (_grid[x, y] != 0)
                    {
                        if (heights[x] == 0)
                            heights[x] = Rows - y;
                        covered++;
                    }
                    else if (covered > 0)
                    {
                        holes++;
                        coveredDepth += covered;
                    }
                }
            }
            var roughness = 0;
            for (var x = 1; x < Columns; x++)
                roughness += Math.Abs(heights[x] - heights[x - 1]);
            return holes * 22d
                + coveredDepth * 1.2d
                + heights.Sum() * 0.65d
                + heights.Max() * 0.8d
                + roughness * 0.65d;
        }
    }
}
