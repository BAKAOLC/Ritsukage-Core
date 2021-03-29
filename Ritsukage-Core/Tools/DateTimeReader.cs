using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static class DateTimeReader
    {
        static readonly Regex[] DateMatch = {
            new Regex(@"((?<year>\d{4})年)?((?<month>\d{1,2})月)?((?<day>\d{1,2})日)?"),
            new Regex(@"(?<year>\d{4})/(?<month>\d{1,2})/(?<day>\d{1,2})"),
            new Regex(@"(?<year>\d{4})-(?<month>\d{1,2})-(?<day>\d{1,2})"),
            new Regex(@"(?<year>\d{4})/(?<month>\d{1,2})"),
            new Regex(@"(?<year>\d{4})-(?<month>\d{1,2})"),
            new Regex(@"(?<month>\d{1,2})/(?<day>\d{1,2})"),
            new Regex(@"(?<month>\d{1,2})-(?<day>\d{1,2})"),
        };
        static DateTime? GetDate(string original)
        {
            var now = DateTime.Now;
            Match m = null;
            foreach (var dm in DateMatch)
            {
                var r = dm.Match(original);
                if (r.Success && !string.IsNullOrWhiteSpace(r.Value))
                {
                    m = r;
                    break;
                }
            }
            if (m != null)
            {
                if (!int.TryParse(m.Groups["year"].Value, out int year))
                    year = now.Year;
                if (!int.TryParse(m.Groups["month"].Value, out int month))
                    month = 1;
                if (!int.TryParse(m.Groups["day"].Value, out int day))
                    day = 1;
                return new DateTime(year, month, day).Date;
            }
            return null;
        }

        static readonly string[] TimeFormats = {
            "%H'时'%m'分'%s'秒'",     //08时41分20秒
            "%H':'%m':'%s",          //08:41:20
            "%H'时'%m'分'",           //08时41分
            "%H':'%m",               //08:41
            "%m'分'%s'秒'",           //41分20秒
        };
        static readonly string[] TimeMatch = {
            @"\d+时\d+分\d+秒",
            @"\d+:\d+:\d+",
            @"\d+时\d+分",
            @"\d+:\d+",
            @"\d+分\d+秒",
        };
        static TimeSpan? GetTime(string original)
        {
            string time = string.Empty;
            bool hour = false;
            int index = 0;
            foreach (var tm in TimeMatch)
            {
                var r = Regex.Match(original, tm);
                if (r.Success)
                {
                    if (index < 4)
                        hour = true;
                    time = r.Value;
                    break;
                }
                index++;
            }
            if (DateTime.TryParseExact(time, TimeFormats, CultureInfo.InvariantCulture,
                DateTimeStyles.AllowWhiteSpaces, out var gotTime))
            {
                if (!hour)
                    return gotTime.TimeOfDay + new TimeSpan(DateTime.Now.Hour, 0, 0);
                else
                    return gotTime.TimeOfDay;
            }
            return null;
        }

        public static DateTime Parse(string original)
        {
            var s = original.ToLower();
            var date = GetDate(s);
            var time = GetTime(s);
            if (date.HasValue && time.HasValue)
                return date.Value + time.Value;
            else if (date.HasValue)
                return date.Value;
            else if (time.HasValue)
                return DateTime.Today + time.Value;
            else
                throw new ArgumentException($"{original} is not a datetime value.");
        }
    }
}
