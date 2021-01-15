using Discord.Commands;
using System.Diagnostics;
using System.Threading.Tasks;

namespace RitsukageBot.Discord.Commands
{
    public class Ping : ModuleBase<SocketCommandContext>
    {
        [Command("ping")]
        public async Task Normal()
        {
            var sw = new Stopwatch();
            sw.Start();
            var msg = await ReplyAsync("pinging...");
            sw.Stop();
            await msg.ModifyAsync(x =>
            x.Content = "> ok, it took me " + sw.ElapsedMilliseconds.ToString() + " ms to ping.");
        }
    }
}
