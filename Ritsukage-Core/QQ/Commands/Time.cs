﻿using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Time
    {
        [Command]
        [CommandDescription("检查bot延迟", "返回消息从qq端接收到bot开始处理所花的时间")]
        public static async void Ping(SoraMessage e) => await e.ReplyToOriginal($"Pong! {(DateTime.Now - e.Time).TotalMilliseconds:F0} ms");

        [Command("时间", "time")]
        [CommandDescription("获取bot服务器当前的时间")]
        public static async void Normal(SoraMessage e)
            => await e.Reply(DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        [Command("北欧历")]
        [CommandDescription("获取bot服务器当前的时间所对应的北欧历时间")]
        public static async void BOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2019, 8, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为北欧历时间：\n2019年08月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("新北欧历")]
        [CommandDescription("获取bot服务器当前的时间所对应的新北欧历时间")]
        public static async void NewBOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2020, 6, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为新北欧历时间：\n2020年06月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("新新北欧历")]
        [CommandDescription("获取bot服务器当前的时间所对应的新新北欧历时间")]
        public static async void NewNewBOL(SoraMessage e)
        {
            var day = Math.Floor((DateTime.Now.Date - new DateTime(2021, 7, 1, 0, 0, 0)).TotalDays) + 1;
            await e.Reply($"当前为新新北欧历时间：\n2021年07月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("高考倒计时")]
        [CommandDescription("获取bot服务器当前的时间到高考开始所差的时间")]
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
        [CommandDescription("测试输入的参数是否为有效的日期参数", "当参数无效时bot不会产生任何反应")]
        [ParameterDescription(1, "日期")]
        public static async void DateTimeTest(SoraMessage e, DateTime dt)
            => await e.Reply(dt.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        [Command("时间测试")]
        [CommandDescription("测试输入的参数是否为有效的时间参数", "当参数无效时bot不会产生任何反应")]
        [ParameterDescription(1, "时间长度")]
        public static async void TimeSpanTest(SoraMessage e, TimeSpan ts)
            => await e.Reply(ts.ToString());
    }
}
