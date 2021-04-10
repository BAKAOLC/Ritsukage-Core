using System;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static class TimeSpanReader
    {
        static readonly Regex TSMatcher = new Regex(@"((?<day>\d+)(天|日|d(ays?)?))?((?<hour>\d+)(小?时|h(ours?)?))?((?<minute>\d+)(分钟?|m(in(utes?)?)?))?((?<second>\d+)(秒|s(ec(onds?)?)?))?");

        public static TimeSpan Parse(string original)
        {
            var s = original.ToLower();
            var m = TSMatcher.Match(s);
            bool flag = false;
            int day = 0, hour = 0, minute = 0, second = 0;
            flag = flag || int.TryParse(m.Groups["day"].Value, out day);
            flag = flag || int.TryParse(m.Groups["hour"].Value, out hour);
            flag = flag || int.TryParse(m.Groups["minute"].Value, out minute);
            flag = flag || int.TryParse(m.Groups["second"].Value, out second);
            if (!flag)
                throw new ArgumentException($"{original} is not a timespan value.");
            return new TimeSpan(day, hour, minute, second);
        }
    }
}
