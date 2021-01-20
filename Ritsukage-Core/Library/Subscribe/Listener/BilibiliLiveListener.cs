using Discord;
using Discord.WebSocket;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckMethod;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using Sora.Entities.CQCodes;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.Listener
{
    public class BilibiliLiveListener : Base.SubscribeListener
    {
        const string type = "bilibili live";

        readonly object _lock = new();

        readonly List<BilibiliLiveCheckMethod> Checker = new();

        public override async void RefreshListener()
        {
            var t = Database.Data.Table<SubscribeList>();
            var list = await t.Where(x => x.Type == type)?.ToListAsync();
            if (list == null)
            {
                Checker.Clear();
                return;
            }
            lock (_lock)
            {
                foreach (var c in Checker.ToArray())
                {
                    if (!list.Where(x => x.Target == c.RoomId.ToString()).Any())
                    {
                        Checker.Remove(c);
                    }
                }
                foreach (var l in list)
                {
                    if (!Checker.Where(x => l.Target == x.RoomId.ToString()).Any())
                    {
                        if (int.TryParse(l.Target, out var id))
                        {
                            ConsoleLog.Debug("Subscribe", $"Start subscribe listener: {type} {id}");
                            Checker.Add(new(id));
                        }
                    }
                }
            }
        }

        public override async void Listen()
        {
            await Task.Run(async () =>
            {
                foreach (var c in Checker.ToArray())
                {
                    Broadcast(await c.Check());
                    await Task.Delay(5000);
                }
            });
        }

        public override async void Broadcast(CheckResult.Base.SubscribeCheckResult result)
        {
            if (result.Updated && result is BilibiliLiveCheckResult b)
            {
                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for {type} {b.RoomId}");
                var t = await Database.Data.Table<SubscribeList>().ToListAsync();
                var records = t.Where(x => x.Type == type && x.Target == b.RoomId.ToString())?.ToArray();
                if (records != null && records.Length > 0)
                {
                    if (Program.Config.QQ)
                    {
                        var qqmsg = GetQQMessageChain(b);
                        var bots = Program.QQServer.GetBotList();
                        var qqgroups = records.Where(x => x.Platform == "qq group")?.Select(x => x.Listener)?.ToArray();
                        if (qqgroups != null && qqgroups.Length > 0)
                        {
                            foreach (var qqgroup in qqgroups)
                            {
                                if (long.TryParse(qqgroup, out var group))
                                {
                                    ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group}");
                                    foreach (var bot in bots)
                                    {
                                        if (GroupList.GetInfo(bot, group).GroupId == group)
                                        {
                                            ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group} with bot {bot}");
                                            var api = Program.QQServer.GetSoraApi(bot);
                                            await api.SendGroupMessage(group, qqmsg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Program.Config.Discord && Program.DiscordServer.Client.ConnectionState == ConnectionState.Connected)
                    {
                        var dcmsg = GetDiscordMessageChain(b);
                        var channels = records.Where(x => x.Platform == "discord channel")?.Select(x => x.Listener)?.ToArray();
                        if (channels != null && channels.Length > 0)
                        {
                            foreach (var id in channels)
                            {
                                if (ulong.TryParse(id, out var cid))
                                {
                                    ConsoleLog.Debug("Subscribe", $"Boardcast updated info to discord channel {cid}");
                                    try
                                    {
                                        var channel = (SocketTextChannel)Program.DiscordServer.Client.GetChannel(cid);
                                        await channel?.SendMessageAsync(dcmsg);
                                    }
                                    catch
                                    {
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        static string GetLiveStatus(LiveStatus status)
            => status switch
            {
                LiveStatus.Live => "当前正在直播",
                LiveStatus.Round => "当前正在轮播",
                _ => "当前未开播"
            };

        static object[] GetQQMessageChain(BilibiliLiveCheckResult result)
        {
            ArrayList msgs = new();
            if (result.UpdateType == BilibiliLiveUpdateType.Initialization)
            {
                msgs.Add(CQCode.CQImage(result.Cover));
                msgs.Add(new StringBuilder()
                    .AppendLine()
                    .AppendLine($"{result.User} 直播状态初始化")
                    .AppendLine("标题：" + result.Title)
                    .AppendLine("直播间ID：" + result.RoomId)
                    .AppendLine("当前分区：" + result.Area)
                    .AppendLine("当前气人值：" + result.Online)
                    .AppendLine(GetLiveStatus(result.Status))
                    .Append(result.Url)
                    .ToString());
            }
            else if (result.UpdateType == BilibiliLiveUpdateType.LiveStatus)
            {
                if (result.Status == LiveStatus.Cancel)
                    msgs.Add($"{result.User} 下播莉……");
                else if (result.Status == LiveStatus.Round)
                    msgs.Add($"{result.User} 下播莉(轮播中)……");
                else
                {
                    msgs.Add(CQCode.CQImage(result.Cover));
                    msgs.Add(new StringBuilder()
                        .AppendLine()
                        .AppendLine($"{result.User} 直播开始啦")
                        .AppendLine("标题：" + result.Title)
                        .AppendLine("直播间ID：" + result.RoomId)
                        .AppendLine("当前分区：" + result.Area)
                        .AppendLine("当前气人值：" + result.Online)
                        .AppendLine(GetLiveStatus(result.Status))
                        .Append(result.Url)
                        .ToString());
                }
            }
            else if (result.UpdateType == BilibiliLiveUpdateType.Title)
            {
                msgs.Add(new StringBuilder()
                    .AppendLine($"{result.User} 更换了直播标题")
                    .AppendLine("标题：" + result.Title)
                    .AppendLine("直播间ID：" + result.RoomId)
                    .AppendLine("当前分区：" + result.Area)
                    .AppendLine("当前气人值：" + result.Online)
                    .AppendLine(GetLiveStatus(result.Status))
                    .Append(result.Url)
                    .ToString());
            }
            return msgs.ToArray();
        }
        
        static string GetDiscordMessageChain(BilibiliLiveCheckResult result)
        {
            if (result.UpdateType == BilibiliLiveUpdateType.Initialization)
                return new StringBuilder()
                    .AppendLine(result.Cover)
                    .AppendLine($"{result.User} 直播状态初始化")
                    .AppendLine("标题：" + result.Title)
                    .AppendLine("直播间ID：" + result.RoomId)
                    .AppendLine("当前分区：" + result.Area)
                    .AppendLine("当前气人值：" + result.Online)
                    .AppendLine(GetLiveStatus(result.Status))
                    .Append(result.Url)
                    .ToString();
            else if (result.UpdateType == BilibiliLiveUpdateType.LiveStatus)
            {
                if (result.Status == LiveStatus.Cancel)
                    return $"{result.User} 下播莉……";
                else if (result.Status == LiveStatus.Round)
                    return $"{result.User} 下播莉(轮播中)……";
                else
                    return new StringBuilder()
                        .AppendLine(result.Cover)
                        .AppendLine($"{result.User} 直播开始啦")
                        .AppendLine("标题：" + result.Title)
                        .AppendLine("直播间ID：" + result.RoomId)
                        .AppendLine("当前分区：" + result.Area)
                        .AppendLine("当前气人值：" + result.Online)
                        .AppendLine(GetLiveStatus(result.Status))
                        .Append(result.Url)
                        .ToString();
            }
            else if (result.UpdateType == BilibiliLiveUpdateType.Title)
                return new StringBuilder()
                    .AppendLine($"{result.User} 更换了直播标题")
                    .AppendLine("标题：" + result.Title)
                    .AppendLine("直播间ID：" + result.RoomId)
                    .AppendLine("当前分区：" + result.Area)
                    .AppendLine("当前气人值：" + result.Online)
                    .AppendLine(GetLiveStatus(result.Status))
                    .Append(result.Url)
                    .ToString();
            return "";
        }
    }
}
