using Discord.Commands;
using Ritsukage.Tools;
using System;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class HHSH : ModuleBase<SocketCommandContext>
    {
        [Command("guess")]
        public async Task Normal(string origin)
        {
            try
            {
                var trans = NBNHHSH.Get(origin);
                if (trans.Length > 0)
                {
                    await ReplyAsync($"{origin} 的意思可能为" + Environment.NewLine + string.Join(" ", trans));
                    return;
                }
            }
            catch
            { }
                await ReplyAsync($"{origin} 未能成功获取到猜测内容");
        }
    }
}