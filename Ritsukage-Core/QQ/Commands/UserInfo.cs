using Ritsukage.Library.Data;
using System;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("User")]
    public static class UserInfo
    {
        [Command("个人信息")]
        public static async void Info(SoraMessage e)
        {
            var sb = new StringBuilder();
            #region QQ
            {
                sb.AppendLine("[Tencent QQ]");
                if (e.IsGroupMessage)
                    sb.Append($"{e.GroupSenderInfo.Card}({e.Sender.Id})");
                else
                    sb.Append($"{e.PrivateSenderInfo.Nick}({e.Sender.Id})");
            }
            #endregion
            var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id);
            if (data != null)
            {
                #region Discord
                {
                    sb.AppendLine();
                    sb.AppendLine("[Discord]");
                    if (data.Discord != 0)
                        sb.Append("ID：" + data.Discord);
                    else
                        sb.Append("未绑定Discord账户");
                }
                #endregion
            }
            await e.Reply(sb.ToString());
        }

        [Command("coins")]
        public static async void Coins(SoraMessage e)
        {
            var c = await e.GetCoins();
            await e.ReplyToOriginal($"当前持有幻币 {c.Total} 枚{Environment.NewLine}(其中 {c.FreeCoins} 枚幻币为当日免费幻币)");
        }
    }
}
