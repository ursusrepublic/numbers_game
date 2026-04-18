using Game.Gameplay.Board;
using UnityEngine;

namespace Game.Gameplay.Score
{
    public sealed class ScoreService
    {
        public int TotalScore { get; private set; }

        public ScoreResult ApplyMatch(BoardMatchResolution resolution, int multiplier)
        {
            int safeMultiplier = Mathf.Max(1, multiplier);
            int pairScore = GetPairScore(resolution.MatchInfo);
            int rowClearBonus = resolution.BoardCleared ? 0 : resolution.NewlyClearedRowCount * 10;
            int boardClearBonus = resolution.BoardCleared ? 150 : 0;
            int awardedScore = Mathf.RoundToInt((pairScore + rowClearBonus + boardClearBonus) * safeMultiplier);

            TotalScore += awardedScore;

            return new ScoreResult(
                pairScore,
                rowClearBonus,
                boardClearBonus,
                safeMultiplier,
                awardedScore,
                TotalScore);
        }

        private int GetPairScore(BoardMatchInfo matchInfo)
        {
            if (!matchInfo.IsAdjacent)
            {
                return 4;
            }

            return matchInfo.PositionType == BoardPositionType.RowBoundary
                ? 2
                : 1;
        }
    }
}
