using System;

namespace Ritsukage.Library.ShouSi
{
    public struct ShouSiDate
    {
        public static readonly DateTime BaseDate = new DateTime(2021, 08, 21).Date;

        public int Year { get; }
        public int Month { get; }
        public int Day { get; }
        public TimeSpan TimeOfDay { get; }

        private static bool IsLeap(int year)
        {
            return (year % 4 == 0 && year % 100 != 0) || year % 400 == 0;
        }

        private static readonly int[] Days = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };
        private static readonly int[] LeapDays = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31 };

        public ShouSiDate(DateTime now)
        {
            TimeOfDay = now.TimeOfDay;
            var dt = now - BaseDate;
            if (dt.TotalSeconds < 0)
                throw new ArgumentOutOfRangeException(nameof(now), "寿司历于2021年08月21号开始计时");
            var year = 1;
            var month = 1;
            var day = 1 + dt.Days;
            var n = 1;
            while (day > 0)
            {
                var days = 365;
                if (IsLeap(n))
                    days = 366;
                if (day > days)
                {
                    year++;
                    day -= days;
                }
                else
                {
                    break;
                }

                n++;
            }

            n = 1;
            var _days = IsLeap(year) ? LeapDays : Days;
            while (day > 0)
            {
                var days = _days[n - 1];
                if (day > days)
                {
                    month++;
                    day -= days;
                }
                else
                {
                    break;
                }

                if (n == 12)
                    n = 1;
                else
                    n++;
            }

            Year = year;
            Month = month;
            Day = day;
        }

        public static ShouSiDate Now => new(DateTime.Now);

        public override string ToString()
        {
            return $"{Year}-{Month}-{Day} {TimeOfDay.Hours:D2}:{TimeOfDay.Minutes:D2}:{TimeOfDay.Seconds:D2}";
        }
    }
}