using System;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class HistoryToday
    {
        [Command("历史上的今天", "historytoday"), NeedCoins(3)]
        public static async void Normal(SoraMessage e)
        {
            try
            {
                var h = Library.Roll.Model.HistoryToday.Today();
                StringBuilder sb = new();
                sb.AppendLine("[" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                sb.Append(h[0].ToString());
                for (var i = 1; i < h.Length && i < 50; i++)
                    sb.AppendLine().Append(h[i].ToString());
                if (h.Length >= 50)
                    sb.AppendLine().Append("<更多条目已被过滤，如果需要请自行搜索>");
                await e.Reply(sb.ToString());
                await e.RemoveCoins(3);
            }
            catch
            {
                await e.Reply("数据获取失败，请稍后再试");
            }
        }
    }
}
