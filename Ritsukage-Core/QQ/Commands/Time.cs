using Ritsukage.Tools;
using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Time
    {
        [Command]
        public static async void Ping(SoraMessage e) => await e.ReplyToOriginal($"Pong! {(DateTime.Now - e.Time).TotalMilliseconds:F0} ms");

        [Command("时间", "time")]
        public static async void Normal(SoraMessage e)
            => await e.Reply(DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        static long GetEorzeaHour(long unix) => unix / 175 % 24;
        static long GetEorzeaMinute(long unix) => Convert.ToInt64(60 * ((double)unix / 175 % 1));

        [Command("艾欧泽亚时间", "et")]
        public static async void ET(SoraMessage e)
        {
            long unix = DateTimeOffset.FromUnixTimeSeconds(Utils.GetNetworkTimeStamp()).ToUniversalTime().ToUnixTimeSeconds();
            await e.Reply($"当前为艾欧泽亚时间：ET {GetEorzeaHour(unix),2:D2}:{GetEorzeaMinute(unix),2:D2}");
        }

        [Command("北欧历")]
        public static async void BOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2019, 8, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为北欧历时间：\n2019年08月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("新北欧历")]
        public static async void NewBOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2020, 6, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为新北欧历时间：\n2020年06月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("新新北欧历")]
        public static async void NewNewBOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2021, 7, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为新新北欧历时间：\n2021年07月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("日期测试")]
        public static async void DateTimeTest(SoraMessage e, DateTime dt)
            => await e.Reply(dt.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        [Command("时间测试")]
        public static async void TimeSpanTest(SoraMessage e, TimeSpan ts)
            => await e.Reply(ts.ToString());
    }
}
