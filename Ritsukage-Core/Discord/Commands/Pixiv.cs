using Discord;
using Discord.Commands;
using Ritsukage.Library.Graphic;
using Ritsukage.Library.Pixiv.Extension;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Pixiv : ModuleBase<SocketCommandContext>
    {
        static readonly string TitleContent = "``Pixiv ID: {0} 基础数据已获取，开始获取图像内容……``";
        static readonly string InfoContent = "``Current: {0} / {1} Successed: {2}  Failed: {3}``";

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

        static readonly Queue<int> SendImageTaskQueue = new();

        async Task SendImage(Illust detail, IUserMessage message = null)
        {
            int id = detail.Id;
            SendImageTaskQueue.Enqueue(id);
            await Task.Run(async () =>
            {
                while (SendImageTaskQueue.Peek() != id)
                    await Task.Delay(1000);
            });
            await Task.Run(async () =>
            {
                if (message == null)
                    message = await ReplyAsync(string.Format(TitleContent, id));
                else
                    await UpdateMessage(message, string.Format(TitleContent, id));
                var info = await ReplyAsync("``== loading ==``");
                int total = detail.Images.Length;
                int successed = 0, failed = 0;
                int current = -1;
                await UpdateInfo(info, current + 1, total, successed, failed);
                if (detail.IsUgoira)
                {
                    current++;
                    await UpdateInfo(info, current + 1, total, successed, failed);
                    var ugoira = await detail.GetUgoira();
                    if (ugoira == null)
                        await UpdateInfo(message, $"动图数据(pid: {id})获取失败");
                    else
                    {
                        var img = await ugoira.LimitGifScale(350, 350);
                        var stream = await img.SaveGifToStream();
                        stream = await GIFsicle.Compress(stream);
                        try
                        {
                            await Context.Channel.SendFileAsync(stream, $"pixiv-{id}.gif");
                        }
                        catch (Exception ex)
                        {
                            await Context.Channel.SendMessageAsync($"gif发送失败 pid:{id} \n {ex.Message}");
                        }
                    }
                }
                else
                {
                    string[] streams = new string[total];
                    foreach (var img in detail.Images)
                    {
                        current++;
                        var cache = await DownloadManager.GetCache(img.Original);
                        if (string.IsNullOrEmpty(cache))
                        {
                            var url = ImageUrls.ToPixivCat(img.Original);
                            cache = await DownloadManager.GetCache(url);
                            if (string.IsNullOrEmpty(cache))
                            {
                                await UpdateInfo(info, current + 1, total, successed, failed);
                                cache = await DownloadManager.Download(url);
                                if (string.IsNullOrEmpty(cache))
                                {
                                    cache = await DownloadManager.Download(img.Original, detail.Url);
                                    if (string.IsNullOrEmpty(cache))
                                    {
                                        failed++;
                                        await UpdateInfo(info, current + 1, total, successed, failed);
                                        continue;
                                    }
                                }
                            }
                        }
                        ImageUtils.LimitImageScale(cache, 2500, 2500);
                        streams[current] = cache;
                        successed++;
                        await UpdateInfo(info, current + 1, total, successed, failed);
                    }
                    for (int i = 0; i < total; i++)
                    {
                        Stream stream = null;
                        try
                        {
                            stream = CopyFile(streams[i]);
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Discord Pixiv", ex.GetFormatString());
                        }
                        if (stream == null)
                            await ReplyAsync($"[图像 pixiv-{id}_p{i}.png 下载失败]");
                        else
                        {
                            try
                            {
                                await Context.Channel.SendFileAsync(stream, $"pixiv-{id}_p{i}.png");
                            }
                            catch (Exception ex)
                            {
                                await Context.Channel.SendMessageAsync($"图像发送失败 pid:{id} p{i} \n {ex.Message}");
                            }
                        }
                        await Task.Delay(1000);
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
            });
            SendImageTaskQueue.Dequeue();
        }

        [Command("pixiv")]
        public async Task GetIllustDetail(params int[] ids)
        {
            foreach (var id in ids)
            {
                var message = await ReplyAsync($"``数据检索中…… Pixiv ID: {id}``");
                try
                {
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
                catch (Exception ex)
                {
                    await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试\n{ex.Message}");
                }
            }
        }

        [Command("pixivpic")]
        public async Task GetIllustWithNoDetail(params int[] ids)
        {
            foreach (var id in ids)
            {
                var message = await ReplyAsync($"``Pixiv ID: {id} 数据检索中……``");
                try
                {
                    var detail = await Illust.Get(id);
                    if (detail == null)
                        await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试");
                    else
                    {
                        await SendImage(detail, message);
                    }
                }
                catch (Exception ex)
                {
                    await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试\n{ex.Message}");
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