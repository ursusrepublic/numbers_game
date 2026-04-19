using System;
using System.Collections.Generic;

namespace Game.Gameplay.Board
{
    public sealed class BoardState
    {
        private readonly List<BoardCell> _cells;
        private readonly int _columns;
        private readonly BoardMatchRules _matchRules;

        public BoardState(IEnumerable<BoardCell> cells, int columns, BoardMatchRules matchRules)
        {
            _cells = new List<BoardCell>(cells);
            _columns = columns;
            _matchRules = matchRules;
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
            if (!_matchRules.TryGetMatchInfo(
                    _cells,
                    _columns,
                    firstIndex,
                    secondIndex,
                    out BoardMatchInfo matchInfo,
                    out string failureReason))
            {
                return BoardMatchResolution.Failed(failureReason);
            }

            _cells[firstIndex].IsMatched = true;
            _cells[firstIndex].IsSelected = false;

            _cells[secondIndex].IsMatched = true;
            _cells[secondIndex].IsSelected = false;

            List<int> rowsToRemove = GetMatchedRows(firstIndex, secondIndex);
            int removedRowCount = rowsToRemove.Count;

            if (removedRowCount > 0)
            {
                RemoveRows(rowsToRemove);
            }

            bool boardCleared = AreAllCellsMatched();

            return BoardMatchResolution.Succeeded(
                matchInfo,
                firstIndex,
                secondIndex,
                removedRowCount,
                boardCleared);
        }

        public int DuplicateUnmatchedNumbers()
        {
            var numbersToDuplicate = new List<int>();
            for (int index = 0; index < _cells.Count; index++)
            {
                if (_cells[index].IsMatched)
                {
                    continue;
                }

                numbersToDuplicate.Add(_cells[index].Number);
            }

            for (int index = 0; index < numbersToDuplicate.Count; index++)
            {
                int newIndex = _cells.Count;
                int row = newIndex / _columns;
                int column = newIndex % _columns;
                _cells.Add(new BoardCell(newIndex, row, column, numbersToDuplicate[index]));
            }

            return numbersToDuplicate.Count;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _cells.Count;
        }

        private List<int> GetMatchedRows(int firstIndex, int secondIndex)
        {
            var rowsToCheck = new HashSet<int>
            {
                _cells[firstIndex].Row,
                _cells[secondIndex].Row,
            };

            var matchedRows = new List<int>();
            foreach (int row in rowsToCheck)
            {
                if (IsRowMatched(row))
                {
                    matchedRows.Add(row);
                }
            }

            matchedRows.Sort();
            return matchedRows;
        }

        private bool IsRowMatched(int row)
        {
            int startIndex = row * _columns;
            if (startIndex < 0 || startIndex >= _cells.Count)
            {
                return false;
            }

            int endIndex = startIndex + _columns;
            if (endIndex > _cells.Count)
            {
                return false;
            }

            for (int index = startIndex; index < endIndex; index++)
            {
                if (!_cells[index].IsMatched)
                {
                    return false;
                }
            }

            return true;
        }

        private bool AreAllCellsMatched()
        {
            for (int index = 0; index < _cells.Count; index++)
            {
                if (!_cells[index].IsMatched)
                {
                    return false;
                }
            }

            return true;
        }

        private void RemoveRows(IReadOnlyList<int> rows)
        {
            for (int rowIndex = rows.Count - 1; rowIndex >= 0; rowIndex--)
            {
                int row = rows[rowIndex];
                int startIndex = row * _columns;
                int count = Math.Min(_columns, _cells.Count - startIndex);

                if (startIndex < 0 || count <= 0)
                {
                    continue;
                }

                _cells.RemoveRange(startIndex, count);
            }

            RebuildCells();
        }

        private void RebuildCells()
        {
            for (int index = 0; index < _cells.Count; index++)
            {
                BoardCell oldCell = _cells[index];
                var rebuiltCell = new BoardCell(
                    index,
                    index / _columns,
                    index % _columns,
                    oldCell.Number)
                {
                    IsMatched = oldCell.IsMatched,
                    IsSelected = oldCell.IsSelected,
                };

                _cells[index] = rebuiltCell;
            }
        }
    }
}
