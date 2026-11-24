// parents: @.frostline/graph/m-explorer.xml

using System;
using System.Collections.Generic;

namespace FlexVerse.Test.Explorer.Showcases
{
    /// <summary>
    /// Plans exchanges between occupied bays. Opposite bypass lanes separate
    /// convoys while independent pairs exchange within their own regions.
    /// Planning advances bay identities, while Timeline owns all actual motion.
    /// </summary>
    internal sealed class TrafficFlow
    {
        private readonly int[,] _bays = new int[4, 4];
        private TrafficMotion[] _tiles;

        public void Initialize(TrafficMotion[] tiles)
        {
            _tiles = tiles;
            for (var row = 0; row < 4; row++)
            for (var column = 0; column < 4; column++)
                _bays[column, row] = row * 4 + column;
        }

        public ITimelineAction Build() =>
            Timeline.Sequence(
                Timeline.Parallel(
                    Timeline.Sequence(Timeline.Wait(0.4f), PairedRows(0, 3, 1)),
                    Timeline.Sequence(SideExchange(0, 1), SideExchange(0, 1)),
                    Timeline.Sequence(Timeline.Wait(0.12f), SideExchange(3, 1), SideExchange(3, 1))
                ),
                Timeline.Parallel(
                    Timeline.Sequence(
                        Timeline.Wait(0.4f),
                        Timeline.Parallel(
                            EdgeColumn(0),
                            Timeline.Sequence(Timeline.Wait(0.1f), EdgeColumn(3))
                        )
                    ),
                    Timeline.Sequence(HorizontalExchange(0, -1), HorizontalExchange(0, -1)),
                    Timeline.Sequence(
                        Timeline.Wait(0.1f),
                        HorizontalExchange(3, 4),
                        HorizontalExchange(3, 4)
                    )
                ),
                PairedRows(1, 2, -1)
            );

        private ITimelineAction PairedRows(int upper, int lower, int direction)
        {
            var upperFront = _bays[direction > 0 ? 3 : 0, upper];
            var lowerFront = _bays[direction > 0 ? 3 : 0, lower];
            var upperDeparture = DepartRow(upper, direction);
            var lowerDeparture = DepartRow(lower, direction);
            return Timeline.Sequence(
                Timeline.Parallel(
                    upperDeparture,
                    Timeline.Sequence(Timeline.Wait(0.08f), lowerDeparture)
                ),
                Timeline.Parallel(
                    ReturnToRow(upperFront, upper, direction, -1),
                    ReturnToRow(lowerFront, lower, direction, 4)
                )
            );
        }

        private ITimelineAction DepartRow(int row, int direction)
        {
            var actions = new List<ITimelineAction>();
            var previous = new int[4];
            for (var column = 0; column < 4; column++)
                previous[column] = _bays[column, row];
            for (var offset = 0; offset < 4; offset++)
            {
                var column = direction > 0 ? 3 - offset : offset;
                var tile = _tiles[previous[column]];
                actions.Add(
                    Timeline.Sequence(
                        Timeline.Wait(offset * 0.06f),
                        tile.Drive((column + 1 + direction, row))
                    )
                );
            }
            for (var column = 0; column < 4; column++)
                _bays[column, row] = previous[(column - direction + 4) % 4];
            return Timeline.Parallel(actions.ToArray());
        }

        private ITimelineAction ReturnToRow(int id, int row, int direction, int bypass) =>
            direction > 0
                ? _tiles[id].Drive((5, bypass), (0, bypass), (0, row), (1, row))
                : _tiles[id].Drive((0, bypass), (5, bypass), (5, row), (4, row));

        private ITimelineAction SideExchange(int column, int upperRow)
        {
            var outside = column == 0 ? 0 : 5;
            var x = column + 1;
            var upper = _tiles[_bays[column, upperRow]];
            var lower = _tiles[_bays[column, upperRow + 1]];
            Swap(column, upperRow, column, upperRow + 1);
            return Timeline.Sequence(
                upper.Drive((outside, upperRow)),
                Timeline.Parallel(
                    upper.Drive((outside, upperRow + 1)),
                    Timeline.Sequence(Timeline.Wait(0.08f), lower.Drive((x, upperRow)))
                ),
                upper.Drive((x, upperRow + 1))
            );
        }

        private ITimelineAction EdgeColumn(int column)
        {
            var x = column + 1;
            var outside = column == 0 ? 0 : 5;
            var previous = new int[4];
            for (var row = 0; row < 4; row++)
                previous[row] = _bays[column, row];
            var actions = new List<ITimelineAction>();
            for (var offset = 0; offset < 4; offset++)
            {
                var row = 3 - offset;
                actions.Add(
                    Timeline.Sequence(
                        Timeline.Wait(offset * 0.06f),
                        _tiles[previous[row]].Drive((x, row + 1))
                    )
                );
            }
            for (var row = 0; row < 4; row++)
                _bays[column, row] = previous[(row + 3) % 4];
            return Timeline.Sequence(
                Timeline.Parallel(actions.ToArray()),
                _tiles[previous[3]].Drive((outside, 4), (outside, 0), (x, 0))
            );
        }

        private ITimelineAction HorizontalExchange(int row, int bypass)
        {
            var left = _tiles[_bays[1, row]];
            var right = _tiles[_bays[2, row]];
            Swap(1, row, 2, row);
            return Timeline.Sequence(
                left.Drive((2, bypass)),
                Timeline.Parallel(
                    left.Drive((3, bypass)),
                    Timeline.Sequence(Timeline.Wait(0.05f), right.Drive((2, row)))
                ),
                left.Drive((3, row))
            );
        }

        private void Swap(int x, int y, int otherX, int otherY) =>
            (_bays[x, y], _bays[otherX, otherY]) = (_bays[otherX, otherY], _bays[x, y]);
    }
}
