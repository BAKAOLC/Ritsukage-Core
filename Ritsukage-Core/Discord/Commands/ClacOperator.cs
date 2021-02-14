using Discord.Commands;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class ClacOperator : ModuleBase<SocketCommandContext>
    {
        [Command("calc")]
        public async Task CalcMath(string expr)
        {
            try
            {
                double result = Tools.CalcTool.GetExprValue(expr.Replace(" ", ""));
                await ReplyAsync($"{expr} = {result}");
            }
            catch
            {
                await ReplyAsync("操作失败");
            }
        }
    }
}