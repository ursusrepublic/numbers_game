using System;
using System.Collections.Generic;

namespace Game.Gameplay.Board
{
    public sealed class BoardMatchRules
    {
        public bool TryValidatePair(
            IReadOnlyList<BoardCell> cells,
            int columns,
            int firstIndex,
            int secondIndex,
            out string failureReason)
        {
            if (!IsValidIndex(firstIndex, cells.Count) || !IsValidIndex(secondIndex, cells.Count))
            {
                failureReason = "One of the selected cells is outside the board.";
                return false;
            }

            if (firstIndex == secondIndex)
            {
                failureReason = "Pick two different cells.";
                return false;
            }

            BoardCell firstCell = cells[firstIndex];
            BoardCell secondCell = cells[secondIndex];

            if (firstCell.IsMatched || secondCell.IsMatched)
            {
                failureReason = "Matched cells can no longer be used.";
                return false;
            }

            if (!IsValueMatch(firstCell.Number, secondCell.Number))
            {
                failureReason = "Values must be equal or sum to 10.";
                return false;
            }

            if (!HasClearPath(cells, columns, firstIndex, secondIndex))
            {
                failureReason = "Cells need a clear flat, vertical, or diagonal path through cleared cells.";
                return false;
            }

            failureReason = string.Empty;
            return true;
        }

        public bool IsValueMatch(int firstNumber, int secondNumber)
        {
            return firstNumber == secondNumber || firstNumber + secondNumber == 10;
        }

        private bool HasClearPath(IReadOnlyList<BoardCell> cells, int columns, int firstIndex, int secondIndex)
        {
            return HasClearFlatPath(cells, firstIndex, secondIndex) ||
                   HasClearVerticalPath(cells, columns, firstIndex, secondIndex) ||
                   HasClearDiagonalPath(cells, columns, firstIndex, secondIndex);
        }

        private bool HasClearFlatPath(IReadOnlyList<BoardCell> cells, int firstIndex, int secondIndex)
        {
            int startIndex = Math.Min(firstIndex, secondIndex) + 1;
            int endIndex = Math.Max(firstIndex, secondIndex);

            for (int index = startIndex; index < endIndex; index++)
            {
                if (!cells[index].IsMatched)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasClearVerticalPath(IReadOnlyList<BoardCell> cells, int columns, int firstIndex, int secondIndex)
        {
            if (columns <= 0)
            {
                return false;
            }

            int firstColumn = firstIndex % columns;
            int secondColumn = secondIndex % columns;
            if (firstColumn != secondColumn)
            {
                return false;
            }

            int startIndex = Math.Min(firstIndex, secondIndex) + columns;
            int endIndex = Math.Max(firstIndex, secondIndex);

            for (int index = startIndex; index < endIndex; index += columns)
            {
                if (!cells[index].IsMatched)
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasClearDiagonalPath(IReadOnlyList<BoardCell> cells, int columns, int firstIndex, int secondIndex)
        {
            if (columns <= 0)
            {
                return false;
            }

            int firstRow = firstIndex / columns;
            int firstColumn = firstIndex % columns;
            int secondRow = secondIndex / columns;
            int secondColumn = secondIndex % columns;

            int rowDelta = secondRow - firstRow;
            int columnDelta = secondColumn - firstColumn;

            if (Math.Abs(rowDelta) != Math.Abs(columnDelta))
            {
                return false;
            }

            int rowStep = Math.Sign(rowDelta);
            int columnStep = Math.Sign(columnDelta);

            int row = firstRow + rowStep;
            int column = firstColumn + columnStep;

            while (row != secondRow && column != secondColumn)
            {
                int index = column + (row * columns);
                if (!cells[index].IsMatched)
                {
                    return false;
                }

                row += rowStep;
                column += columnStep;
            }

            return true;
        }

        private bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }
    }
}
