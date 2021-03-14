using System;
using System.Text.RegularExpressions;

namespace Ritsukage.Tools
{
    public static class TimeSpanReader
    {
        static readonly Regex TSMatcher = new Regex(@"((?<day>\d+)(天|日|days|day|d))?((?<hour>\d+)(小?时|hours|hour|h))?((?<minute>\d+)(分钟?|minutes|minute|min|m))?((?<second>\d+)(秒|seconds|second|sec|s))?");

        public static TimeSpan Parse(string original)
        {
            var s = original.ToLower();
            var m = TSMatcher.Match(s);
            if (!int.TryParse(m.Groups["day"].Value, out int day))
                day = 0;
            if (!int.TryParse(m.Groups["hour"].Value, out int hour))
                hour = 0;
            if (!int.TryParse(m.Groups["minute"].Value, out int minute))
                minute = 0;
            if (!int.TryParse(m.Groups["second"].Value, out int second))
                second = 0;
            if (string.IsNullOrWhiteSpace(m.Value))
                throw new ArgumentException($"{original} is not a timespan value.");
            return new TimeSpan(day, hour, minute, second);
        }
    }
}
