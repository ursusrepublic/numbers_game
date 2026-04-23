using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Game.App.Save;
using UnityEngine;

namespace Game.App.Daily
{
    public sealed class DailyChallengesService
    {
        private static readonly int[] GoalPattern =
        {
            100, 150, 80, 90, 160, 200, 220,
            150, 120, 130, 110, 250, 270, 280,
            300, 170, 190, 110, 160, 200, 210,
            220, 230, 240, 250, 300, 260, 160,
            190, 180, 150
        };

        private readonly DailyChallengesSaveData _saveData;

        public DailyChallengesService(DailyChallengesSaveData saveData)
        {
            _saveData = saveData ?? new DailyChallengesSaveData();
            _saveData.Days ??= Array.Empty<DailyChallengeDaySaveData>();
            EnsureInitialized(DateTime.Today);
        }

        public DailyChallengesSaveData SaveData => _saveData;

        public DateTime FirstSupportedMonth => new DateTime(2025, 10, 1);

        public DateTime CurrentMonth
        {
            get
            {
                DateTime today = DateTime.Today;
                return new DateTime(today.Year, today.Month, 1);
            }
        }

        public DateTime GetViewedMonth()
        {
            EnsureInitialized(DateTime.Today);
            return new DateTime(_saveData.ViewedYear, _saveData.ViewedMonth, 1);
        }

        public DailyChallengeDateKey GetSelectedDate()
        {
            EnsureInitialized(DateTime.Today);
            return new DailyChallengeDateKey(
                _saveData.SelectedYear,
                _saveData.SelectedMonth,
                _saveData.SelectedDay);
        }

        public void EnsureInitialized(DateTime today)
        {
            DateTime currentMonth = new DateTime(today.Year, today.Month, 1);
            DateTime clampedCurrentMonth = ClampMonth(currentMonth);

            if (_saveData.ViewedYear <= 0 || _saveData.ViewedMonth <= 0)
            {
                _saveData.ViewedYear = clampedCurrentMonth.Year;
                _saveData.ViewedMonth = clampedCurrentMonth.Month;
            }

            DateTime viewedMonth = ClampMonth(new DateTime(_saveData.ViewedYear, Mathf.Max(1, _saveData.ViewedMonth), 1));
            _saveData.ViewedYear = viewedMonth.Year;
            _saveData.ViewedMonth = viewedMonth.Month;

            bool hasSelectedDate = _saveData.SelectedYear > 0 && _saveData.SelectedMonth > 0 && _saveData.SelectedDay > 0;
            if (!hasSelectedDate)
            {
                DailyChallengeDateKey defaultSelection = GetDefaultSelectionForMonth(viewedMonth, today);
                SetSelectedDate(defaultSelection);
                return;
            }

            var selected = new DailyChallengeDateKey(
                _saveData.SelectedYear,
                _saveData.SelectedMonth,
                _saveData.SelectedDay);

            if (!IsSelectable(selected, today))
            {
                DailyChallengeDateKey defaultSelection = GetDefaultSelectionForMonth(viewedMonth, today);
                SetSelectedDate(defaultSelection);
            }
        }

        public void SetViewedMonth(DateTime month, DateTime today)
        {
            DateTime clamped = ClampMonth(new DateTime(month.Year, month.Month, 1));
            _saveData.ViewedYear = clamped.Year;
            _saveData.ViewedMonth = clamped.Month;

            DailyChallengeDateKey selected = GetSelectedDate();
            if (selected.Year != clamped.Year || selected.Month != clamped.Month || !IsSelectable(selected, today))
            {
                SetSelectedDate(GetDefaultSelectionForMonth(clamped, today));
            }
        }

        public void SetSelectedDate(DailyChallengeDateKey date)
        {
            _saveData.SelectedYear = date.Year;
            _saveData.SelectedMonth = date.Month;
            _saveData.SelectedDay = date.Day;
        }

        public bool IsSelectable(DailyChallengeDateKey date, DateTime today)
        {
            DateTime challengeDate = date.ToDateTime().Date;
            return challengeDate >= FirstSupportedMonth.Date && challengeDate <= today.Date;
        }

        public bool IsFuture(DailyChallengeDateKey date, DateTime today)
        {
            return date.ToDateTime().Date > today.Date;
        }

