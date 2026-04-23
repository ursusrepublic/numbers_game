using System;

namespace Game.App.Save
{
    [Serializable]
    public sealed class RunSaveData
    {
        public int Columns;
        public int InitialRows;
        public int StartingPairs;
        public int StartingAdditions;
        public int StartingHints;
        public int CurrentScore;
        public int ClearedBoardCount;
        public int RemainingAdditions;
        public int RemainingHints;
        public int CurrentBoardSeed;
        public int NextBoardSeed;
        public int HintedFirstIndex = -1;
        public int HintedSecondIndex = -1;
        public CellSaveData[] Cells;
    }
}
