using Sora.EventArgs.SoraEvent;
using System;

namespace Ritsukage.Commands
{
    [CommandGroup]
    public static class Time
    {
        [Command]
        public static async void Ping(BaseSoraEventArgs e)
        {
            if (e is GroupMessageEventArgs gm)
                await gm.Reply("Pong!");
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply("Pong!");
        }

        [Command("时间", "time")]
        public static async void Normal(BaseSoraEventArgs e)
        {
            string msg = DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        static long GetEorzeaHour(long unix) => unix / 175 % 24;
        static long GetEorzeaMinute(long unix) => Convert.ToInt64(60 * ((double)unix / 175 % 1));

        [Command("艾欧泽亚时间", "et")]
        public static async void ET(BaseSoraEventArgs e)
        {
            long unix = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            string msg = $"当前为艾欧泽亚时间：ET {GetEorzeaHour(unix),2:D2}:{GetEorzeaMinute(unix),2:D2}";
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command("北欧历")]
        public static async void BOL(BaseSoraEventArgs e)
        {
            var day = Math.Floor((DateTime.Now - new DateTime(2019, 8, 1, 0, 0, 0)).TotalDays) + 1;
            string msg = $"当前为北欧历时间：\n2019年08月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒");
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command("新北欧历")]
        public static async void NewBOL(BaseSoraEventArgs e)
        {
            var day = Math.Floor((DateTime.Now - new DateTime(2020, 6, 1, 0, 0, 0)).TotalDays) + 1;
            string msg = $"当前为新北欧历时间：\n2020年06月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒");
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command("日期测试")]
        public static async void DateTimeTest(BaseSoraEventArgs e, DateTime dt)
        {
            string msg = dt.ToString("yyyy年MM月dd日 HH时mm分ss秒");
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command("时间测试")]
        public static async void TimeSpanTest(BaseSoraEventArgs e, TimeSpan ts)
        {
            string msg = ts.ToString();
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }
    }
}
