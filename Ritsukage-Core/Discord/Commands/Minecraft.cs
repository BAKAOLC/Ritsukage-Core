using Discord.Commands;
using Ritsukage.Library.Data;
using Ritsukage.Tools.Console;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Minecraft : ModuleBase<SocketCommandContext>
    {
        [Command("订阅minecraft更新")]
        public async Task AddVersionListener()
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "minecraft version"
                && x.Target == "java" && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await ReplyAsync("本频道已订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.InsertAsync(new SubscribeList()
                {
                    Platform = "discord channel",
                    Type = "minecraft version",
                    Target = "java",
                    Listener = Context.Channel.Id.ToString()
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致添加失败，请稍后重试");
                });
            }
        }

        [Command("取消订阅minecraft更新")]
        public async Task RemoveVersionListener()
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "minecraft version"
                && x.Target == "java" && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await ReplyAsync("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已移除");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致移除失败，请稍后重试");
                });
            }
        }
    }
}
