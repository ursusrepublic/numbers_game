using System.Collections.Generic;

namespace Game.Gameplay.Board
{
    public sealed class BoardPairFinder
    {
        private readonly BoardMatchRules _matchRules;

        public BoardPairFinder(BoardMatchRules matchRules)
        {
            _matchRules = matchRules;
        }

        public List<BoardMatchInfo> FindAll(IReadOnlyList<BoardCell> cells, int columns)
        {
            var pairs = new List<BoardMatchInfo>();

            for (int firstIndex = 0; firstIndex < cells.Count; firstIndex++)
            {
                if (cells[firstIndex].IsMatched)
                {
                    continue;
                }

                for (int secondIndex = firstIndex + 1; secondIndex < cells.Count; secondIndex++)
                {
                    if (cells[secondIndex].IsMatched)
                    {
                        continue;
                    }

                    if (_matchRules.TryGetMatchInfo(cells, columns, firstIndex, secondIndex, out BoardMatchInfo matchInfo, out _))
                    {
                        pairs.Add(matchInfo);
                    }
                }
            }

            return pairs;
        }
    }
}
