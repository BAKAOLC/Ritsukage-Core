using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili;
using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Sora.Entities;
using Sora.Entities.CQCodes;
using Sora.EventArgs.SoraEvent;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Commands
{
    [CommandGroup]
    public static class Bilibili
    {
        [Command]
        [CommandArgumentErrorCallback("AVBVConverterFallback")]
        public static async void AV2BV(BaseSoraEventArgs e, long av)
        {
            string msg;
            try
            {
                msg = $"[Bilibili][AV→BV] {av} → {BilibiliAVBVConverter.ToBV(av)}";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command]
        [CommandArgumentErrorCallback("AVBVConverterFallback")]
        public static async void BV2AV(BaseSoraEventArgs e, string bv)
        {
            string msg;
            try
            {
                msg = $"[Bilibili][BV→AV] {bv} → {BilibiliAVBVConverter.ToAV(bv)}";
            }
            catch (Exception ex)
            {
                msg = ex.Message;
            }
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }

        [Command]
        public static async void Login(BaseSoraEventArgs e)
        {
            User target = null;
            bool inGroup = false;
            Action<string> Tip = null;

            if (e is GroupMessageEventArgs gm)
            {
                inGroup = true;
                target = gm.Sender;
                Tip = async (msg) => await gm.Reply(target.CQCodeAt(), msg);
            }
            else if (e is PrivateMessageEventArgs pm)
            {
                target = pm.Sender;
                Tip = async (msg) => await pm.Reply(msg);
            }

            if (target == null) return;

            await Task.Run(() => Library.Bilibili.Bilibili.QRCodeLoginRequest(
                async (bitmap) =>
                {
                    if (inGroup)
                        Tip("登录任务已建立，请前往私聊等待登录二维码的发送");
                    else
                        Tip("登录任务已建立，请等待登录二维码的发送");
                    var qr = new MemoryImage(bitmap);
                    var path = qr.ToBase64File();
                    await target.SendPrivateMessage(CQCode.CQImage(path), "\n请使用Bilibili客户端扫码登录");
                },
                async () => await target.SendPrivateMessage("检测到扫描事件，请在客户端中确认登录"),
                async (cookie) =>
                {
                    if (int.TryParse(cookie.Split(";").Where(x => x.StartsWith("DedeUserID=")).First()[11..], out var id))
                    {
                        await target.SendPrivateMessage("登录成功\n数据储存中……");
                        var t = Database.Data.Table<UserData>();
                        var data = await t.Where(x => x.QQ == target.Id).FirstOrDefaultAsync();
                        if (data != null)
                        {
                            data.Bilibili = id;
                            data.BilibiliCookie = cookie;
                            await Database.Data.UpdateAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await target.SendPrivateMessage("记录数据已更新");
                                else
                                    await target.SendPrivateMessage("记录数据未能成功更新");
                            });
                        }
                        else
                        {
                            data = new()
                            {
                                QQ = target.Id,
                                Bilibili = id,
                                BilibiliCookie = cookie
                            };
                            await Database.Data.InsertAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await target.SendPrivateMessage("记录数据已添加");
                                else
                                    await target.SendPrivateMessage("记录数据未能成功添加");
                            });
                        }
                    }
                    else
                        await target.SendPrivateMessage("登录失败\n未能匹配到用户UID");
                },
                async (errMsg) => await target.SendPrivateMessage("登录失败\n" + errMsg)));
        }

        [Command("开启直播")]
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
            var data = await t.Where(x => x.QQ == user.Id).FirstOrDefaultAsync();

            if (data == null)
            {
                Tip("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }

            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                Tip("用户直播间数据获取失败，请稍后重试");
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

        [Command("关闭直播")]
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
            var data = await t.Where(x => x.QQ == user.Id).FirstOrDefaultAsync();

            if (data == null)
            {
                Tip("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }

            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                Tip("用户直播间数据获取失败，请稍后重试");
                return;
            }

            var result = JObject.Parse(BiliLive.StopLive(roomid, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已停止直播");
        }

        [Command("设置直播分区")]
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
            var data = await t.Where(x => x.QQ == user.Id).FirstOrDefaultAsync();

            if (data == null)
            {
                Tip("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }

            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                Tip("用户直播间数据获取失败，请稍后重试");
                return;
            }

            var result = JObject.Parse(BiliLive.UpdateLiveArea(roomid, area, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已成功更换直播分区");
        }

        [Command("设置直播标题")]
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
            var data = await t.Where(x => x.QQ == user.Id).FirstOrDefaultAsync();

            if (data == null)
            {
                Tip("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }

            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                Tip("用户直播间数据获取失败，请稍后重试");
                return;
            }

            var result = JObject.Parse(BiliLive.UpdateLiveTitle(roomid, title, data.BilibiliCookie));
            if (!string.IsNullOrEmpty((string)result["message"]))
                Tip("服务器返回消息：" + (string)result["message"]);
            else
                Tip("已成功更换直播分区");
        }

        public static async void AVBVConverterFallback(BaseSoraEventArgs e, Exception ex = null)
        {
            string msg;
            if (ex is ArgumentOutOfRangeException)
                msg = $"未填写参数，请重新输入";
            else
                msg = $"错误的参数指定，请检查参数是否正确({ex.Message})";
            if (e is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (e is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
        }
    }
}
