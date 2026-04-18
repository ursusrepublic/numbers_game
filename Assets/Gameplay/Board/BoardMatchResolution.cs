namespace Game.Gameplay.Board
{
    public sealed class BoardMatchResolution
    {
        private BoardMatchResolution(
            bool success,
            string failureReason,
            BoardMatchInfo matchInfo,
            int firstIndex,
            int secondIndex,
            int newlyClearedRowCount,
            bool boardCleared)
        {
            Success = success;
            FailureReason = failureReason;
            MatchInfo = matchInfo;
            FirstIndex = firstIndex;
            SecondIndex = secondIndex;
            NewlyClearedRowCount = newlyClearedRowCount;
            BoardCleared = boardCleared;
        }

        public bool Success { get; }

        public string FailureReason { get; }

        public BoardMatchInfo MatchInfo { get; }

        public int FirstIndex { get; }

        public int SecondIndex { get; }

        public int NewlyClearedRowCount { get; }

        public bool BoardCleared { get; }

        public static BoardMatchResolution Failed(string failureReason)
        {
            return new BoardMatchResolution(false, failureReason, null, -1, -1, 0, false);
        }

        public static BoardMatchResolution Succeeded(
            BoardMatchInfo matchInfo,
            int firstIndex,
            int secondIndex,
            int newlyClearedRowCount,
            bool boardCleared)
        {
            return new BoardMatchResolution(
                true,
                string.Empty,
                matchInfo,
                firstIndex,
                secondIndex,
                newlyClearedRowCount,
                boardCleared);
        }
    }
}
