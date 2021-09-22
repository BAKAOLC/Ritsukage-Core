using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp.Formats.Png;
using Sora.Entities.CQCodes;
using Sora.Enumeration.EventParamsType;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Bilibili")]
    public static class Bilibili
    {
        [Command]
        [CommandDescription("登录b站账户", "使用二维码扫描登录", "用于部分需要用户登录才能执行的操作")]
        public static async void Login(SoraMessage e)
        {
            await Task.Run(() => Library.Bilibili.Bilibili.QRCodeLoginRequest(
                async (bitmap) =>
                {
                    if (e.IsGroupMessage)
                        await e.ReplyToOriginal("登录任务已建立，请前往私聊等待登录二维码的发送");
                    else
                        await e.ReplyToOriginal("登录任务已建立，请等待登录二维码的发送");
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
                        var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id || x.Bilibili == id);
                        if (data != null)
                        {
                            data.QQ = e.Sender.Id;
                            data.Bilibili = id;
                            data.BilibiliCookie = cookie;
                            await Database.UpdateAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await e.SendPrivateMessage("记录数据已更新");
                                else if (x.IsFaulted && x.Exception != null)
                                    await e.SendPrivateMessage(new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await e.SendPrivateMessage("记录数据因未知原因导致更新失败，请稍后重试");
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
                            await Database.InsertAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await e.SendPrivateMessage("记录数据已添加");
                                else if (x.IsFaulted && x.Exception != null)
                                    await e.SendPrivateMessage(new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await e.SendPrivateMessage("记录数据因未知原因导致更新失败，请稍后重试");
                            });
                        }
                    }
                    else
                        await e.SendPrivateMessage("登录失败\n未能匹配到用户UID");
                },
                async (errMsg) => await e.SendPrivateMessage("登录失败\n" + errMsg)));
        }

        [Command("获取b站用户信息")]
        [CommandDescription("获取并格式化指定B站用户的个人信息")]
        [ParameterDescription(1, "用户UID")]
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
        [CommandDescription("获取并格式化指定B站直播间的信息")]
        [ParameterDescription(1, "直播间ID")]
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
                    .AppendLine().Append(room.BaseToString()).ToString());
            }
            else
                await e.Reply($"[Bilibili Live] 直播间{roomid}信息获取失败");
        }

        [Command("获取b站直播间推流地址")]
        [CommandDescription("获取指定B站直播间的直接推流链接", "可用于如PotPlayer之类的软件进行直接观看而不需要开启浏览器")]
        [ParameterDescription(1, "直播间ID")]
        public static async void LiveRoomStream(SoraMessage e, int roomid)
        {
            LiveStream stream = null;
            try
            {
                stream = LiveStream.Get(roomid);
            }
            catch
            {
            }
            if (stream != null)
            {
                if (stream.LiveStatus != LiveStatus.Live)
                {
                    await e.Reply($"[Bilibili Live] 直播间{roomid}当前未开播");
                }
                else
                {
                    var thread = stream.Thread.OrderByDescending(x => x.Quality).First();
                    var s = new StringBuilder("[Bilibili Live]")
                        .AppendLine().Append($"房间号：{stream.Id}")
                        .AppendLine().Append($"{thread.Quality}")
                        .AppendLine().Append($"{await Utils.GetShortUrl(thread.Url.First())}");
                    await e.Reply(s.ToString());
                }
            }
            else
                await e.Reply($"[Bilibili Live] 直播间{roomid}信息获取失败");
        }

        [Command("获取b站视频信息")]
        [CommandDescription("获取指定B站视频的信息", "为了避免一些东西所以将不会支持视频地址的解析")]
        [ParameterDescription(1, "视频AV号/BV号")]
        public static async void VideoInfo(SoraMessage e, string id)
        {
            Video video = null;
            try
            {
                if (id.ToLower().StartsWith("av"))
                    id = id[2..];
                if (long.TryParse(id, out var av))
                    video = Video.Get(av);
                else
                    video = Video.Get(id);
            }
            catch
            {
            }
            if (video != null)
            {
                await e.Reply(CQCode.CQImage(video.PicUrl), new StringBuilder()
                    .AppendLine().Append(video.BaseToString()).ToString());
            }
            else
                await e.Reply($"[Bilibili] 视频{id}信息获取失败");
        }

        [Command("获取b站视频分P信息")]
        [CommandDescription("获取指定B站视频的分P信息", "为了避免一些东西所以将不会支持视频地址的解析")]
        [ParameterDescription(1, "视频AV号/BV号")]
        public static async void VideoPageInfo(SoraMessage e, string id)
        {
            Video video = null;
            try
            {
                if (id.ToLower().StartsWith("av"))
                    id = id[2..];
                if (long.TryParse(id, out var av))
                    video = Video.Get(av);
                else
                    video = Video.Get(id);
            }
            catch
            {
            }
            if (video != null)
            {
                int hour = video.Duration.Days * 24 + video.Duration.Hours;
                string hourStr = hour > 0 ? $"{hour}时" : string.Empty;
                var sb = new StringBuilder();
                sb.AppendLine(video.Title);
                sb.AppendLine($"av{video.AV}  {video.BV}{(string.IsNullOrEmpty(video.AreaName) ? "" : ("  分区：" + video.AreaName))}");
                sb.AppendLine($"UP：{video.UserName}(https://space.bilibili.com/{video.UserId})");
                sb.AppendLine($"视频共{video.Pages.Length}P 总长度：{hourStr}{video.Duration.Minutes:D2}分{video.Duration.Seconds:D2}秒");
                sb.AppendLine("分P列表如下：");
                foreach (var page in video.Pages)
                {
                    int ph = page.Duration.Days * 24 + page.Duration.Hours;
                    string phStr = ph > 0 ? $"{ph}时" : string.Empty;
                    sb.AppendLine($"  P{page.Index}  长度：{phStr}{page.Duration.Minutes:D2}分{page.Duration.Seconds:D2}秒");
                    sb.Append("    ").AppendLine(page.Name);
                }
                sb.Append(video.Url);
                if (video.Pages.Length > 10)
                {
                    var bin = UbuntuPastebin.Paste(sb.ToString(), "text", "Bilibili Video Pages");
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine(video.Title)
                        .AppendLine($"av{video.AV}  {video.BV}{(string.IsNullOrEmpty(video.AreaName) ? "" : ("  分区：" + video.AreaName))}")
                        .AppendLine($"UP：{video.UserName}(https://space.bilibili.com/{video.UserId})")
                        .AppendLine($"视频共{video.Pages.Length}P 总长度：{hourStr}{video.Duration.Minutes:D2}分{video.Duration.Seconds:D2}秒")
                        .AppendLine("数据过多，请前往以下链接查看")
                        .Append(bin).ToString());
                }
                else
                    await e.Reply(sb.ToString());
            }
            else
                await e.Reply($"[Bilibili] 视频{id}信息获取失败");
        }

        [Command("获取b站音频信息")]
        [CommandDescription("获取指定B站音频的信息", "为了避免一些东西所以将不会支持音频地址的解析")]
        [ParameterDescription(1, "音频AU号")]
        public static async void AudioInfo(SoraMessage e, int id)
        {
            Audio audio = null;
            try
            {
                audio = Audio.Get(id);
            }
            catch
            {
            }
            if (audio != null)
            {
                await e.Reply(CQCode.CQImage(audio.CoverUrl), new StringBuilder()
                    .AppendLine().Append(audio.BaseToString()).ToString());
            }
            else
                await e.Reply($"[Bilibili] 音频{id}信息获取失败");
        }

        [Command("获取b站专栏信息")]
        [CommandDescription("获取指定B站专栏的信息")]
        [ParameterDescription(1, "专栏CV号")]
        public static async void ArticleInfo(SoraMessage e, int id)
        {
            Article article = null;
            try
            {
                article = Article.Get(id);
            }
            catch
            {
            }
            if (article != null)
                await e.Reply(article.ToString());
            else
                await e.Reply($"[Bilibili] 专栏{id}信息获取失败");
        }

        [Command("获取b站动态信息")]
        [CommandDescription("获取指定B站动态的信息")]
        [ParameterDescription(1, "动态ID")]
        public static async void DynamicInfo(SoraMessage e, ulong id)
        {
            Dynamic dynamic = null;
            try
            {
                dynamic = Dynamic.Get(id);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Bilibili", ex.GetFormatString(true));
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
                var np = await dynamic.GetNinePicture();
                if (np != null)
                {
                    var name = Path.GetTempFileName();
                    var output = File.OpenWrite(name);
                    var encoder = new PngEncoder();
                    encoder.Encode(np, output);
                    output.Dispose();
                    await e.Reply(CQCode.CQImage(name));
                }
            }
            else
                await e.Reply($"[Bilibili] 动态{id}信息获取失败");
        }

        [Command("开启直播")]
        [CommandDescription("开启用户直播间", "需要进行过B站登录操作才可使用")]
        [ParameterDescription(1, "分区ID", "默认单机▪其他单机")]
        [ParameterDescription(2, "标题", "默认保持原样不变")]
        public static async void StartLive(SoraMessage e, int area = 235, string title = "")
        {
            title = SoraMessage.Escape(title);
            var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id);
            if (data == null)
            {
                await e.ReplyToOriginal("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.ReplyToOriginal("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.StartLive(roomid, area, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.ReplyToOriginal("服务器返回消息：" + (string)result["message"]);
                else
                {
                    await e.ReplyToOriginal("开播成功，直播分区已设置为 " + (await LiveAreaList.Get(area)).ToString());
                    await e.SendPrivateMessage($"rtmp地址: {(string)result["data"]["rtmp"]["addr"]}\n推流码: {(string)result["data"]["rtmp"]["code"]}");
                    if (!string.IsNullOrEmpty(title))
                        SetLiveTitle(e, title);
                }
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("开启直播")]
        [CommandDescription("开启用户直播间", "需要进行过B站登录操作才可使用")]
        [ParameterDescription(1, "分区关键词", "当只有一个目标时才会切换分区")]
        [ParameterDescription(2, "标题", "默认保持原样不变")]
        public static async void StartLive(SoraMessage e, string area, string title = "")
        {
            area = SoraMessage.Escape(area);
            title = SoraMessage.Escape(title);
            var list = await LiveAreaList.Get(area);
            if (!list.Any())
            {
                await e.ReplyToOriginal($"未能成功搜索到有关于 {area} 的直播分区，请检查输入是否正确");
                return;
            }
            else if (list.Length > 1)
            {
                var sb = new StringBuilder();
                sb.AppendLine("搜索到多个目标分区，请使用准确的分区名称或目标分区ID进行操作：");
                if (list.Length > 10)
                {
                    sb.AppendLine(string.Join(Environment.NewLine, list.Take(10)));
                    sb.Append($"[共搜索到 {list.Length} 个目标，仅显示前 10 个]");
                }
                else
                    sb.AppendLine(string.Join(Environment.NewLine, list));
                await e.ReplyToOriginal(sb.ToString());
                return;
            }
            StartLive(e, list[0].Id, title);
        }

        [Command("关闭直播")]
        [CommandDescription("关闭用户直播间", "需要进行过B站登录操作才可使用")]
        public static async void StopLive(SoraMessage e)
        {
            var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id);
            if (data == null)
            {
                await e.ReplyToOriginal("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.ReplyToOriginal("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.StopLive(roomid, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.ReplyToOriginal("服务器返回消息：" + (string)result["message"]);
                else
                    await e.ReplyToOriginal("已停止直播");
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("设置直播分区")]
        [CommandDescription("切换用户分区为指定分区", "需要进行过B站登录操作才可使用")]
        [ParameterDescription(1, "分区ID")]
        public static async void SetLiveArea(SoraMessage e, int area)
        {
            var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id);
            if (data == null)
            {
                await e.ReplyToOriginal("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.ReplyToOriginal("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.UpdateLiveArea(roomid, area, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]))
                    await e.ReplyToOriginal("服务器返回消息：" + (string)result["message"]);
                else
                    await e.ReplyToOriginal("已成功更换直播分区至 " + (await LiveAreaList.Get(area)).ToString());
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("设置直播分区")]
        [CommandDescription("切换用户分区为指定分区", "需要进行过B站登录操作才可使用")]
        [ParameterDescription(1, "分区关键词", "当只有一个目标时才会切换分区")]
        public static async void SetLiveArea(SoraMessage e, string area)
        {
            area = SoraMessage.Escape(area);
            var list = await LiveAreaList.Get(area);
            if (!list.Any())
            {
                await e.ReplyToOriginal($"未能成功搜索到有关于 {area} 的直播分区，请检查输入是否正确");
                return;
            }
            else if (list.Length > 1)
            {
                var sb = new StringBuilder();
                sb.AppendLine("搜索到多个目标分区，请使用准确的分区名称或目标分区ID进行操作：");
                if (list.Length > 10)
                {
                    sb.AppendLine(string.Join(Environment.NewLine, list.Take(10)));
                    sb.Append($"[共搜索到 {list.Length} 个目标，仅显示前 10 个]");
                }
                else
                    sb.AppendLine(string.Join(Environment.NewLine, list));
                await e.ReplyToOriginal(sb.ToString());
                return;
            }
            SetLiveArea(e, list[0].Id);
        }

        [Command("设置直播标题")]
        [CommandDescription("修改直播标题", "需要进行过B站登录操作才可使用")]
        [ParameterDescription(1, "标题")]
        public static async void SetLiveTitle(SoraMessage e, string title)
        {
            title = SoraMessage.Escape(title);
            var data = await Database.FindAsync<UserData>(x => x.QQ == e.Sender.Id);
            if (data == null)
            {
                await e.ReplyToOriginal("数据库中没有用户信息，无法执行该指令，请通过 +login 进行账户登录");
                return;
            }
            var roomid = BiliLive.GetUserLiveRoom(data.Bilibili);
            if (roomid == 0)
            {
                await e.ReplyToOriginal("用户直播间数据获取失败，请稍后重试");
                return;
            }
            try
            {
                var result = JObject.Parse(BiliLive.UpdateLiveTitle(roomid, title, data.BilibiliCookie));
                if (!string.IsNullOrEmpty((string)result["message"]) && (string)result["message"] != "ok")
                    await e.ReplyToOriginal("服务器返回消息：" + (string)result["message"]);
                else
                    await e.ReplyToOriginal("已成功修改直播标题为 " + title);
            }
            catch
            {
                await e.ReplyToOriginal("操作失败");
            }
        }

        [Command("订阅b站直播"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("订阅目标B站直播间的状态更新事件")]
        [ParameterDescription(1, "直播间ID")]
        public static async void AddLiveListener(SoraMessage e, int roomid)
        {
            var data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "bilibili live"
                && x.Target == roomid.ToString()
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data != null)
            {
                await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "bilibili live",
                Target = roomid.ToString(),
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

        [Command("取消订阅b站直播"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("取消订阅目标B站直播间的状态更新事件")]
        [ParameterDescription(1, "直播间ID")]
        public static async void RemoveLiveListener(SoraMessage e, int roomid)
        {
            var data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "bilibili live"
                && x.Target == roomid.ToString()
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
                    await e.ReplyToOriginal("订阅项目因未知原因导致移除失败，请稍后重试");
            });
        }

        [Command("订阅b站动态"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("订阅目标B站用户动态更新事件")]
        [ParameterDescription(1, "用户UID")]
        public static async void AddDynamicListener(SoraMessage e, int userid)
        {
            var data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString()
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data != null)
            {
                await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "bilibili dynamic",
                Target = userid.ToString(),
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

        [Command("取消订阅b站动态"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("取消订阅目标B站用户动态更新事件")]
        [ParameterDescription(1, "用户UID")]
        public static async void RemoveDynamicListener(SoraMessage e, int userid)
        {
            var data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString()
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
                    await e.ReplyToOriginal("订阅项目因未知原因导致移除失败，请稍后重试");
            });
        }

        [Command("启用b站链接智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("启用B站相关链接的自动信息解析功能")]
        public static async void EnableAutoLink(SoraMessage e)
        {
            var data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data != null)
            {
                if (data.SmartBilibiliLink)
                {
                    await e.ReplyToOriginal("本群已启用该功能，无需再次启用");
                    return;
                }
                data.SmartBilibiliLink = true;
                await Database.UpdateAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用b站链接智能解析功能");
                    else if (x.IsFaulted && x.Exception != null)
                        await e.ReplyToOriginal(new StringBuilder()
                            .AppendLine("因异常导致功能启用失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await e.ReplyToOriginal("因未知原因导致功能启用失败，请稍后重试");
                });
            }
            else
            {
                await Database.InsertAsync(new QQGroupSetting()
                {
                    Group = e.SourceGroup.Id,
                    SmartBilibiliLink = true
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用b站链接智能解析功能");
                    else if (x.IsFaulted && x.Exception != null)
                        await e.ReplyToOriginal(new StringBuilder()
                            .AppendLine("因异常导致功能启用失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await e.ReplyToOriginal("因未知原因导致功能启用失败，请稍后重试");
                });
            }
        }

        [Command("禁用b站链接智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        [CommandDescription("禁用B站相关链接的自动信息解析功能")]
        public static async void DisableAutoLink(SoraMessage e)
        {
            var data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data == null || !data.SmartBilibiliLink)
            {
                await e.ReplyToOriginal("本群未启用该功能，无需禁用");
                return;
            }
            data.SmartBilibiliLink = false;
            await Database.UpdateAsync(data).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.ReplyToOriginal("本群已成功禁用b站链接智能解析功能");
                else if (x.IsFaulted && x.Exception != null)
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("因异常导致功能禁用失败，错误信息：")
                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.ReplyToOriginal("因未知原因导致功能禁用失败，请稍后重试");
            });
        }

        [Command]
        [CommandDescription("转换指定AV号为BV号")]
        [ParameterDescription(1, "AV号")]
        public static async void AV2BV(SoraMessage e, string av = "")
        {
            if (string.IsNullOrEmpty(av))
                await e.ReplyToOriginal("参数错误，请重新输入");
            else
            {
                if (av.ToLower().StartsWith("av"))
                    av = av[2..];
                long id = long.Parse(av);
                string msg;
                try
                {
                    msg = $"[Bilibili][AV→BV] {id} → {BilibiliAVBVConverter.ToBV(id)}";
                }
                catch (Exception ex)
                {
                    msg = ex.Message;
                }
                await e.ReplyToOriginal(msg);
            }
        }

        [Command]
        [CommandDescription("转换指定BV号为AV号")]
        [ParameterDescription(1, "BV号")]
        public static async void BV2AV(SoraMessage e, string bv = "")
        {
            if (string.IsNullOrEmpty(bv))
                await e.ReplyToOriginal("参数错误，请重新输入");
            else
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
                await e.ReplyToOriginal(msg);
            }
        }
    }
}
