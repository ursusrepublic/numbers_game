namespace Game.App.Daily
{
    public sealed class DailyCalendarDayState
    {
        public bool IsEmpty;
        public DailyChallengeDateKey Date;
        public int DayNumber;
        public bool IsToday;
        public bool IsFuture;
        public bool IsSelectable;
        public bool IsSelected;
        public bool IsCompleted;
        public bool HasProgress;
        public bool HasActiveRun;
        public float Progress01;
    }
}
