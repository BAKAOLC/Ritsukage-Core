using Discord;
using Discord.WebSocket;
using Ritsukage.Library.Data;
using Ritsukage.Library.Subscribe.CheckMethod;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.Listener
{
    public class MinecraftVersionListener : Base.SubscribeListener
    {
        const string type = "minecraft version";

        readonly MinecraftVersionCheckMethod Checker = new();

        public override async void RefreshListener()
        {
            await Task.CompletedTask;
        }

        public override async void Listen()
        {
            await Task.Run(async () =>
            {
                Broadcast(await Checker.Check());
                await Task.Delay(5000);
            });
        }

        public override async void Broadcast(CheckResult.Base.SubscribeCheckResult result)
        {
            if (result.Updated && result is MinecraftVersionCheckResult b)
            {
                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for {type}");
                var t = await Database.Data.Table<SubscribeList>().ToListAsync();
                var records = t.Where(x => x.Type == type && x.Target == "java")?.ToArray();
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
                                        if (GroupList.GetInfo(bot, group).GroupId == group)
                                        {
                                            ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group} with bot {bot}");
                                            var api = Program.QQServer.GetSoraApi(bot);
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

        static string GetString(MinecraftVersionCheckResult result)
            => new StringBuilder()
                .AppendLine("[Minecraft]")
                .AppendLine(result.Title)
                .Append(result.Time.ToString("yyyy-MM-dd HH:mm:ss"))
                .ToString();
    }
}
