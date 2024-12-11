using Sora.EventArgs.SoraEvent;
using Ritsukage.Tools.Console;
using Sora.Enumeration.ApiType;

namespace Ritsukage.QQ.Events
{
    [EventGroup]
    public static class AddFriends
    {
        [Event(typeof(FriendRequestEventArgs))]
        public static async void Accept(object sender, FriendRequestEventArgs args)
        {
            var (apiStatus, info, qid) = await args.SoraApi.GetUserInfo(args.Sender.Id).ConfigureAwait(false);
            if (apiStatus.RetCode != ApiStatusType.Ok)
            {
                ConsoleLog.Error(args.EventName,
                    $"[{args.LoginUid}] 获取用户信息失败: {apiStatus.ApiMessage}");
                return;
            }

            ConsoleLog.Info(args.EventName,
                $"[{args.LoginUid}] 接收到来自 {info.Nick}({info.UserId}) 的好友添加请求(id:{args.RequestFlag})");
            await args.SoraApi.SetFriendAddRequest(args.RequestFlag, true).ConfigureAwait(false);
            ConsoleLog.Info(args.EventName, $"[{args.LoginUid}] 请求id: {args.RequestFlag} 的好友添加请求已自动同意");
        }
    }
}