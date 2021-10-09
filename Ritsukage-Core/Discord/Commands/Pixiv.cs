using Discord;
using Discord.Commands;
using Ritsukage.Library.Graphic;
using Ritsukage.Library.Pixiv.Extension;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Pixiv : ModuleBase<SocketCommandContext>
    {
        static readonly string TitleContent = "``Pixiv ID: {0} 基础数据已获取，开始获取图像内容……``";
        static readonly string InfoContent = new StringBuilder()
            .AppendLine("``Current: {0} / {1}``")
            .AppendLine("``Progress: {2} / {3}  {4:F2}%``")
            .Append("``Successed: {5}  Failed: {6}``")
            .ToString();

        static async Task UpdateMessage(IUserMessage message, string text)
        {
            try
            {
                await message.ModifyAsync(x => x.Content = text);
            }
            catch
            { }
        }

        static async Task UpdateInfo(IUserMessage message, params object[] args)
            => await UpdateMessage(message, string.Format(InfoContent, args));

        async Task SendImage(Illust detail, IUserMessage message = null)
        {
            int id = detail.Id;
            if (message == null)
                message = await ReplyAsync(string.Format(TitleContent, id));
            else
                await UpdateMessage(message, string.Format(TitleContent, id));
            var info = await ReplyAsync("``== loading ==``");
            int total = detail.Images.Length;
            int successed = 0, failed = 0;
            int current = -1;
            long receivedbyte = 0, totalbyte = 0;
            double percentage = 0;
            await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
            if (detail.IsUgoira)
            {
                current++;
                var ugoira = await detail.GetUgoira(
                 DownloadStartedAction: async (e) =>
                 {
                     receivedbyte = 0;
                     totalbyte = e.FileSize;
                     percentage = 0;
                     await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                 },
                 DownloadProgressChangedAction: async (e) =>
                 {
                     receivedbyte = e.ReceivedBytes;
                     totalbyte = e.TotalBytes;
                     percentage = e.DownloadPercentage;
                     await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                 },
                 DownloadFileCompletedAction: async (e) =>
                 {
                     if (e.Status == DownloadTaskStatus.Completed)
                     {
                         receivedbyte = totalbyte;
                         percentage = 100;
                         await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                     }
                 }, UpdateInfoDelay: 2000);
                if (ugoira == null)
                    await UpdateInfo(message, $"动图数据(pid: {id})获取失败");
                else
                {
                    var img = await ugoira.LimitGifScale(350, 350);
                    var stream = await img.SaveGifToStream();
                    stream = await GIFsicle.Compress(stream);
                    await Context.Channel.SendFileAsync(stream, $"pixiv-{id}.gif");
                }
            }
            else
            {
                Stream[] streams = new Stream[total];
                foreach (var img in detail.Images)
                {
                    current++;
                    var cache = await DownloadManager.GetCache(img.Large);
                    if (string.IsNullOrEmpty(cache))
                    {
                        var url = ImageUrls.ToPixivCat(img.Large);
                        cache = await DownloadManager.GetCache(url);
                        if (string.IsNullOrEmpty(cache))
                        {
                            cache = await DownloadManager.Download(url,
                                DownloadStartedAction: async (e) =>
                                {
                                    receivedbyte = 0;
                                    totalbyte = e.FileSize;
                                    percentage = 0;
                                    await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                },
                                DownloadProgressChangedAction: async (e) =>
                                {
                                    receivedbyte = e.ReceivedBytes;
                                    totalbyte = e.TotalBytes;
                                    percentage = e.DownloadPercentage;
                                    await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                },
                                DownloadFileCompletedAction: async (e) =>
                                {
                                    if (e.Status == DownloadTaskStatus.Completed)
                                    {
                                        receivedbyte = totalbyte;
                                        percentage = 100;
                                        await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                    }
                                }, UpdateInfoDelay: 2000);
                            if (string.IsNullOrEmpty(cache))
                            {
                                cache = await DownloadManager.Download(img.Large, detail.Url,
                                    DownloadStartedAction: async (e) =>
                                    {
                                        receivedbyte = 0;
                                        totalbyte = e.FileSize;
                                        percentage = 0;
                                        await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                    },
                                    DownloadProgressChangedAction: async (e) =>
                                    {
                                        receivedbyte = e.ReceivedBytes;
                                        totalbyte = e.TotalBytes;
                                        percentage = e.DownloadPercentage;
                                        await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                    },
                                    DownloadFileCompletedAction: async (e) =>
                                    {
                                        if (e.Status == DownloadTaskStatus.Completed)
                                        {
                                            receivedbyte = totalbyte;
                                            percentage = 100;
                                            await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                        }
                                    }, UpdateInfoDelay: 2000);
                                if (string.IsNullOrEmpty(cache))
                                {
                                    failed++;
                                    await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                                    continue;
                                }
                            }
                        }
                    }
                    ImageUtils.LimitImageScale(cache, 2500, 2500);
                    streams[current] = CopyFile(cache);
                    successed++;
                    await UpdateInfo(info, current + 1, total, receivedbyte, totalbyte, percentage, successed, failed);
                }
                for (int i = 0; i < total; i++)
                {
                    var stream = streams[i];
                    if (stream == null)
                        await ReplyAsync($"[图像 pixiv-{id}_p{i}.png 下载失败]");
                    else
                        await Context.Channel.SendFileAsync(stream, $"pixiv-{id}_p{i}.png");
                }
            }
            try
            {
                await info.DeleteAsync();
            }
            catch
            { }
            try
            {
                await message.DeleteAsync();
            }
            catch
            { }
        }

        [Command("pixiv")]
        public async Task GetIllustDetail(params int[] ids)
        {
            foreach (var id in ids)
            {
                var message = await ReplyAsync($"``数据检索中…… Pixiv ID: {id}``");
                var detail = await Illust.Get(id);
                if (detail == null)
                    await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试");
                else
                {
                    var sb = new StringBuilder()
                        .AppendLine(detail.Title)
                        .AppendLine($"Author: {detail.Author}")
                        .AppendLine(detail.Caption)
                        .AppendLine($"Tags: {string.Join(" | ", detail.Tags)}")
                        .AppendLine($"Publish Date: {detail.CreateDate:yyyy-MM-dd HH:mm:ss}")
                        .AppendLine($"Bookmarks: {detail.TotalBookmarks} Comments:{detail.TotalComments} Views:{detail.TotalView}")
                        .Append(detail.Url);
                    await message.ModifyAsync(x => x.Content = sb.ToString());
                    await SendImage(detail);
                }
            }
        }

        [Command("pixivpic")]
        public async Task GetIllustWithNoDetail(params int[] ids)
        {
            foreach (var id in ids)
            {
                var message = await ReplyAsync($"``Pixiv ID: {id} 数据检索中……``");
                var detail = await Illust.Get(id);
                if (detail == null)
                    await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试");
                else
                {
                    await SendImage(detail, message);
                }
            }
        }

        static Stream CopyFile(string path)
        {
            var stream = new MemoryStream();
            using var file = File.OpenRead(path);
            var buffer = new byte[4096];
            int osize;
            while ((osize = file.Read(buffer, 0, buffer.Length)) > 0)
                stream.Write(buffer, 0, osize);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}