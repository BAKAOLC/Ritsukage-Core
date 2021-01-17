using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.Entities.CQCodes;
using Sora.Enumeration.EventParamsType;
using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class Bilibili
    {
        [Command]
        public static async void Login(SoraMessage e)
        {
            await Task.Run(() => Library.Bilibili.Bilibili.QRCodeLoginRequest(
                async (bitmap) =>
                {
                    if (e.IsGroupMessage)
                        await e.AutoAtReply("登录任务已建立，请前往私聊等待登录二维码的发送");
                    else
                        await e.Reply("登录任务已建立，请等待登录二维码的发送");
                    var qr = new MemoryImage(bitmap);
                    var path = qr.ToBase64File();
                    await e.SendPrivateMessage(CQCode.CQImage(path), "\n请使用Bilibili客户端扫码登录");
                },
                async () => await e.SendPrivateMessage("检测到扫描事件，请在客户端中确认登录"),
                async (cookie) =>
                {
                    if (int.TryParse(cookie.Split(";").Where(x => x.StartsWith("DedeUserID=")).First()[11..], out var id))
                    {
                        await e.SendPrivateMessage("登录成功\n数据储存中……");
                        var t = Database.Data.Table<UserData>();
                        var data = await t.Where(x => x.QQ == e.Sender.Id || x.Bilibili == id).FirstOrDefaultAsync();
                        if (data != null)
                        {
                            data.QQ = e.Sender.Id;
                            data.Bilibili = id;
                            data.BilibiliCookie = cookie;
                            await Database.Data.UpdateAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await e.SendPrivateMessage("记录数据已更新");
                                else if (x.IsFaulted && x.Exception != null)
                                    await e.SendPrivateMessage(new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await e.SendPrivateMessage("记录数据因未知原因导致成功失败，请稍后重试");
                            });
                        }
                        else
                        {
                            data = new()
                            {
                                QQ = e.Sender.Id,
                                Bilibili = id,
                                BilibiliCookie = cookie
                            };
                            await Database.Data.InsertAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await e.SendPrivateMessage("记录数据已添加");
                                else if (x.IsFaulted && x.Exception != null)
                                    await e.SendPrivateMessage(new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await e.SendPrivateMessage("记录数据因未知原因导致成功失败，请稍后重试");
                            });
                        }
                    }
                    else
                        await e.SendPrivateMessage("登录失败\n未能匹配到用户UID");
                },
                async (errMsg) => await e.SendPrivateMessage("登录失败\n" + errMsg)));
        }

        [Command("获取b站用户信息")]
        public static async void UserInfo(SoraMessage e, int uid)
        {
            User user = null;
            try
            {
                user = User.Get(uid);
            }
            catch
            {
            }
            if (user != null)
            {
                await e.Reply(CQCode.CQImage(user.FaceUrl), new StringBuilder()
                    .AppendLine()
                    .AppendLine(user.BaseToString())
                    .ToString());
            }
            else
                await e.Reply($"[Bilibili] 用户{uid}信息获取失败");
        }

        [Command("获取b站直播间信息")]
        public static async void LiveRoomInfo(SoraMessage e, int roomid)
        {
            LiveRoom room = null;
            try
            {
                room = LiveRoom.Get(roomid);
            }
            catch
            {
            }
            if (room != null)
            {
                string cover = string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl;
                await e.Reply(CQCode.CQImage(cover), new StringBuilder()
                    .AppendLine()
                    .AppendLine(room.BaseToString())
                    .ToString());
            }
            else
                await e.Reply($"[Bilibili Live] 直播间{roomid}信息获取失败");
        }

        [Command("获取b站视频信息")]
        public static async void VideoInfo(SoraMessage e, string bv)
        {
            Video video = null;
            try
            {
                if (long.TryParse(bv, out var av))
                    video = Video.Get(av);
                else
                    video = Video.Get(bv);
            }
            catch
            {
            }
            if (video != null)
            {
                await e.Reply(CQCode.CQImage(video.PicUrl), new StringBuilder()
                    .AppendLine()
                    .AppendLine(video.BaseToString())
                    .ToString());
            }
            else
                await e.Reply($"[Bilibili] 视频{bv}信息获取失败");
        }

        [Command("获取b站动态信息")]
        public static async void DynamicInfo(SoraMessage e, ulong id)
        {
            Dynamic dynamic = null;
            try
            {
                dynamic = Dynamic.Get(id);
            }
            catch
            {
            }
            if (dynamic != null)
            {
                ArrayList msg = new();
                foreach (var pic in dynamic.Pictures)
                {
                    msg.Add(CQCode.CQImage(pic));
                    msg.Add(Environment.NewLine);
                }
                if (dynamic.Pictures.Length > 4)
                    await e.Reply("该动态含有超过4张图像存在，任务时长可能较长，请耐心等候");
                msg.Add(dynamic.BaseToString());
                await e.Reply(msg.ToArray());
            }
            else
                await e.Reply($"[Bilibili] 动态{id}信息获取失败");
        }

        [Command("开启直播")]
        public static async void StartLive(SoraMessage e)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == e.Sender.Id).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.AutoAtReply("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.StartLive(roomid, 235, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
                else
                {
                    await e.AutoAtReply("开播成功");
                    await e.SendPrivateMessage($"rtmp地址: {(string)result["data"]["rtmp"]["addr"]}\n推流码: {(string)result["data"]["rtmp"]["code"]}");
                }
            }
            catch
            {
                await e.AutoAtReply("操作失败");
            }
        }

        [Command("关闭直播")]
        public static async void StopLive(SoraMessage e)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == e.Sender.Id).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.AutoAtReply("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.StopLive(roomid, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
                else
                    await e.AutoAtReply("已停止直播");
            }
            catch
            {
                await e.AutoAtReply("操作失败");
            }
        }

        [Command("设置直播分区")]
        public static async void SetLiveArea(SoraMessage e, int area)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == e.Sender.Id).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.AutoAtReply("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.UpdateLiveArea(roomid, area, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
                else
                    await e.AutoAtReply("已成功更换直播分区");
            }
            catch
            {
                await e.AutoAtReply("操作失败");
            }
        }

        [Command("设置直播标题")]
        public static async void SetLiveTitle(SoraMessage e, string title)
        {
            var t = Database.Data.Table<UserData>();
            var data = await t.Where(x => x.QQ == e.Sender.Id).FirstOrDefaultAsync();
            if (data == null)
            {
                await e.AutoAtReply("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.AutoAtReply("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.UpdateLiveTitle(roomid, title, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.AutoAtReply("服务器返回消息：" + (string)result["message"]);
                else
                    await e.AutoAtReply("已成功更换直播分区");
            }
            catch
            {
                await e.AutoAtReply("操作失败");
            }
        }

        [Command("订阅b站直播"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddLiveListener(SoraMessage e, int roomid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "bilibili live"
                && x.Target == roomid.ToString() && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await e.AutoAtReply("本群已订阅该目标，请检查输入是否正确");
                    return;
                }
            }
            await Database.Data.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "bilibili live",
                Target = roomid.ToString(),
                Listener = e.SourceGroup.Id.ToString()
            }).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.AutoAtReply("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                else if (x.IsFaulted && x.Exception != null)
                    await e.AutoAtReply(new StringBuilder()
                        .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                        .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.AutoAtReply("订阅项目因未知原因导致添加失败，请稍后重试");
            });
        }

        [Command("取消订阅b站直播"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveLiveListener(SoraMessage e, int roomid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "bilibili live"
                && x.Target == roomid.ToString() && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await e.AutoAtReply("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.AutoAtReply("订阅项目已移除");
                    else if (x.IsFaulted && x.Exception != null)
                        await e.AutoAtReply(new StringBuilder()
                            .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                            .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await e.AutoAtReply("订阅项目因未知原因导致移除失败，请稍后重试");
                });
            }
            else
                await e.AutoAtReply("本群未订阅该目标，请检查输入是否正确");
        }

        [Command("订阅b站动态"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddDynamicListener(SoraMessage e, int userid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString() && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await e.AutoAtReply("本群已订阅该目标，请检查输入是否正确");
                    return;
                }
            }
            await Database.Data.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "bilibili dynamic",
                Target = userid.ToString(),
                Listener = e.SourceGroup.Id.ToString()
            }).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.AutoAtReply("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                else if (x.IsFaulted && x.Exception != null)
                    await e.AutoAtReply(new StringBuilder()
                        .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                        .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.AutoAtReply("订阅项目因未知原因导致添加失败，请稍后重试");
            });
        }

        [Command("取消订阅b站动态"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveDynamicListener(SoraMessage e, int userid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "qq group" && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString() && x.Listener == e.SourceGroup.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await e.AutoAtReply("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.AutoAtReply("订阅项目已移除");
                    else if (x.IsFaulted && x.Exception != null)
                        await e.AutoAtReply(new StringBuilder()
                            .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                            .AppendLine(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await e.AutoAtReply("订阅项目因未知原因导致移除失败，请稍后重试");
                });
            }
            else
                await e.AutoAtReply("本群未订阅该目标，请检查输入是否正确");
        }

        [Command]
        public static async void AV2BV(SoraMessage e, long av)
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
            await e.Reply(msg);
        }
        [Command]
        public static async void AV2BV(SoraMessage e)
            => await e.AutoAtReply("参数错误，请重新输入");

        [Command]
        public static async void BV2AV(SoraMessage e, string bv)
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
            await e.Reply(msg);
        }
        [Command]
        public static async void BV2AV(SoraMessage e)
            => await e.AutoAtReply("参数错误，请重新输入");
    }
}
