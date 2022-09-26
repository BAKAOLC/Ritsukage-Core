using Ritsukage.Library.Data;
using Ritsukage.Library.Graphic;
using Ritsukage.Library.Pixiv.Extension;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.Entities.Segment;
using Sora.Enumeration.EventParamsType;
using System;
using System.Collections;
using System.IO;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Pixiv")]
    public static class Pixiv
    {
        [Command("pixiv")]
        [CommandDescription("获取指定Pixiv作品信息")]
        [ParameterDescription(1, "Issust ID", "接口来自 https://github.com/mixmoe/HibiAPI")]
        public static void GetIllustDetail(SoraMessage e, params int[] ids)
            => GetIllustDetail(ids, async (msg) => await e.ReplyToOriginal(msg), async (msg) => await e.Reply(msg));

        static async void _GetIllustDetail(int id, Action<object[]> Reply, Action<object[]> SendMessage, bool slient = false)
        {
            try
            {
                var detail = await Illust.Get(id);
                if (detail == null)
                {
                    if (!slient)
                    {
                        Reply?.Invoke(new object[] { $"数据(pid:{id})获取失败，请稍后再试" });
                    }
                    return;
                }
                ArrayList msg = new();
                if (detail.IsUgoira)
                {
                    var ugoira = await detail.GetUgoira();
                    if (ugoira == null)
                    {
                        if (!slient)
                        {
                            Reply?.Invoke(new object[] { $"动图数据(pid:{id})获取失败" });
                        }
                    }
                    else
                    {
                        if (!slient)
                        {
                            Reply?.Invoke(new object[] { $"动图数据(pid:{id})获取成功，正在进行压缩..." });
                        }
                        var img = await ugoira.LimitGifScale(500, 500);
                        var stream = await img.SaveGifToStream();
                        stream = await GIFsicle.Compress(stream);
                        var filename = Path.GetTempFileName();
                        GIFsicle.SaveStreamToFile(stream, filename);
                        msg.Add(SoraSegment.Image(filename));
                    }
                }
                else
                {
                    foreach (var img in detail.Images)
                    {
                        var cache = await DownloadManager.GetCache(img.Medium);
                        if (string.IsNullOrEmpty(cache))
                        {
                            var url = ImageUrls.ToPixivCat(img.Medium);
                            cache = await DownloadManager.GetCache(url);
                            if (string.IsNullOrEmpty(cache))
                            {
                                cache = await DownloadManager.Download(url, enableAria2Download: true);
                                if (string.IsNullOrEmpty(cache))
                                {
                                    cache = await DownloadManager.Download(img.Medium, detail.Url, enableAria2Download: true);
                                    if (string.IsNullOrEmpty(cache))
                                    {
                                        msg.Add("[图像缓存失败]");
                                        continue;
                                    }
                                }
                            }
                        }
                        ImageUtils.LimitImageScale(cache, 2500, 2500);
                        msg.Add(SoraSegment.Image(cache));
                    }
                }
                msg.Add(detail.ToString());
                SendMessage?.Invoke(msg.ToArray());
            }
            catch (Exception ex)
            {
                ConsoleLog.Debug("QQ Command - Pixiv", ex.GetFormatString(true));
                if (!slient)
                {
                    Reply?.Invoke(new object[] { $"处理作品(pid:{id})时发生异常错误，任务已终止" });
                }
            }
        }

        public static void GetIllustDetail(int id, Action<object[]> Reply, Action<object[]> SendMessage, bool slient = false)
        {
            if (!slient)
                Reply?.Invoke(new object[] { $"数据(pid:{id})获取中，请稍后" });
            _GetIllustDetail(id, Reply, SendMessage, slient);
        }

        public static void GetIllustDetail(int[] ids, Action<object[]> Reply, Action<object[]> SendMessage, bool slient = false)
        {
            if (!slient)
                Reply?.Invoke(new object[] { $"数据(pid:{string.Join(", ", ids)})获取中，请稍后" });
            foreach (var id in ids)
                _GetIllustDetail(id, Reply, SendMessage, slient);
        }

        [Command("启用pixiv智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void EnableAutoLink(SoraMessage e)
        {
            QQGroupSetting data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data != null)
            {
                if (data.SmartPixivLink)
                {
                    await e.ReplyToOriginal("本群已启用该功能，无需再次启用");
                    return;
                }
                data.SmartPixivLink = true;
                await Database.UpdateAsync(data).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用pixiv智能解析功能");
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
                    SmartPixivLink = true
                }).ContinueWith(async x =>
                {
                    if (x.Result > 0)
                        await e.ReplyToOriginal("本群已成功启用pixiv智能解析功能");
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

        [Command("禁用pixiv智能解析"), CanWorkIn(WorkIn.Group), LimitMemberRoleType(MemberRoleType.Owner)]
        public static async void DisableAutoLink(SoraMessage e)
        {
            QQGroupSetting data = await Database.FindAsync<QQGroupSetting>(x => x.Group == e.SourceGroup.Id);
            if (data == null || !data.SmartPixivLink)
            {
                await e.ReplyToOriginal("本群未启用该功能，无需禁用");
                return;
            }
            data.SmartPixivLink = false;
            await Database.UpdateAsync(data).ContinueWith(async x =>
            {
                if (x.Result > 0)
                    await e.ReplyToOriginal("本群已成功禁用pixiv智能解析功能");
                else if (x.IsFaulted && x.Exception != null)
                    await e.ReplyToOriginal(new StringBuilder()
                        .AppendLine("因异常导致功能禁用失败，错误信息：")
                        .Append(ConsoleLog.ErrorLogBuilder(x.Exception))
                        .ToString());
                else
                    await e.ReplyToOriginal("因未知原因导致功能禁用失败，请稍后重试");
            });
        }
    }
}
