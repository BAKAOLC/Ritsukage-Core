using Discord.Commands;
using Ritsukage.Tools;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Choose : ModuleBase<SocketCommandContext>
    {
        static readonly Rand rnd = new();
        static bool _init = false;

        [Command("choose")]
        public async Task ChooseOne(params string[] choose)
        {
            if (!_init)
            {
                _init = true;
                rnd.Seed(Convert.ToUInt32(DateTime.UtcNow.Millisecond));
            }
            if (choose.Length <= 1)
            {
                await ReplyAsync("参数不合法，请至少给出2项选择项");
                return;
            }
            await ReplyAsync("#抉择：" + choose[rnd.Int(0, choose.Length - 1)]);
        }
    }
}
