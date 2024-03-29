﻿using System;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class Holiday
    {
        [Command("最近节日", "holiday"), ExecutesCooldown("holiday", 120, true)]
        [CommandDescription("获取最近节日的相关信息", "API接口来自 https://www.mxnzp.com")]
        public static async void Normal(SoraMessage e)
        {
            try
            {
                var h = Library.Roll.Model.Holiday.Recent();
                StringBuilder sb = new();
                sb.AppendLine("[" + DateTime.Today.ToString("yyyy-MM-dd") + "]");
                sb.Append(h[0].ToString());
                for (var i = 1; i < h.Length; i++)
                    sb.AppendLine().Append(h[i].ToString());
                await e.Reply(sb.ToString());
                await e.UpdateGroupCooldown("holiday");
            }
            catch
            {
                await e.Reply("数据获取失败，请稍后再试");
            }
        }
    }
}