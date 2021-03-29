using Discord.Commands;
using Ritsukage.Library.Data;
using Ritsukage.Library.Service;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class UserInfo : ModuleBase<SocketCommandContext>
    {
        [Command("个人信息")]
        public async Task Info()
        {
            var msg = await ReplyAsync("``数据检索中……``");
            var sb = new StringBuilder();
            sb.AppendLine("```");
            #region Discord
            {
                var dcUser = Context.User;
                sb.AppendLine("[Discord]");
                sb.AppendLine(dcUser.ToString());
                sb.AppendLine("ID：" + dcUser.Id);
            }
            #endregion
            var data = await Database.FindAsync<UserData>(x => x.Discord == Convert.ToInt64(Context.User.Id));
            if (data != null)
            {
                #region QQ
                {
                    sb.AppendLine("[Tencent QQ]");
                    if (data.QQ != 0)
                        sb.AppendLine("账户：" + data.QQ);
                    else
                        sb.AppendLine("未绑定QQ账户");
                }
                #endregion
                #region Bilibili
                {
                    sb.AppendLine("[Bilibili]");
                    if (data.BilibiliCookie != null)
                    {
                        try
                        {
                            var info = new Library.Bilibili.Model.MyUserInfo(data.BilibiliCookie);
                            var birth = string.IsNullOrWhiteSpace(info.Birth) ? "保密" : info.Birth;
                            sb.AppendLine($"{info.Name} (UID:{info.Id}) Lv{info.Level}" + "\n"
                            + $"性别：{info.Sex}  生日：{birth}  关注：{info.Following}  粉丝：{info.Follower}" + "\n"
                            + info.Sign);
                        }
                        catch (Exception e)
                        {
                            sb.AppendLine("获取用户信息时发生错误：");
                            sb.AppendLine(e.GetFormatString());
                        }
                    }
                    else
                        sb.AppendLine("未登录B站账户");
                }
                #endregion
            }
            sb.AppendLine("```");
            await msg.ModifyAsync(x => x.Content = sb.ToString());
        }

        [Command("coins")]
        public async Task Coins()
        {
            var c = await Context.User.GetCoins();
            await ReplyAsync($"当前持有幻币 {c.Total} 枚{Environment.NewLine}(其中 {c.FreeCoins} 枚幻币为当日免费幻币)");
        }
    }
}
