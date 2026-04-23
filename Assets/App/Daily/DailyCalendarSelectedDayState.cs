namespace Game.App.Daily
{
    public sealed class DailyCalendarSelectedDayState
    {
        public bool HasSelection;
        public DailyChallengeDateKey Date;
        public int GoalScore;
        public int ProgressScore;
        public bool IsCompleted;
        public bool HasActiveRun;
        public bool IsFuture;
        public string ActionLabel;
        public bool ActionEnabled;
    }
}
