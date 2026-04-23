using System;

namespace Game.App.Daily
{
    [Serializable]
    public struct DailyChallengeDateKey : IEquatable<DailyChallengeDateKey>
    {
        public int Year;
        public int Month;
        public int Day;

        public DailyChallengeDateKey(int year, int month, int day)
        {
            Year = year;
            Month = month;
            Day = day;
        }

        public DailyChallengeDateKey(DateTime dateTime)
        {
            Year = dateTime.Year;
            Month = dateTime.Month;
            Day = dateTime.Day;
        }

        public DateTime ToDateTime()
        {
            return new DateTime(Year, Month, Day);
        }

        public bool Equals(DailyChallengeDateKey other)
        {
            return Year == other.Year && Month == other.Month && Day == other.Day;
        }

        public override bool Equals(object obj)
        {
            return obj is DailyChallengeDateKey other && Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Year;
                hashCode = (hashCode * 397) ^ Month;
                hashCode = (hashCode * 397) ^ Day;
                return hashCode;
            }
        }

        public override string ToString()
        {
            return $"{Year:D4}-{Month:D2}-{Day:D2}";
        }
    }
}
