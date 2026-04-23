using System;

namespace Game.App.Save
{
    [Serializable]
    public sealed class DailyChallengeDaySaveData
    {
        public int Year;
        public int Month;
        public int Day;
        public int GoalScore;
        public int AccumulatedScore;
        public bool IsCompleted;
        public RunSaveData ActiveRun;
    }
}
