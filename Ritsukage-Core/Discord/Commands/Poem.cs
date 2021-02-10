using Discord.Commands;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Poem : ModuleBase<SocketCommandContext>
    {
        [Command("飞花令")]
        public async Task FHL(string _char)
        {
            if (await Context.User.CheckCoins(2))
            {
                var msg = await ReplyAsync("``数据检索中……``");
                if (_char.Length != 1)
                {
                    await msg.ModifyAsync(x => x.Content = "参数错误，请检查后重试");
                    return;
                }
                var result = await Tools.Poem.Search(_char);
                if (result.Count <= 0)
                {
                    await msg.ModifyAsync(x => x.Content = "没有搜索到任何结果，请检查后重试");
                    return;
                }
                if (result.Count <= 5)
                    await msg.ModifyAsync(x => x.Content = $"带有「{_char}」字的诗句有：" + Environment.NewLine + string.Join(Environment.NewLine, result));
                else
                {
                    var s = new string[5];
                    var rand = new Tools.Rand();
                    for (var i = 0; i < 5; i++)
                    {
                        var id = rand.Int(1, result.Count) - 1;
                        s[i] = result[id];
                        result.RemoveAt(id);
                    }
                    await msg.ModifyAsync(x => x.Content = $"带有「{_char}」字的诗句有(随机选取5句)："
                    + Environment.NewLine + string.Join(Environment.NewLine, s));
                }
                await Context.User.RemoveCoins(2);
            }
            else
                await ReplyAsync("幻币数量不足");
        }
    }
}