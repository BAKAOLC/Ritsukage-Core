using System;
using System.Globalization;

namespace Ritsukage.Tools
{
    public static class TimeSpanReader
    {
        static readonly string[] Formats = {
            "%d'天'%h'小时'%m'分钟'%s'秒'",        //4d3h2m1s
            "%d'天'%h'时'%m'分'%s'秒'",           //4d3h2m1s
            "%d'天'%h'小时'%m'分钟'",             //4d3h2m
            "%d'天'%h'时'%m'分'",                 //4d3h2m
            "%d'天'%h'小时'%s'秒'",               //4d3h  1s
            "%d'天'%h'时'%s'秒'",                 //4d3h  1s
            "%d'天'%h'小时'",                     //4d3h
            "%d'天'%h'时'",                       //4d3h
            "%d'天'%m'分钟'%s'秒'",               //4d  2m1s
            "%d'天'%m'分'%s'秒'",                 //4d  2m1s
            "%d'天'%m'分钟'",                     //4d  2m
            "%d'天'%m'分'",                       //4d  2m
            "%d'天'%s'秒'",                      //4d    1s
            "%d'天'",                            //4d
            "%h'小时'%m'分钟'%s'秒'",             //  3h2m1s
            "%h'时'%m'分'%s'秒'",                 //  3h2m1s
            "%h'小时'%m'分钟'",                   //  3h2m
            "%h'时'%m'分'",                      //  3h2m
            "%h'小时'%s'秒'",                    //  3h  1s
            "%h'时'%s'秒'",                      //  3h  1s
            "%h'小时'",                          //  3h
            "%h'时'",                            //  3h
            "%m'分钟'%s'秒'",                    //    2m1s
            "%m'分'%s'秒'",                      //    2m1s
            "%m'分钟'",                          //    2m
            "%m'分'",                            //    2m
            "%s'秒'",                            //      1s
            "%d'd'%h'h'%m'm'%s's'",              //4d3h2m1s
            "%d'd'%h'h'%m'm'",                   //4d3h2m
            "%d'd'%h'h'%s's'",                   //4d3h  1s
            "%d'd'%h'h'",                        //4d3h
            "%d'd'%m'm'%s's'",                   //4d  2m1s
            "%d'd'%m'm'",                        //4d  2m
            "%d'd'%s's'",                        //4d    1s
            "%d'd'",                             //4d
            "%h'h'%m'm'%s's'",                   //  3h2m1s
            "%h'h'%m'm'",                        //  3h2m
            "%h'h'%s's'",                        //  3h  1s
            "%h'h'",                             //  3h
            "%m'm'%s's'",                        //    2m1s
            "%m'm'",                             //    2m
            "%s's'",                             //      1s
        };

        public static TimeSpan Parse(string original)
        {
            var s = original.ToLower();
            if (TimeSpan.TryParseExact(s, Formats, CultureInfo.InvariantCulture, out var timeSpan))
                return timeSpan;
            else throw new ArgumentException($"{original} is not a timespan value.");
        }
    }
}
