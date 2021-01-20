using Discord.Commands;
using System;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Holiday : ModuleBase<SocketCommandContext>
    {
        [Command("最近节日"), Alias("holiday")]
        public async Task Normal()
        {
            var msg = await ReplyAsync("``数据检索中……``");
            try
            {
                var h = Library.Roll.Model.Holiday.Recent();
                StringBuilder sb = new();
                sb.AppendLine("[" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                sb.Append(h[0].ToString());
                for (var i = 1; i < h.Length; i++)
                    sb.AppendLine().Append(h[i].ToString());
                await msg.ModifyAsync(x => x.Content = sb.ToString());
            }
            catch
            {
                await msg.ModifyAsync(x => x.Content = "数据获取失败，请稍后再试");
            }
        }
    }
}
