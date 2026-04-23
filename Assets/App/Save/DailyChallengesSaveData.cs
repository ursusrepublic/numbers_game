using System;

namespace Game.App.Save
{
    [Serializable]
    public sealed class DailyChallengesSaveData
    {
        public int ViewedYear;
        public int ViewedMonth;
        public int SelectedYear;
        public int SelectedMonth;
        public int SelectedDay;
        public DailyChallengeDaySaveData[] Days;
    }
}
