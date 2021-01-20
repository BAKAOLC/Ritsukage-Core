using Discord.Commands;
using Ritsukage.Tools;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Time : ModuleBase<SocketCommandContext>
    {
        [Command("时间"), Alias("time")]
        public async Task Normal() => await ReplyAsync(DateTime.Now.ToString("yyyy年MM月dd日 HH时mm分ss秒"));

        [Command("北欧历")]
        public async Task BOL()
        {
            var day = Math.Floor((DateTime.Now - new DateTime(2019, 8, 1, 0, 0, 0)).TotalDays) + 1;
            await ReplyAsync($"当前为北欧历时间：\n2019年08月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        [Command("新北欧历")]
        public async Task NewBOL()
        {
            var day = Math.Floor((DateTime.Now - new DateTime(2020, 6, 1, 0, 0, 0)).TotalDays) + 1;
            await ReplyAsync($"当前为新北欧历时间：\n2020年06月{day,2}日 " + DateTime.Now.ToString("HH时mm分ss秒"));
        }

        static long GetEorzeaHour(long unix) => unix / 175 % 24;
        static long GetEorzeaMinute(long unix) => Convert.ToInt64(60 * ((double)unix / 175 % 1));

        [Command("艾欧泽亚时间"), Alias("et")]
        public async Task ET()
        {
            long unix = DateTimeOffset.FromUnixTimeSeconds(Utils.GetNetworkTimeStamp()).ToUniversalTime().ToUnixTimeSeconds();
            await ReplyAsync($"当前为艾欧泽亚时间：ET {GetEorzeaHour(unix),2:D2}:{GetEorzeaMinute(unix),2:D2}");
        }
    }
}