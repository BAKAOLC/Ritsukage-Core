using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class HistoryToday : ModuleBase<SocketCommandContext>
    {
        [Command("历史上的今天"), Alias("historytoday")]
        public async Task Normal()
        {
            if (await Context.User.CheckCoins(3))
            {
                var msg = await ReplyAsync("``数据检索中……``");
                try
                {
                    var h = Library.Roll.Model.HistoryToday.Today();
                    StringBuilder sb = new();
                    sb.AppendLine("[" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                    sb.Append(h[0].ToString());
                    for (var i = 1; i < h.Length; i++)
                        sb.AppendLine().Append(h[i].ToString());
                    await msg.ModifyAsync(x => x.Content = sb.ToString());
                    await Context.User.RemoveCoins(3);
                }
                catch
                {
                    await msg.ModifyAsync(x => x.Content = "数据获取失败，请稍后再试");
                }
            }
            else
                await ReplyAsync("幻币数量不足");
        }
    }
}
