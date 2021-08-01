using Discord;
using Discord.WebSocket;
using Ritsukage.Library.Data;
using Ritsukage.Library.Minecraft.Changelog;
using Ritsukage.Library.Subscribe.CheckMethod;
using Ritsukage.Library.Subscribe.CheckResult;
using Ritsukage.QQ;
using Ritsukage.Tools.Console;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Library.Subscribe.Listener
{
    public class MinecraftVersionListener : Base.SubscribeListener
    {
        const string type = "minecraft version";

        readonly MinecraftVersionCheckMethod Checker = new();

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
            if (result.Updated && result is MinecraftVersionCheckResult b)
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

        static string GetString(MinecraftVersionCheckResult result)
        {
            var m = Regex.Match(result.Title.Trim(), "^(?<version>[^ ]+) (?<type>快照|正式版)更新$");
            var type = m.Groups["type"].Value;
            var version = m.Groups["version"].Value.Trim();
            var vm = Regex.Match(version, @"^(?<mainVersion>[^-]+)(?<sub>-(?<subType>pre|rc)(?<subNum>\d+))?$");
            var mainVersion = vm.Groups["mainVersion"].Value;
            var subType = type == "快照" ? "snapshot" : "release";
            var subNum = "";
            if (vm.Groups["sub"].Success && !string.IsNullOrWhiteSpace(vm.Groups["sub"].Value))
            {
                subType = vm.Groups["subType"].Value;
                subNum = vm.Groups["subNum"].Value;
            }
            string changelog = null;
            switch (subType)
            {
                case "snapshot":
                case "pre":
                case "rc":
                    try
                    {
                        var articles = new ArticleList("snapshot");
                        var article = articles.Articles.Where(x => x.Key.Contains(mainVersion)
                        && subType == "snapshot" || ((subType == "pre" ? x.Key.Contains("PRE-RELEASE")
                        : subType == "rc" && x.Key.Contains("Release Candidate")) && x.Key.Contains(subNum))).FirstOrDefault();
                        if (!string.IsNullOrEmpty(article.Key))
                            changelog = article.Value;
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Subscribe", ex.GetFormatString());
                    }
                    break;
            }
            var sb = new StringBuilder()
                .AppendLine("[Minecraft]")
                .AppendLine(result.Title)
                .Append(result.Time.ToString("yyyy-MM-dd HH:mm:ss"));
            if (!string.IsNullOrEmpty(changelog))
                sb.AppendLine().Append("Change logs: " + changelog);
            return sb.ToString();
        }
    }
}
