using Ritsukage.Library.FFXIV.Enum;
using System;

namespace Ritsukage.Library.FFXIV.Struct
{
    public struct EorzeaTime
    {
        public const double EORZEA_TIME_CONST = 3600.0 / 175.0;
        public const int MonthsOfYear = 12;
        public const int DaysOfMonth = 32;
        public const int HoursOfDay = 24;
        public const int MinutesOfHour = 60;
        public const int SecondsOfMinute = 60;
        public const int DaysOfYear = DaysOfMonth * MonthsOfYear;
        public const int HoursOfYear = HoursOfDay * DaysOfMonth * MonthsOfYear;
        public const int HoursOfMonth = HoursOfDay * DaysOfMonth;
        public const int MinutesOfYear = MinutesOfHour * HoursOfDay * DaysOfMonth * MonthsOfYear;
        public const int MinutesOfMonth = MinutesOfHour * HoursOfDay * DaysOfMonth;
        public const int MinutesOfDay = MinutesOfHour * HoursOfDay;
        public const int SecondsOfYear = SecondsOfMinute * MinutesOfHour * HoursOfDay * DaysOfMonth * MonthsOfYear;
        public const int SecondsOfMonth = SecondsOfMinute * MinutesOfHour * HoursOfDay * DaysOfMonth;
        public const int SecondsOfDay = SecondsOfMinute * MinutesOfHour * HoursOfDay;
        public const int SecondsOfHour = SecondsOfMinute * MinutesOfHour;

        static readonly DateTime DATETIME_ZERO = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        public static EorzeaTime Now => new(DateTime.UtcNow);

        double ET;

        public double TotalYears
        {
            get => TotalMonths / MonthsOfYear;
            set => TotalMonths = value * MonthsOfYear;
        }

        public double TotalMonths
        {
            get => TotalDays / DaysOfMonth;
            set => TotalDays = value * DaysOfMonth;
        }

        public double TotalDays
        {
            get => TotalHours / HoursOfDay;
            set => TotalHours = value * HoursOfDay;
        }

        public double TotalHours
        {
            get => TotalMinutes / MinutesOfHour;
            set => TotalMinutes = value * MinutesOfHour;
        }

        public double TotalMinutes
        {
            get => ET / SecondsOfMinute;
            set => ET = value * SecondsOfMinute;
        }

        public double TotalSeconds
        {
            get => ET;
            set => ET = value;
        }

        public double UnixTime
        {
            get => ET / EORZEA_TIME_CONST;
            set => ET = value * EORZEA_TIME_CONST;
        }

        public int Year
        {
            get => (int)Math.Floor(TotalYears);
            set => ET = value + ET % SecondsOfYear;
        }

        public int Month
        {
            get => (int)(Math.Floor(TotalMonths % MonthsOfYear) + 1);
            set => ET = Math.Floor(TotalYears) * SecondsOfYear + (value - 1) * SecondsOfMonth + ET % SecondsOfMonth;
        }

        public int Day
        {
            get => (int)(Math.Floor(TotalDays % DaysOfMonth) + 1);
            set => ET = Math.Floor(TotalMonths) * SecondsOfMonth + (value - 1) * SecondsOfDay + ET % SecondsOfDay;
        }

        public int Hour
        {
            get => (int)Math.Floor(TotalHours % HoursOfDay);
            set => ET = Math.Floor(TotalDays) * SecondsOfDay + value * SecondsOfHour + ET % SecondsOfHour;
        }

        public int Minute
        {
            get => (int)Math.Floor(TotalMinutes % MinutesOfHour);
            set => ET = Math.Floor(TotalHours) * SecondsOfHour + value * SecondsOfMinute + ET % SecondsOfMinute;
        }

        public int Second
        {
            get => (int)Math.Floor(TotalSeconds % SecondsOfMinute);
            set => ET = Math.Floor(TotalMinutes) * SecondsOfMinute + value;
        }

        public TheTwelve TheTwelve => (TheTwelve)(Month - 1);
        public Polarity Polarity => (Polarity)((Month - 1) % 2);
        public Aether Aether => (Aether)((Month - 1) / 2);

        public DateTime DateTime => DateTimeOffset.DateTime.ToLocalTime();
        public DateTimeOffset DateTimeOffset => DateTimeOffset.FromUnixTimeSeconds((long)UnixTime);

        public EorzeaTime(double time)
        {
            ET = time * EORZEA_TIME_CONST;
        }

        public EorzeaTime(DateTimeOffset time)
            : this(time.ToUnixTimeSeconds())
        { }

        public EorzeaTime(DateTime time)
            : this((DateTimeOffset)time)
        { }

        public override string ToString()
            => $"Eorzer Time: {Year}/{Month:D2}/{Day:D2} {Hour:D2}:{Minute:D2}";

        public static EorzeaTime operator +(EorzeaTime et, TimeSpan ts)
            => new(et.UnixTime + ts.TotalSeconds);
        public static TimeSpan operator -(EorzeaTime et1, EorzeaTime et2)
            => TimeSpan.FromSeconds((et1.ET - et2.ET) / EORZEA_TIME_CONST);
        public static EorzeaTime operator -(EorzeaTime et, TimeSpan ts)
            => new(et.UnixTime - ts.TotalSeconds);
        public static bool operator ==(EorzeaTime et1, EorzeaTime et2)
            => et1.ET == et2.ET;
        public static bool operator !=(EorzeaTime et1, EorzeaTime et2)
            => !(et1 == et2);
        public static bool operator <(EorzeaTime et1, EorzeaTime et2)
            => et1.ET < et2.ET;
        public static bool operator >(EorzeaTime et1, EorzeaTime et2)
            => et1.ET > et2.ET;
        public static bool operator <=(EorzeaTime et1, EorzeaTime et2)
            => et1.ET <= et2.ET;
        public static bool operator >=(EorzeaTime et1, EorzeaTime et2)
            => et1.ET >= et2.ET;
    }
}
