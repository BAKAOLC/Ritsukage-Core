using Ritsukage.Tools;
using System;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class HistoryToday
    {
        [Command("历史上的今天", "historytoday"), NeedCoins(3), ExecutesCooldownAttribute("historytoday", 120, true)]
        public static async void Normal(SoraMessage e)
        {
            try
            {
                var h = Library.Roll.Model.HistoryToday.Today();
                StringBuilder sb = new();
                sb.AppendLine("[" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                sb.AppendJoin(Environment.NewLine, h);
                if (h.Length > 30)
                {
                    var bin = UbuntuPastebin.Paste(sb.ToString(), "text", "Hitsory Today");
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("数据过多，请前往以下链接查看")
                        .Append(bin).ToString());
                }
                else
                    await e.Reply(sb.ToString());
                await e.RemoveCoins(3);
                await e.UpdateGroupCooldown("historytoday");
            }
            catch
            {
                await e.Reply("数据获取失败，请稍后再试");
            }
        }
    }
}
