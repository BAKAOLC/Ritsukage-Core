using Ritsukage.Library.Data;
using Ritsukage.Tools.Console;
using Sora.Enumeration.EventParamsType;
using System.Linq;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class Minecraft
    {
        [Command("订阅minecraft更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddVersionListener(SoraMessage e)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "minecraft version"
                && x.Target == "java" && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                    return;
                }
            }
            await Database.Data.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "minecraft version",
                Target = "java",
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

        [Command("取消订阅minecraft更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveVersionListener(SoraMessage e)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "minecraft version"
                && x.Target == "java" && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await e.ReplyToOriginal("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
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
            else
                await e.ReplyToOriginal("本群未订阅该目标，请检查输入是否正确");
        }
    }
}
