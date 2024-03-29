﻿using Ritsukage.Library.Data;
using Ritsukage.Library.Graphic;
using Ritsukage.Library.Minecraft.Changelog;
using Ritsukage.Library.Minecraft.Jila;
using Ritsukage.Library.Minecraft.Server;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.Segment;
using Sora.Enumeration.EventParamsType;
using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Minecraft")]
    public static partial class Minecraft
    {
        const string Indent = "    ";
        public static string GetIssueInfo(string id)
        {
            var issue = Issue.GetIssue(id);
            if (issue == null)
                return $"未能获取到ID为 {issue.Id} 的issue";
            return GetIssueInfo(issue);
        }
        public static string GetIssueInfo(Issue issue)
        {
            var sb = new StringBuilder().AppendLine(issue.Title);
            sb.Append("类型: " + issue.Type).Append(Indent)
                .AppendLine("分类: " + issue.Category);
            sb.Append("状态: " + issue.Status).Append(Indent)
                .AppendLine("解决方案: " + issue.Resolution);
            {
                bool flag = false;
                if (!string.IsNullOrWhiteSpace(issue.ConfirmationStatus))
                {
                    flag = true;
                    sb.Append("确认状态: " + issue.ConfirmationStatus);
                }
                if (!string.IsNullOrWhiteSpace(issue.MojangPriority))
                {
                    if (flag) sb.Append(Indent);
                    sb.AppendLine("Mojang处理优先级: " + issue.MojangPriority);
                }
                else if (flag) sb.AppendLine();
            }
            foreach (var i in issue.IssueLinks)
                sb.AppendLine(i.ToString());
            sb.AppendLine("发现版本: " + string.Join(", ", issue.Versions));
            if (!string.IsNullOrWhiteSpace(issue.Platform))
                sb.AppendLine("产生平台: " + issue.Platform);
            if (issue.FixVersions.Length > 0)
                sb.AppendLine("修复版本: " + string.Join(", ", issue.FixVersions));
            if (issue.Labels.Length > 0)
                sb.AppendLine("标签: " + string.Join(", ", issue.Labels));
            sb.AppendLine("创建于: " + issue.CreatedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.AppendLine("更新于: " + issue.UpdatedTime.ToString("yyyy-MM-dd HH:mm:ss"));
            if (issue.ResolvedTime != null)
                sb.AppendLine("解决于: " + issue.ResolvedTime.Value.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append(issue.Url);
            return sb.ToString();
        }

        [Command("获取mc服务器状态")]
        [CommandDescription("获取指定MC服务器的状态信息")]
        [ParameterDescription(1, "服务器IP")]
        public static async void ServerStatus(SoraMessage e, string target)
        {
            var x = GetServerIPRegex().Match(target);
            string host = x.Groups["host"].Value;
            ushort port = 25565;
            if (ushort.TryParse(x.Groups["port"].Value, out ushort _port))
                port = _port;
            ServerInfo info = new ServerInfo(host, port);
            await info.StartGetServerInfoAsync();
            if (info.State == ServerInfo.StateType.GOOD)
            {
                var sb = new StringBuilder();
                sb.AppendLine("[Minecraft Server Status]");
                sb.AppendLine($"IP: {host}:{port}");
                sb.AppendLine($"Version: {info.GameVersion}");
                sb.AppendLine($"Players: {info.CurrentPlayerCount} / {info.MaxPlayerCount}");
                sb.AppendLine($"Latency: {info.Ping}ms");
                sb.AppendLine($"MOTD：").Append(info.MOTD);
                if (info.IconData != null)
                {
                    var icon = Image.Load<Rgba32>(info.IconData);
                    await e.Reply(SoraSegment.Image(icon.ToBase64File()), Environment.NewLine, sb.ToString());
                }
                else
                    await e.Reply(sb.ToString());
            }
            else
                await e.ReplyToOriginal("未能成功获取到目标服务器的数据，可能为参数输入错误或目标已离线");
        }

        [Command]
        [CommandDescription("获取指定MOJIRA问题的信息")]
        [ParameterDescription(1, "ID")]
        public static async void MOJIRA(SoraMessage e, string id)
        {
            if (!GetMOJIRAIDRegex().IsMatch(id))
                await e.ReplyToOriginal($"不合法的ID指定：{id}");
            else
            {
                try
                {
                    await e.ReplyToOriginal(GetIssueInfo(id));
                }
                catch (Exception ex)
                {
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("获取信息时发生错误：")
                        .Append(ex.GetFormatString())
                        .ToString());
                }
            }
        }

        [Command("获取mc修复列表")]
        [CommandDescription("获取指定日期修复的问题内容")]
        [ParameterDescription(1, "日期")]
        public static async void GetMojiraList(SoraMessage e, DateTime date)
        {
            Issue[] issues = null;
            try
            {
                issues = JiraExtension.GetMCFixedIssues(date);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Minecraft Jira Checker", ConsoleLog.ErrorLogBuilder(ex));
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("获取信息时发生错误：")
                    .Append(ex.GetFormatString())
                    .ToString());
            }
            if (issues.Length > 0)
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("[Minecraft Jira]")
                    .AppendLine("哇哦，Bugjang杀死了这些虫子:")
                    .AppendLine(string.Join(Environment.NewLine, issues.Select(x => x.Title)))
                    .Append($"统计时间: {date.Date:yyyy-MM-dd HH:mm} ~ {date.Date.AddDays(1):yyyy-MM-dd HH:mm}")
                    .ToString());
            else
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("[Minecraft Jira]")
                    .AppendLine("可恶，这段时间Bugjang一个虫子也没有杀")
                    .Append($"统计时间: {date.Date:yyyy-MM-dd HH:mm} ~ {date.Date.AddDays(1):yyyy-MM-dd HH:mm}")
                    .ToString());
        }

        [Command("获取mc修复列表")]
        [CommandDescription("获取指定时间区间内修复的问题内容")]
        [ParameterDescription(1, "起始时间")]
        [ParameterDescription(2, "结束时间")]
        public static async void GetMojiraList(SoraMessage e, DateTime from, DateTime to)
        {
            Issue[] issues = null;
            try
            {
                issues = JiraExtension.GetMCFixedIssues(from, to);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Minecraft Jira Checker", ConsoleLog.ErrorLogBuilder(ex));
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("获取信息时发生错误：")
                    .Append(ex.GetFormatString())
                    .ToString());
            }
            if (issues.Length > 0)
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("[Minecraft Jira]")
                    .AppendLine("哇哦，Bugjang杀死了这些虫子:")
                    .AppendLine(string.Join(Environment.NewLine, issues.Select(x => x.Title)))
                    .Append($"统计时间: {from:yyyy-MM-dd HH:mm} ~ {to:yyyy-MM-dd HH:mm}")
                    .ToString());
            else
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("[Minecraft Jira]")
                    .AppendLine("可恶，这段时间Bugjang一个虫子也没有杀")
                    .Append($"统计时间: {from:yyyy-MM-dd HH:mm} ~ {to:yyyy-MM-dd HH:mm}")
                    .ToString());
        }

        [Command("查询mc快照更新日志")]
        [CommandDescription("获取指定版本的快照更新日志")]
        [ParameterDescription(1, "版本号")]
        public static async void FindMCSnapshot(SoraMessage e, string version)
        {
            var vm = GetMinecraftVersionRegex().Match(version.Trim());
            var mainVersion = vm.Groups["mainVersion"].Value;
            var subType = "snapshot";
            var subNum = "";
            if (vm.Groups["sub"].Success && !string.IsNullOrWhiteSpace(vm.Groups["sub"].Value))
            {
                subType = vm.Groups["subType"].Value;
                subNum = vm.Groups["subNum"].Value;
            }
            string changelog = null;
            var articles = new ArticleList("snapshot");
            var article = articles.Articles.Where(x => x.Key.Contains(mainVersion)
            && (subType == "snapshot" || (subType == "pre" ? x.Key.Contains("PRE-RELEASE")
            : subType == "rc" && x.Key.Contains("Release Candidate")) && x.Key.Contains(subNum))).FirstOrDefault();
            if (!string.IsNullOrEmpty(article.Key))
                changelog = article.Value;
            if (!string.IsNullOrEmpty(changelog))
                MoChangeLogsFormat(e, changelog);
            else
                await e.Reply($"未能查找到 {version} 的更新日志");
        }

        [Command]
        [CommandDescription("获取指定类型的更新日志")]
        [ParameterDescription(1, "类型")]
        public static async void MoChangeLogs(SoraMessage e, string type)
        {
            try
            {
                var articles = new ArticleList(type);
                if (articles.Articles.Count > 0)
                    await e.Reply(new StringBuilder()
                        .AppendLine(articles.Title)
                        .Append(string.Join(Environment.NewLine,
                        articles.Articles.Select(x => x.Key + Environment.NewLine + "    " + x.Value).Take(5)))
                        .ToString());
                else
                    await e.ReplyToOriginal("无效的类型 (仅支持 beta/release/snapshot)");
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("获取文章列表时发生错误")
                    .Append(ex.GetFormatString()));
            }
        }

        [Command]
        [CommandDescription("格式化指定的更新日志页面为Markdown文本")]
        [ParameterDescription(1, "网页URL")]
        public static async void MoChangeLogsFormat(SoraMessage e, string url)
        {
            if (!url.StartsWith("https://feedback.minecraft.net/hc/en-us/articles/"))
            {
                await e.ReplyToOriginal("无效的目标文章网址");
                return;
            }
            try
            {
                var article = new Article(url);
                if (string.IsNullOrWhiteSpace(article.Title))
                {
                    await e.ReplyToOriginal("无效的目标文章网址");
                    return;
                }
                var bin = UbuntuPastebin.Paste(article.Markdown, "md", "Mojang");
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("目标文章已格式化至以下地址暂存，请及时查阅以免数据过期")
                    .Append(bin).ToString());
            }
            catch (Exception ex)
            {
                await e.ReplyToOriginal(new StringBuilder()
                    .AppendLine("获取文章时发生错误")
                    .Append(ex.GetFormatString()));
            }
        }

        [Command("订阅minecraft更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddVersionListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "minecraft version"
                && x.Target == "java"
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data != null)
            {
                await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "minecraft version",
                Target = "java",
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

        [Command("取消订阅minecraft更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveVersionListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "minecraft version"
                && x.Target == "java"
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
                    await e.AutoAtReply("订阅项目因未知原因导致移除失败，请稍后重试");
            });
        }

        [Command("订阅mojira更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void AddJiraListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "minecraft jira"
                && x.Target == "java"
                && x.Listener == e.SourceGroup.Id.ToString());
            if (data != null)
            {
                await e.ReplyToOriginal("本群已订阅该目标，请检查输入是否正确");
                return;
            }
            await Database.InsertAsync(new SubscribeList()
            {
                Platform = "qq group",
                Type = "minecraft jira",
                Target = "java",
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

        [Command("取消订阅mojira更新"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void RemoveJiraListener(SoraMessage e)
        {
            SubscribeList data = await Database.FindAsync<SubscribeList>(
                x
                => x.Platform == "qq group"
                && x.Type == "minecraft jira"
                && x.Target == "java"
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
                    await e.AutoAtReply("订阅项目因未知原因导致移除失败，请稍后重试");
            });
        }

        [Command("启用mojira智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void EnableAutoLink(SoraMessage e)
        {
            QQGroupSetting data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data != null)
            {
                if (data.SmartMinecraftLink)
                {
                    await e.ReplyToOriginal("本群已启用该功能，无需再次启用");
                    return;
                }
                data.SmartMinecraftLink = true;
                await Database.UpdateAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用mojira智能解析功能");
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
                    SmartMinecraftLink = true
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用mojira智能解析功能");
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

        [Command("禁用mojira智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void DisableAutoLink(SoraMessage e)
        {
            QQGroupSetting data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data == null || !data.SmartMinecraftLink)
            {
                await e.ReplyToOriginal("本群未启用该功能，无需禁用");
                return;
            }
            data.SmartMinecraftLink = false;
            await Database.UpdateAsync(data).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.ReplyToOriginal("本群已成功禁用mojira智能解析功能");
                else if (x.IsFaulted && x.Exception != null)
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("因异常导致功能禁用失败，错误信息：")
                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.ReplyToOriginal("因未知原因导致功能禁用失败，请稍后重试");
            });
        }

        [GeneratedRegex("^MC(PE)?-\\d+$")]
        private static partial Regex GetMOJIRAIDRegex();

        [GeneratedRegex("^(?<mainVersion>[^-]+)(?<sub>-(?<subType>pre|rc)(?<subNum>\\d+))?$")]
        private static partial Regex GetMinecraftVersionRegex();

        [GeneratedRegex("^(?<host>[^:]+)(:(?<port>\\d+))?$")]
        private static partial Regex GetServerIPRegex();
    }
}
