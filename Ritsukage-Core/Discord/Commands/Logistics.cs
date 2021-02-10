using Discord.Commands;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Logistics : ModuleBase<SocketCommandContext>
    {
        [Command("快递详情")]
        public async Task Normal(string id)
        {
            if (await Context.User.CheckCoins(15))
            {
                await Context.Message.DeleteAsync();
                var msg = await ReplyAsync("``数据检索中……``");
                try
                {
                    var h = Library.Roll.Model.Logistics.Get(id);
                    var dm = await Context.User.GetOrCreateDMChannelAsync();
                    await dm.SendMessageAsync(h.GetFullString());
                    await msg.ModifyAsync(x => x.Content = "数据获取成功，请前往私聊查看");
                    await Context.User.RemoveCoins(15);
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
