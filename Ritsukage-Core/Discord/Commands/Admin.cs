using Discord.Commands;
using Ritsukage.Library.Data;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Admin : ModuleBase<SocketCommandContext>
    {
        [Command("firstcommingrole")]
        public async Task Normal(ulong id)
        {
            var user = Context.Guild.GetUser(Context.User.Id);
            if (user.GuildPermissions.Administrator || user.GuildPermissions.ManageRoles)
            {
                var data = await Database.FindAsync<DiscordGuildSetting>(x => x.Guild == Convert.ToInt64(Context.Guild.Id));
                if (data != null)
                {
                    data.FirstCommingRole = Convert.ToInt64(id);
                    await Database.UpdateAsync(data).ContinueWith(async x =>
                    {
                        if (x.Result > 0)
                            await ReplyAsync(":white_check_mark: 设置成功");
                        else if (x.IsFaulted && x.Exception != null)
                            await ReplyAsync(":x: " + new StringBuilder()
                                .AppendLine("因异常导致设置失败，错误信息：")
                                .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                                .ToString());
                        else
                            await ReplyAsync(":x: 因未知原因导致设置失败，请稍后重试");
                    });
                }
                data = new()
                {
                    Guild = Convert.ToInt64(Context.Guild.Id),
                    FirstCommingRole = Convert.ToInt64(id)
                };
                await Database.InsertAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync(":white_check_mark: 设置成功");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(":x: " + new StringBuilder()
                            .AppendLine("因异常导致设置失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync(":x: 因未知原因导致设置失败，请稍后重试");
                });
            }
        }
    }
}
