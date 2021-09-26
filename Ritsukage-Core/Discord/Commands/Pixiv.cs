using Discord;
using Discord.Commands;
using Ritsukage.Library.Graphic;
using Ritsukage.Library.Pixiv.Extension;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Pixiv : ModuleBase<SocketCommandContext>
    {
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
                    if (detail.IsUgoira)
                    {
                        var ugoira = await detail.GetUgoira();
                        if (ugoira == null)
                            await ReplyAsync($"动图数据(pid:{id})获取失败");
                        else
                        {
                            var img = await ugoira.LimitGifScale(350, 350);
                            var stream = await img.SaveGifToStream();
                            await Context.Channel.SendFileAsync(stream, $"pixiv-{id}.gif");
                        }
                    }
                    else
                    {
                        Stream[] streams = new Stream[detail.Images.Length];
                        int i = 0;
                        foreach (var img in detail.Images)
                        {
                            var cache = await DownloadManager.GetCache(img.Medium);
                            if (string.IsNullOrEmpty(cache))
                            {
                                var url = ImageUrls.ToPixivCat(img.Medium);
                                cache = await DownloadManager.GetCache(url);
                                if (string.IsNullOrEmpty(cache))
                                {
                                    cache = await DownloadManager.Download(url);
                                    if (string.IsNullOrEmpty(cache))
                                    {
                                        cache = await DownloadManager.Download(img.Medium, detail.Url);
                                        if (string.IsNullOrEmpty(cache))
                                        {
                                            i++;
                                            continue;
                                        }
                                    }
                                }
                            }
                            ImageUtils.LimitImageScale(cache, 1500, 1500);
                            streams[i++] = CopyFile(cache);
                            i++;
                        }
                        for (i = 0; i < streams.Length; i++)
                        {
                            var stream = streams[i];
                            if (stream == null)
                                await ReplyAsync($"[图像 pixiv-{id}_p{i}.png 下载失败]");
                            else
                                await Context.Channel.SendFileAsync(stream, $"pixiv-{id}_p{i}.png");
                        }
                    }
                }
            }
        }

        [Command("pixivpic")]
        public async Task GetIllustWithNoDetail(params int[] ids)
        {
            foreach (var id in ids)
            {
                var message = await ReplyAsync($"``数据检索中…… Pixiv ID: {id}``");
                var detail = await Illust.Get(id);
                if (detail == null)
                    await message.ModifyAsync(x => x.Content = $"数据(pid:{id})获取失败，请稍后再试");
                else
                {
                    if (detail.IsUgoira)
                    {
                        var ugoira = await detail.GetUgoira();
                        if (ugoira == null)
                            await ReplyAsync($"动图数据(pid:{id})获取失败");
                        else
                        {
                            var img = await ugoira.LimitGifScale(350, 350);
                            var stream = await img.SaveGifToStream();
                            await Context.Channel.SendFileAsync(stream, $"pixiv-{id}.gif");
                        }
                    }
                    else
                    {
                        Stream[] streams = new Stream[detail.Images.Length];
                        int i = 0;
                        foreach (var img in detail.Images)
                        {
                            var cache = await DownloadManager.GetCache(img.Medium);
                            if (string.IsNullOrEmpty(cache))
                            {
                                var url = ImageUrls.ToPixivCat(img.Medium);
                                cache = await DownloadManager.GetCache(url);
                                if (string.IsNullOrEmpty(cache))
                                {
                                    cache = await DownloadManager.Download(url);
                                    if (string.IsNullOrEmpty(cache))
                                    {
                                        cache = await DownloadManager.Download(img.Medium, detail.Url);
                                        if (string.IsNullOrEmpty(cache))
                                        {
                                            i++;
                                            continue;
                                        }
                                    }
                                }
                            }
                            ImageUtils.LimitImageScale(cache, 1500, 1500);
                            streams[i++] = CopyFile(cache);
                        }
                        for (i = 0; i < streams.Length; i++)
                        {
                            var stream = streams[i];
                            if (stream == null)
                                await ReplyAsync($"[图像 pixiv-{id}_p{i}.png 下载失败]");
                            else
                                await Context.Channel.SendFileAsync(stream, $"pixiv-{id}_p{i}.png");
                        }
                    }
                    await message.DeleteAsync();
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