using Discord;
using Discord.Commands;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Bilibili : ModuleBase<SocketCommandContext>
    {
        [Command("bv2av")]
        public async Task BV2AV(string bv)
            => await ReplyAsync($"[Bilibili][BV→AV] {bv} → {BilibiliAVBVConverter.ToAV(bv)}");

        [Command("av2bv")]
        public async Task AV2BV(long av)
            => await ReplyAsync($"[Bilibili][AV→BV] {av} → {BilibiliAVBVConverter.ToBV(av)}");

        [Command("获取b站用户信息")]
        public async Task UserInfo(int uid)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……(UID: {uid})");
            User user = null;
            try
            {
                user = User.Get(uid);
            }
            catch
            {
            }
            if (user != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + user.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 用户{uid}信息获取失败");
        }

        [Command("获取b站直播间信息")]
        public async Task LiveRoomInfo(int roomid)
        {
            var msg = await ReplyAsync($"[Bilibili Live]正在搜索中……(Room ID: {roomid})");
            LiveRoom room = null;
            try
            {
                room = LiveRoom.Get(roomid);
            }
            catch
            {
            }
            if (room != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili Live]\n" + room.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili Live] 直播间{roomid}信息获取失败");
        }

        [Command("获取b站视频信息")]
        public async Task VideoInfo(int av)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……(av{av})");
            Video video = null;
            try
            {
                video = Video.Get(av);
            }
            catch
            {
            }
            if (video != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + video.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 视频av{av}信息获取失败");
        }

        [Command("获取b站视频信息")]
        public async Task VideoInfo(string bv)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……({bv})");
            Video video = null;
            try
            {
                video = Video.Get(bv);
            }
            catch
            {
            }
            if (video != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + video.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 视频{bv}信息获取失败");
        }

        [Command("获取b站音频信息")]
        public async Task AudioInfo(int id)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……(au{id})");
            Audio audio = null;
            try
            {
                audio = Audio.Get(id);
            }
            catch
            {
            }
            if (audio != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + audio.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 音频au{id}信息获取失败");
        }

        [Command("获取b站专栏信息")]
        public async Task ArticleInfo(int id)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……(au{id})");
            Article article = null;
            try
            {
                article = Article.Get(id);
            }
            catch
            {
            }
            if (article != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + article.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 专栏cv{id}信息获取失败");
        }
        
        [Command("获取b站动态信息")]
        public async Task DynamicInfo(ulong id)
        {
            var msg = await ReplyAsync($"[Bilibili]正在搜索中……({id})");
            Dynamic dynamic = null;
            try
            {
                dynamic = Dynamic.Get(id);
            }
            catch
            {
            }
            if (dynamic != null)
                await msg.ModifyAsync(x => x.Content = "[Bilibili]\n" + dynamic.ToString());
            else
                await msg.ModifyAsync(x => x.Content = $"[Bilibili] 动态{id}信息获取失败");
        }

        [Command("订阅b站直播")]
        public async Task AddLiveListener(int roomid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "bilibili live"
                && x.Target == roomid.ToString() && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await ReplyAsync("本频道已订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.InsertAsync(new SubscribeList()
                {
                    Platform = "discord channel",
                    Type = "bilibili live",
                    Target = roomid.ToString(),
                    Listener = Context.Channel.Id.ToString()
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致添加失败，请稍后重试");
                });
            }
        }

        [Command("取消订阅b站直播")]
        public async Task RemoveLiveListener(int roomid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "bilibili live"
                && x.Target == roomid.ToString() && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await ReplyAsync("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已移除");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致移除失败，请稍后重试");
                });
            }
        }

        [Command("订阅b站动态")]
        public async Task AddDynamicListener(int userid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString() && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data != null)
                {
                    await ReplyAsync("本频道已订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.InsertAsync(new SubscribeList()
                {
                    Platform = "discord channel",
                    Type = "bilibili dynamic",
                    Target = userid.ToString(),
                    Listener = Context.Channel.Id.ToString()
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已添加，如果该目标曾经未被任何人订阅过那么将会在下一次检查时发送一次初始化广播信息");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致添加失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致添加失败，请稍后重试");
                });
            }
        }

        [Command("取消订阅b站动态")]
        public async Task RemoveDynamicListener(int userid)
        {
            var t = await Database.Data.Table<SubscribeList>().ToListAsync();
            if (t != null && t.Count > 0)
            {
                SubscribeList data = t.Where(x => x.Platform == "discord channel" && x.Type == "bilibili dynamic"
                && x.Target == userid.ToString() && x.Listener == Context.Channel.Id.ToString())?.FirstOrDefault();
                if (data == null)
                {
                    await ReplyAsync("本群未订阅该目标，请检查输入是否正确");
                    return;
                }
                await Database.Data.DeleteAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await ReplyAsync("订阅项目已移除");
                    else if (x.IsFaulted && x.Exception != null)
                        await ReplyAsync(new StringBuilder()
                            .AppendLine("订阅项目因异常导致移除失败，错误信息：")
                            .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                            .ToString());
                    else
                        await ReplyAsync("订阅项目因未知原因导致移除失败，请稍后重试");
                });
            }
        }

        [Command("login")]
        public async Task Login()
        {
            var msg = await ReplyAsync("请求已接受，请稍后……");
            var dm = await Context.User.GetOrCreateDMChannelAsync();
            IUserMessage qr = null;
            IUserMessage dmmsg = null;
            Library.Bilibili.Bilibili.QRCodeLoginRequest(
                async (bitmap) =>
                {
                    using var stream = new MemoryStream();
                    bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
                    stream.Seek(0, SeekOrigin.Begin);
                    qr = await dm.SendFileAsync(stream, "1.jpg");
                    bitmap.Dispose();
                    stream.Dispose();
                    dmmsg = await dm.SendMessageAsync("请在5分钟内使用Bilibili客户端扫描二维码进行登录");
                    await msg.ModifyAsync(x => x.Content = "登陆事件已建立，请前往私聊继续操作");
                },
                async () =>
                {
                    await dmmsg.ModifyAsync(x => x.Content = "已检测到扫描事件，请在Bilibili客户端中确认登录");
                },
                async (cookie) =>
                {
                    await qr?.DeleteAsync();
                    if (int.TryParse(cookie.Split(";").Where(x => x.StartsWith("DedeUserID=")).First()[11..], out var id))
                    {
                        var t = await Database.Data.Table<UserData>().ToListAsync();
                        var data = t.Where(x => x.Discord == Convert.ToInt64(Context.User.Id) || x.Bilibili == id).FirstOrDefault();
                        if (data != null)
                        {
                            data.Discord = Convert.ToInt64(Context.User.Id);
                            data.Bilibili = id;
                            data.BilibiliCookie = cookie;
                            await Database.Data.UpdateAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await dmmsg.ModifyAsync(x => x.Content = ":white_check_mark: 登录成功，用户数据已保存");
                                else if (x.IsFaulted && x.Exception != null)
                                    await dmmsg.ModifyAsync(y => y.Content = ":x: " + new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await dmmsg.ModifyAsync(x => x.Content = ":x: 记录数据因未知原因导致更新失败，请稍后重试");
                            });
                        }
                        else
                        {
                            data = new()
                            {
                                Discord = Convert.ToInt64(Context.User.Id),
                                Bilibili = id,
                                BilibiliCookie = cookie
                            };
                            await Database.Data.InsertAsync(data).ContinueWith(async x =>
                            {
                                if (x.Result > 0)
                                    await dmmsg.ModifyAsync(x => x.Content = ":white_check_mark: 登录成功，用户数据已保存");
                                else if (x.IsFaulted && x.Exception != null)
                                    await dmmsg.ModifyAsync(y => y.Content = ":x: " + new StringBuilder()
                                        .AppendLine("记录数据因异常导致更新失败，错误信息：")
                                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                                        .ToString());
                                else
                                    await dmmsg.ModifyAsync(x => x.Content = ":x: 记录数据因未知原因导致更新失败，请稍后重试");
                            });
                        }
                        await dmmsg.ModifyAsync(x => x.Content = ":white_check_mark: 登录成功，用户数据已保存");
                    }
                    else
                        await dmmsg.ModifyAsync(x => x.Content = ":x: 登录失败：未能匹配到用户UID");
                    await msg.ModifyAsync(x => x.Content = ":o: 事件已结束");
                },
                async (errMsg) =>
                {
                    await qr?.DeleteAsync();
                    await msg?.ModifyAsync(x => x.Content = ":x: 登录失败：" + errMsg);
                    await dmmsg?.ModifyAsync(x => x.Content = ":x: 登录失败：" + errMsg);
                });
        }
    }
}
