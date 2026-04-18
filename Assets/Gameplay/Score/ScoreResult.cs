namespace Game.Gameplay.Score
{
    public sealed class ScoreResult
    {
        public ScoreResult(
            int pairScore,
            int rowClearBonus,
            int boardClearBonus,
            int multiplier,
            int awardedScore,
            int totalScore)
        {
            PairScore = pairScore;
            RowClearBonus = rowClearBonus;
            BoardClearBonus = boardClearBonus;
            Multiplier = multiplier;
            AwardedScore = awardedScore;
            TotalScore = totalScore;
        }

        public int PairScore { get; }

        public int RowClearBonus { get; }

        public int BoardClearBonus { get; }

        public int Multiplier { get; }

        public int AwardedScore { get; }

        public int TotalScore { get; }
    }
}
