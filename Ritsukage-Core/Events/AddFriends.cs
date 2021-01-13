using Sora.EventArgs.SoraEvent;
using Sora.Tool;

namespace Ritsukage.Events
{
    [EventGroup]
    public static class AddFriends
    {
        [Event(typeof(FriendRequestEventArgs), 0)]
        public static async void Accept(object sender, FriendRequestEventArgs args)
        {
            var info = (await args.Sender.GetUserInfo()).userInfo;
            ConsoleLog.Info(args.EventName, $"[{args.LoginUid}] 接收到来自 {info.Nick}({info.UserId}) 的好友添加请求(id:{args.RequsetFlag})");
            await args.SoraApi.SetFriendAddRequest(args.RequsetFlag, true);
            ConsoleLog.Info(args.EventName, $"[{args.LoginUid}] 请求id: {args.RequsetFlag} 的好友添加请求已自动同意");
        }
    }
}
