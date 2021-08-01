using Discord;
using Discord.WebSocket;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckMethod;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.Listener
{
    public class MinecraftJiraListener : Base.SubscribeListener
    {
        const string type = "minecraft jira";

        readonly MinecraftJiraCheckMethod Checker = new();

        public override async void RefreshListener()
            => await Task.CompletedTask;

        public override async void Listen()
            => Broadcast(await Checker.Check());

        public override async void Broadcast(CheckResult.Base.SubscribeCheckResult result)
        {
            if (result.Updated && result is MinecraftJiraCheckResult b)
            {
                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for {type}");
                var records = await Database.GetArrayAsync<SubscribeList>(x => x.Type == type && x.Target == "java");
                if (records != null && records.Length > 0)
                {
                    var msg = GetString(b);
                    if (Program.Config.QQ)
                    {
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
                                        var api = Program.QQServer.GetSoraApi(bot);
                                        if (await api.CheckHasGroup(group))
                                        {
                                            ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group} with bot {bot}");
                                            await api.SendGroupMessage(group, msg);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (Program.Config.Discord && Program.DiscordServer.Client.ConnectionState == ConnectionState.Connected)
                    {
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
                                        await channel?.SendMessageAsync(msg);
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

        static string GetString(MinecraftJiraCheckResult result)
            => new StringBuilder()
            .AppendLine("[Minecraft Jira]")
            .AppendLine("哇哦，Bugjang杀死了这些虫子:")
            .AppendLine(string.Join(Environment.NewLine, result.Data.Select(x => x.Title)))
            .Append($"统计时间: {result.From:yyyy-MM-dd HH:mm} ~ {result.To:yyyy-MM-dd HH:mm}")
            .ToString();
    }
}
