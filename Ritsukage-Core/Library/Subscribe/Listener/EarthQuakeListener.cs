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
    public class EarthQuakeListener : Base.SubscribeListener
    {
        const string type = "earth quake";
        const string target = "cn";

        readonly EarthQuakeCheckMethod Checker = new();

        public override async void RefreshListener()
            => await Task.CompletedTask;

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
            if (result.Updated && result is EarthQuakeCheckResult b)
            {
                ConsoleLog.Debug("Subscribe", $"Boardcast updated info for {type}");
                var records = await Database.GetArrayAsync<SubscribeList>(x => x.Type == type && x.Target == target);
                if (records != null && records.Length > 0)
                {
                    var datas = b.Data.Where(x => x.震级 >= 4).Select(x => x.ToString());
                    if (datas.Any())
                    {
                        var sb = new StringBuilder();
                        sb.Append("[Earth Quake]");
                        foreach (var m in datas)
                            sb.AppendLine().Append(m);
                        var msg = sb.ToString();
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
                                            _ = Task.Factory.StartNew(async () =>
                                            {
                                                var api = Program.QQServer.GetSoraApi(bot);
                                                if (await api.CheckHasGroup(group))
                                                {
                                                    ConsoleLog.Debug("Subscribe", $"Boardcast updated info for group {group} with bot {bot}");
                                                    await api.SendGroupMessage(group, msg);
                                                }
                                            });
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
                                        _ = Task.Factory.StartNew(async () =>
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
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
