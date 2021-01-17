using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili;
using Ritsukage.Library.Data;
using Sora.Entities.CQCodes;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup, OnlyForUser(2565128043, 1418780411)]
    public static class RitsukageLive
    {
        const long main = 2565128043;
        const int roomid = 3241620;

        [Command("startlive")]
        public static async void StartLive(SoraMessage e)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }
            var result = JObject.Parse(BiliLive.StartLive(roomid, 235, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
            else
            {
                await e.AutoAtReply("开播成功");
                await e.SendPrivateMessage($"rtmp地址: {(string)result["data"]["rtmp"]["addr"]}\n推流码: {(string)result["data"]["rtmp"]["code"]}");
            }
        }

        [Command("stoplive")]
        public static async void StopLive(SoraMessage e)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }
            var result = JObject.Parse(BiliLive.StopLive(roomid, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
            else
                await e.AutoAtReply("已停止直播");
        }

        [Command("changearea")]
        public static async void SetLiveArea(SoraMessage e, int area)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }
            var result = JObject.Parse(BiliLive.UpdateLiveArea(roomid, area, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
            else
                await e.AutoAtReply("已成功更换直播分区");
        }

        [Command("changetitle")]
        public static async void SetLiveTitle(SoraMessage e, string title)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }
            var result = JObject.Parse(BiliLive.UpdateLiveTitle(roomid, title, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
            else
                await e.AutoAtReply("已成功更换直播分区");
        }

        [Command("livestatus")]
        public static async void LiveStatus(SoraMessage e)
        {
            var room = Library.Bilibili.Model.LiveRoom.Get(roomid);
            await e.Reply(CQCode.CQImage(string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl), "\n" + "标题：" + room.Title + "\n"
                + "直播间ID：" + room.Id + "\n"
                + "当前分区：" + room.ParentAreaName + "·" + room.AreaName + "\n"
                + room.LiveStatus switch
                {
                    Library.Bilibili.Model.LiveStatus.Live => "当前正在直播",
                    Library.Bilibili.Model.LiveStatus.Round => "当前正在轮播",
                    _ => "当前未开播"
                } + "\n"
                + "当前人气值：" + room.Online + "\n"
                + room.Url);
        }
    }
}
