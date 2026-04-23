namespace Game.App.Daily
{
    public sealed class DailyCalendarMonthState
    {
        public int Year;
        public int Month;
        public string MonthLabel;
        public int CompletedDays;
        public int TotalDays;
        public bool CanGoPreviousMonth;
        public bool CanGoNextMonth;
        public DailyChallengeDateKey SelectedDate;
        public DailyCalendarSelectedDayState SelectedDayState;
        public DailyCalendarDayState[] Days;
    }
}