        public int GetGoalScore(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
            return Mathf.Max(0, daySave.GoalScore);
        }

        public int GetDisplayedProgress(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
            int activeRunScore = daySave.ActiveRun != null ? Mathf.Max(0, daySave.ActiveRun.CurrentScore) : 0;
            int goal = Mathf.Max(0, daySave.GoalScore);
            int progress = Mathf.Max(0, daySave.AccumulatedScore) + activeRunScore;
            return goal > 0 ? Mathf.Min(goal, progress) : progress;
        }

        public int GetAccumulatedScore(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
            return Mathf.Max(0, daySave.AccumulatedScore);
        }

        public bool IsCompleted(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = FindDay(date);
            return daySave != null && daySave.IsCompleted;
        }

        public bool HasActiveRun(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = FindDay(date);
            return daySave != null && daySave.ActiveRun != null;
        }

        public RunSaveData GetActiveRun(DailyChallengeDateKey date)
        {
            return FindDay(date)?.ActiveRun;
        }

        public void SetActiveRun(DailyChallengeDateKey date, RunSaveData runSave)
        {
            DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
            daySave.ActiveRun = runSave;
        }

        public void ClearActiveRun(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = FindDay(date);
            if (daySave != null)
            {
                daySave.ActiveRun = null;
            }
        }

        public void ApplyCompletedRun(DailyChallengeDateKey date, int runScore)
        {
            DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
            daySave.ActiveRun = null;
            daySave.AccumulatedScore = Mathf.Max(0, daySave.AccumulatedScore) + Mathf.Max(0, runScore);
            if (daySave.AccumulatedScore >= daySave.GoalScore)
            {
                daySave.AccumulatedScore = daySave.GoalScore;
                daySave.IsCompleted = true;
            }
        }

        public DailyCalendarMonthState BuildMonthState(DateTime today)
        {
            EnsureInitialized(today);

            DateTime viewedMonth = GetViewedMonth();
            DailyChallengeDateKey selectedDate = GetSelectedDate();
            int daysInMonth = DateTime.DaysInMonth(viewedMonth.Year, viewedMonth.Month);
            int emptyDaysCount = (int)new DateTime(viewedMonth.Year, viewedMonth.Month, 1).DayOfWeek;
            int totalCells = emptyDaysCount + daysInMonth;
            int gridCells = Mathf.CeilToInt(totalCells / 7f) * 7;

            var dayStates = new DailyCalendarDayState[gridCells];
            int completedDays = 0;

            for (int index = 0; index < gridCells; index++)
            {
                if (index < emptyDaysCount || index >= emptyDaysCount + daysInMonth)
                {
                    dayStates[index] = new DailyCalendarDayState { IsEmpty = true };
                    continue;
                }

                int day = (index - emptyDaysCount) + 1;
                var date = new DailyChallengeDateKey(viewedMonth.Year, viewedMonth.Month, day);
                DailyChallengeDaySaveData daySave = GetOrCreateDay(date);
                int goal = Mathf.Max(0, daySave.GoalScore);
                int progress = GetDisplayedProgress(date);
                float progress01 = goal > 0 ? Mathf.Clamp01((float)progress / goal) : 0f;
                bool isCompleted = daySave.IsCompleted;
                bool isFuture = IsFuture(date, today);
                bool isSelectable = IsSelectable(date, today);

                if (isCompleted)
                {
                    completedDays++;
                }

                dayStates[index] = new DailyCalendarDayState
                {
                    IsEmpty = false,
                    Date = date,
                    DayNumber = day,
                    IsToday = today.Date == date.ToDateTime().Date,
                    IsFuture = isFuture,
                    IsSelectable = isSelectable,
                    IsSelected = selectedDate.Equals(date),
                    IsCompleted = isCompleted,
                    HasProgress = progress > 0 && !isCompleted,
                    HasActiveRun = daySave.ActiveRun != null,
                    Progress01 = progress01,
                };
            }

            DailyCalendarSelectedDayState selectedDayState = BuildSelectedDayState(selectedDate, today);

            return new DailyCalendarMonthState
            {
                Year = viewedMonth.Year,
                Month = viewedMonth.Month,
                MonthLabel = viewedMonth.ToString("MMMM yyyy", CultureInfo.CurrentCulture),
                CompletedDays = completedDays,
                TotalDays = daysInMonth,
                CanGoPreviousMonth = viewedMonth > FirstSupportedMonth,
                CanGoNextMonth = viewedMonth < CurrentMonth,
                SelectedDate = selectedDate,
                SelectedDayState = selectedDayState,
                Days = dayStates,
            };
        }

