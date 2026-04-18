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

        public bool TryMatchPair(int firstIndex, int secondIndex, out string failureReason)
        {
            if (!_matchRules.TryValidatePair(_cells, _columns, firstIndex, secondIndex, out failureReason))
            {
                return false;
            }

            _cells[firstIndex].IsMatched = true;
            _cells[firstIndex].IsSelected = false;

            _cells[secondIndex].IsMatched = true;
            _cells[secondIndex].IsSelected = false;

            return true;
        }

        private bool IsValidIndex(int index)
        {
            return index >= 0 && index < _cells.Count;
        }
    }
}
