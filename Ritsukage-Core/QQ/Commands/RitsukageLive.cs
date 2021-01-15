using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili;
using Ritsukage.Library.Data;
using Sora.Entities;
using Sora.Entities.CQCodes;
using Sora.EventArgs.SoraEvent;
using System;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup, OnlyForUser(2565128043, 1418780411)]
    public static class RitsukageLive
    {
        const long main = 2565128043;
        const int roomid = 3241620;

        [Command("startlive")]
        public static async void StartLive(BaseSoraEventArgs e)
        {
            User user = null;
            Action<string> Tip = null;
            if (e is GroupMessageEventArgs gm)
            {
                user = gm.Sender;
                Tip = async (msg) => await gm.Reply(user.CQCodeAt(), msg);
            }
            else if (e is PrivateMessageEventArgs pm)
            {
                user = pm.Sender;
                Tip = async (msg) => await pm.Reply(msg);
            }
            if (user == null) return;

            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                Tip("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }

            var result = JObject.Parse(BiliLive.StartLive(roomid, 235, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
            {
                Tip("开播成功");
                await user.SendPrivateMessage($"rtmp地址: {(string)result["data"]["rtmp"]["addr"]}\n推流码: {(string)result["data"]["rtmp"]["code"]}");
            }
        }

        [Command("stoplive")]
        public static async void StopLive(BaseSoraEventArgs e)
        {
            User user = null;
            Action<string> Tip = null;
            if (e is GroupMessageEventArgs gm)
            {
                user = gm.Sender;
                Tip = async (msg) => await gm.Reply(user.CQCodeAt(), msg);
            }
            else if (e is PrivateMessageEventArgs pm)
            {
                user = pm.Sender;
                Tip = async (msg) => await pm.Reply(msg);
            }
            if (user == null) return;

            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                Tip("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }

            var result = JObject.Parse(BiliLive.StopLive(roomid, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已停止直播");
        }

        [Command("changearea")]
        public static async void SetLiveArea(BaseSoraEventArgs e, int area)
        {
            User user = null;
            Action<string> Tip = null;
            if (e is GroupMessageEventArgs gm)
            {
                user = gm.Sender;
                Tip = async (msg) => await gm.Reply(user.CQCodeAt(), msg);
            }
            else if (e is PrivateMessageEventArgs pm)
            {
                user = pm.Sender;
                Tip = async (msg) => await pm.Reply(msg);
            }
            if (user == null) return;

            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                Tip("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }

            var result = JObject.Parse(BiliLive.UpdateLiveArea(roomid, area, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已成功更换直播分区");
        }

        [Command("changetitle")]
        public static async void SetLiveTitle(BaseSoraEventArgs e, string title)
        {
            User user = null;
            Action<string> Tip = null;
            if (e is GroupMessageEventArgs gm)
            {
                user = gm.Sender;
                Tip = async (msg) => await gm.Reply(user.CQCodeAt(), msg);
            }
            else if (e is PrivateMessageEventArgs pm)
            {
                user = pm.Sender;
                Tip = async (msg) => await pm.Reply(msg);
            }
            if (user == null) return;

            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == main).FirstOrDefaultAsync();
            if (data == null)
            {
                Tip("数据库中没有主用户信息，无法执行该指令，请提醒主用户通过 +login 进行账户登录");
                return;
            }

            var result = JObject.Parse(BiliLive.UpdateLiveTitle(roomid, title, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已成功更换直播分区");
        }

        [Command("livestatus")]
        public static async void LiveStatus(BaseSoraEventArgs e)
        {
            User user = null;
            GroupMessageEventArgs gm = null;
            PrivateMessageEventArgs pm = null;
            if (e is GroupMessageEventArgs _gm)
            {
                user = _gm.Sender;
                gm = _gm;
            }
            else if (e is PrivateMessageEventArgs _pm)
            {
                user = _pm.Sender;
                pm = _pm;
            }
            if (user == null) return;

            var room = Library.Bilibili.Model.LiveRoom.Get(roomid);
            var msg = new object[]
            {
                CQCode.CQImage(string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl), "\n" + "标题：" + room.Title + "\n"
                + "直播间ID：" + room.Id + "\n"
                + "当前分区：" + room.ParentAreaName + "·" + room.AreaName + "\n"
                + room.LiveStatus switch
                {
                    Library.Bilibili.Model.LiveStatus.Live => "当前正在直播",
                    Library.Bilibili.Model.LiveStatus.Round => "当前正在轮播",
                    _ => "当前未开播"
                } + "\n"
                + "当前人气值：" + room.Online + "\n"
                + room.Url
            };
            if (gm != null)
                await gm.Reply(msg);
            else if (pm != null)
                await pm.Reply(msg);
        }
    }
}
