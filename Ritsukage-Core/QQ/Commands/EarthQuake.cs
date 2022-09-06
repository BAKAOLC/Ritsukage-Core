using Ritsukage.Library.Data;
using Ritsukage.Tools.Console;
using Sora.Enumeration.EventParamsType;
using System;
using System.Linq;
using System.Text;
using static Ritsukage.Library.EarthQuake.EarthQuake;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Earth Quake")]
    public static class EarthQuake
    {
        [Command("获取最近地震事件")]
        public static async void GetEarthQuakeList(SoraMessage e)
        {
            try
            {
                var data = GetData();
                if (data == null || data.Count == 0)
                {
                    await e.ReplyToOriginal("未获取到任何数据");
                }
                else
                {
                    var sb = new StringBuilder();
                    sb.Append("[Earth Quake]");
                    foreach (var m in data.Select(x => x.ToString()))
                        sb.AppendLine().Append(m);
                    await e.Reply(sb.ToString());
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Earth Quake", ConsoleLog.ErrorLogBuilder(ex));
                await e.ReplyToOriginal("获取数据时发生错误");
            }
        }

        [Command("订阅地震事件"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddEarthQuakeListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "earth quake"
                && x.Target == "cn"
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data != null)
            {
                await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "earth quake",
                Target = "cn",
                Listener = e.SourceGroup.Id.ToString()
            }).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.ReplyToOriginal("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                else if (x.IsFaulted && x.Exception != null)
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.ReplyToOriginal("订阅项目因未知原因导致添加失败，请稍后重试");
            });
        }

        [Command("取消订阅地震事件"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveEarthQuakeListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "earth quake"
                && x.Target == "cn"
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data == null)
            {
                await e.ReplyToOriginal("本群未订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.DeleteAsync(data).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.ReplyToOriginal("订阅项目已移除");
                else if (x.IsFaulted && x.Exception != null)
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.AutoAtReply("订阅项目因未知原因导致移除失败，请稍后重试");
            });
        }
    }
}
