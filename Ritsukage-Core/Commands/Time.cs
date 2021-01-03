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
    }
}
