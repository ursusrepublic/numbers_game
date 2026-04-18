using System.Collections.Generic;
using System.Linq;

namespace Game.Gameplay.Board
{
    public sealed class BoardState
    {
        private readonly List<BoardCell> _cells;
        private readonly int _columns;
        private readonly BoardMatchRules _matchRules;
        private readonly HashSet<int> _clearedRows;
        private int _matchedCellCount;

        public BoardState(IEnumerable<BoardCell> cells, int columns, BoardMatchRules matchRules)
        {
            _cells = new List<BoardCell>(cells);
            _columns = columns;
            _matchRules = matchRules;
            _clearedRows = new HashSet<int>();
            _matchedCellCount = _cells.Count(cell => cell.IsMatched);

            for (int row = 0; row < GetRowCount(); row++)
            {
                if (IsRowCleared(row))
                {
                    _clearedRows.Add(row);
                }
            }
        }

        public IReadOnlyList<BoardCell> Cells => _cells;

        public bool IsSelectable(int index)
        {
            return IsValidIndex(index) && !_cells[index].IsMatched;
        }

        public void SetSelected(int index, bool isSelected)
        {
            if (!IsSelectable(index))
            {
                return;
            }

            _cells[index].IsSelected = isSelected;
        }

        public BoardMatchResolution TryMatchPair(int firstIndex, int secondIndex)
        {
            if (!_matchRules.TryGetMatchInfo(_cells, _columns, firstIndex, secondIndex, out BoardMatchInfo matchInfo, out string failureReason))
            {
                return BoardMatchResolution.Failed(failureReason);
            }

            _cells[firstIndex].IsMatched = true;
            _cells[firstIndex].IsSelected = false;

            _cells[secondIndex].IsMatched = true;
            _cells[secondIndex].IsSelected = false;

            _matchedCellCount += 2;

            int newlyClearedRowCount = CountNewlyClearedRows(firstIndex, secondIndex);
            bool boardCleared = _matchedCellCount >= _cells.Count;

            return BoardMatchResolution.Succeeded(
                matchInfo,
                firstIndex,
                secondIndex,
                newlyClearedRowCount,
                boardCleared);
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _cells.Count;
        }

        private int CountNewlyClearedRows(int firstIndex, int secondIndex)
        {
            var candidateRows = new HashSet<int>
            {
                _cells[firstIndex].Row,
                _cells[secondIndex].Row,
            };

            int clearedRowCount = 0;
            foreach (int row in candidateRows)
            {
                if (_clearedRows.Contains(row))
                {
                    continue;
                }

                if (!IsRowCleared(row))
                {
                    continue;
                }

                _clearedRows.Add(row);
                clearedRowCount++;
            }

            return clearedRowCount;
        }

        private bool IsRowCleared(int row)
        {
            int startIndex = row * _columns;
            int endIndex = System.Math.Min(startIndex + _columns, _cells.Count);

            for (int index = startIndex; index < endIndex; index++)
            {
                if (!_cells[index].IsMatched)
                {
                    return false;
                }
            }

            return true;
        }

        private int GetRowCount()
        {
            if (_columns <= 0)
            {
                return 0;
            }

            return (_cells.Count + _columns - 1) / _columns;
        }
    }
}
