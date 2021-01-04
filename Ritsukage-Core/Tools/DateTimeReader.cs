using System;
using System.Globalization;

namespace Ritsukage.Tools
{
    public static class DateTimeReader
    {
        static readonly string[] DateFormats = {
            "yyyy'年'MM'月'dd'日'",     //2021年01月04日
            "yyyy'年'MM'月'",          //2021年01月
            "yyyy'年'",                //2021年
            "MM'月'dd'日'",            //01月04日
            "MM'月'",                  //01月
            "dd'日'",                  //04日
            "yyyy'-'MM'-'dd",          //2021-01-04
            "yyyy'-'MM'",              //2021-01
            "MM'-'dd",                 //01-04
            "yyyy'/'MM'/'dd",          //2021/01/04
            "yyyy'/'MM",               //2021/01
            "MM'/'dd",                 //01/04
        };
        static readonly string[] TimeFormats = {
            "HH'时'mm'分'ss'秒'", //08时41分20秒
            "HH'时'mm'分'", //08时41分
            "mm'分'ss'秒'", //41分20秒
            "HH':'mm':'ss", //08:41:20
            "HH':'mm", //08:41
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
