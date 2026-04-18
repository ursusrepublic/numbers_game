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
            return TryGetMatchInfo(cells, columns, firstIndex, secondIndex, out _, out failureReason);
        }

        public bool TryGetMatchInfo(
            IReadOnlyList<BoardCell> cells,
            int columns,
            int firstIndex,
            int secondIndex,
            out BoardMatchInfo matchInfo,
            out string failureReason)
        {
            matchInfo = null;

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

            BoardValueType valueType = GetValueType(firstCell.Number, secondCell.Number);

            if (!TryBuildMatchInfo(cells, columns, firstIndex, secondIndex, valueType, out matchInfo))
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

        private bool TryBuildMatchInfo(
            IReadOnlyList<BoardCell> cells,
            int columns,
            int firstIndex,
            int secondIndex,
            BoardValueType valueType,
            out BoardMatchInfo matchInfo)
        {
            if (HasClearFlatPath(cells, firstIndex, secondIndex))
            {
                bool isSameRow = (firstIndex / columns) == (secondIndex / columns);
                BoardPositionType positionType = isSameRow
                    ? BoardPositionType.Horizontal
                    : BoardPositionType.RowBoundary;

                matchInfo = new BoardMatchInfo(
                    firstIndex,
                    secondIndex,
                    positionType,
                    valueType,
                    Math.Abs(firstIndex - secondIndex) == 1);
                return true;
            }

            if (HasClearVerticalPath(cells, columns, firstIndex, secondIndex))
            {
                matchInfo = new BoardMatchInfo(
                    firstIndex,
                    secondIndex,
                    BoardPositionType.Vertical,
                    valueType,
                    Math.Abs(firstIndex - secondIndex) == columns);
                return true;
            }

            if (HasClearDiagonalPath(cells, columns, firstIndex, secondIndex))
            {
                int firstRow = firstIndex / columns;
                int secondRow = secondIndex / columns;

                matchInfo = new BoardMatchInfo(
                    firstIndex,
                    secondIndex,
                    BoardPositionType.Diagonal,
                    valueType,
                    Math.Abs(firstRow - secondRow) == 1);
                return true;
            }

            matchInfo = null;
            return false;
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

        private BoardValueType GetValueType(int firstNumber, int secondNumber)
        {
            return firstNumber == secondNumber
                ? BoardValueType.SameValue
                : BoardValueType.SumToTen;
        }

        private bool IsValidIndex(int index, int count)
        {
            return index >= 0 && index < count;
        }
    }
}