        private DailyCalendarSelectedDayState BuildSelectedDayState(DailyChallengeDateKey selectedDate, DateTime today)
        {
            if (!IsSelectable(selectedDate, today))
            {
                return new DailyCalendarSelectedDayState
                {
                    HasSelection = false,
                    ActionLabel = "Play",
                    ActionEnabled = false,
                };
            }

            DailyChallengeDaySaveData daySave = GetOrCreateDay(selectedDate);
            int progress = GetDisplayedProgress(selectedDate);
            if (daySave.IsCompleted)
            {
                return new DailyCalendarSelectedDayState
                {
                    HasSelection = true,
                    Date = selectedDate,
                    GoalScore = daySave.GoalScore,
                    ProgressScore = daySave.GoalScore,
                    IsCompleted = true,
                    HasActiveRun = false,
                    IsFuture = false,
                    ActionLabel = "Completed",
                    ActionEnabled = false,
                };
            }

            bool hasActiveRun = daySave.ActiveRun != null;
            return new DailyCalendarSelectedDayState
            {
                HasSelection = true,
                Date = selectedDate,
                GoalScore = daySave.GoalScore,
                ProgressScore = progress,
                IsCompleted = false,
                HasActiveRun = hasActiveRun,
                IsFuture = false,
                ActionLabel = hasActiveRun ? "Continue" : "Play",
                ActionEnabled = true,
            };
        }

        private DailyChallengeDaySaveData FindDay(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData[] days = _saveData.Days ?? Array.Empty<DailyChallengeDaySaveData>();
            for (int index = 0; index < days.Length; index++)
            {
                DailyChallengeDaySaveData daySave = days[index];
                if (daySave != null &&
                    daySave.Year == date.Year &&
                    daySave.Month == date.Month &&
                    daySave.Day == date.Day)
                {
                    return daySave;
                }
            }

            return null;
        }

        private DailyChallengeDaySaveData GetOrCreateDay(DailyChallengeDateKey date)
        {
            DailyChallengeDaySaveData daySave = FindDay(date);
            if (daySave != null)
            {
                if (daySave.GoalScore <= 0)
                {
                    daySave.GoalScore = GetDefaultGoalScore(date);
                }

                return daySave;
            }

            daySave = new DailyChallengeDaySaveData
            {
                Year = date.Year,
                Month = date.Month,
                Day = date.Day,
                GoalScore = GetDefaultGoalScore(date),
                AccumulatedScore = 0,
                IsCompleted = false,
                ActiveRun = null,
            };

            List<DailyChallengeDaySaveData> days = (_saveData.Days ?? Array.Empty<DailyChallengeDaySaveData>()).ToList();
            days.Add(daySave);
            _saveData.Days = days
                .OrderBy(item => item.Year)
                .ThenBy(item => item.Month)
                .ThenBy(item => item.Day)
                .ToArray();
            return daySave;
        }

        private DailyChallengeDateKey GetDefaultSelectionForMonth(DateTime month, DateTime today)
        {
            if (month.Year == today.Year && month.Month == today.Month)
            {
                return new DailyChallengeDateKey(today);
            }

            return new DailyChallengeDateKey(month.Year, month.Month, 1);
        }

        private DateTime ClampMonth(DateTime month)
        {
            if (month < FirstSupportedMonth)
            {
                return FirstSupportedMonth;
            }

            DateTime currentMonth = CurrentMonth;
            if (month > currentMonth)
            {
                return currentMonth;
            }

            return new DateTime(month.Year, month.Month, 1);
        }

        private static int GetDefaultGoalScore(DailyChallengeDateKey date)
        {
            int patternIndex = Mathf.Abs(date.Day - 1) % GoalPattern.Length;
            return GoalPattern[patternIndex];
        }
    }
}
