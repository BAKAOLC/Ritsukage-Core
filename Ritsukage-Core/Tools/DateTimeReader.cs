using System;
using System.Collections;
using System.Globalization;

namespace Ritsukage.Tools
{
    public static class DateTimeReader
    {
        static readonly string[] DateFormats = {
            "%y'年'%M'月'%d'日'",     //2021年01月04日
            "%y'年'%M'月'",          //2021年01月
            "%y'年'",                //2021年
            "%M'月'%d'日'",            //01月04日
            "%M'月'",                  //01月
            "%d'日'",                  //04日
            "%y'-'%M'-'%d",          //2021-01-04
            "%y'-'%M'",              //2021-01
            "%M'-'%d",                 //01-04
            "%y'/'%M'/'%d",          //2021/01/04
            "%y'/'%M",               //2021/01
            "%M'/'%d",                 //01/04
        };
        static readonly string[] TimeFormats = {
            "%H'时'%m'分'%s'秒'", //08时41分20秒
            "%H'时'%m'分'", //08时41分
            "%m'分'%s'秒'", //41分20秒
            "%H':'%m':'%s", //08:41:20
            "%H':'%m", //08:41
        };

        public static DateTime Parse(string original)
        {
            var s = original.ToLower();
            DateTime? Date = null;
            TimeSpan? Time = null;
            if (DateTime.TryParseExact(s, DateFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var gotDate))
                Date = gotDate.Date;
            if (DateTime.TryParseExact(s, TimeFormats, CultureInfo.InvariantCulture, DateTimeStyles.AllowWhiteSpaces, out var gotTime))
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
