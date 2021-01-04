using System;
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static class DateTimeReader
    {
        static readonly string[] DateFormats = {
            "%y'年'%M'月'%d'日'",      //2021年01月04日
            "%y'/'%M'/'%d",            //2021/01/04
            "%y'-'%M'-'%d",            //2021-01-04
            "%y'年'%M'月'",            //2021年01月
            "%y'/'%M",                 //2021/01
            "%y'-'%M'",                //2021-01
            "%M'月'%d'日'",            //01月04日
            "%M'/'%d",                 //01/04
            "%M'-'%d",                 //01-04
            "%y'年'",                  //2021年
            "%M'月'",                  //01月
            "%d'日'",                  //04日
        };
        static readonly string[] DateMatch = {
            @"\d+年\d+月\d+日",
            @"\d+/\d+/\d+",
            @"\d+\-\d+\-\d+",
            @"\d+月\d+日",
            @"\d+年\d+月",
            @"\d+/\d+",
            @"\d+\-\d+",
            @"\d+年",
            @"\d+月",
            @"\d+日",
        };
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

        public static DateTime Parse(string original)
        {
            var s = original.ToLower();

            string date = string.Empty;
            string time = string.Empty;
            foreach (var dm in DateMatch)
            {
                var r = Regex.Match(s, dm);
                if (r.Success)
                {
                    date = r.Value;
                    break;
                }
            }
            foreach (var tm in TimeMatch)
            {
                var r = Regex.Match(s, tm);
                if (r.Success)
                {
                    time = r.Value;
                    break;
                }
            }

            DateTime? Date = null;
            TimeSpan? Time = null;
            if (DateTime.TryParseExact(date, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var gotDate))
                Date = gotDate.Date;
            if (DateTime.TryParseExact(time, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var gotTime))
                Time = gotTime.TimeOfDay;

            if (Date.HasValue && Time.HasValue)
                return Date.Value + Time.Value;
            else if (Date.HasValue)
                return Date.Value;
            else if (Time.HasValue)
                return DateTime.Today + Time.Value;
            else throw new ArgumentException($"{original} is not a datetime value.");
        }
    }
}
