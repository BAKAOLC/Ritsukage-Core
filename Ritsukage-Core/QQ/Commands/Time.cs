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

        [Command("高考倒计时")]
        public static async void Examination(SoraMessage e)
        {
            var day = Math.Floor((new DateTime(2021, 6, 7, 0, 0, 0) - DateTime.Now.Date).TotalDays);
            if (day > 3)
                await e.Reply($"距离高考还有 {day} 天");
            else if (day == 3)
                await e.Reply("距离高考还有 3 天，冲冲冲");
            else if (day == 2)
                await e.Reply("距离高考还有 2 天，加油啊");
            else if (day == 1)
                await e.Reply("明天就开始高考啦，祝你们好运！");
            else if (day < 1 && day > -4)
                await e.Reply("已经在高考期间啦，考个好成绩回来哦！");
            else
                await e.Reply("考完啦，放松一下吧");
        }

        [Command("日期测试")]
        public static async void DateTimeTest(SoraMessage e, DateTime dt)
            => await e.Reply(dt.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        [Command("时间测试")]
        public static async void TimeSpanTest(SoraMessage e, TimeSpan ts)
            => await e.Reply(ts.ToString());
    }
}
