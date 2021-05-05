using AnimatedGif;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools.Console;
using Ritsukage.Tools.Download;
using Ritsukage.Tools.Zip;
using Sora.Entities.CQCodes;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Pixiv")]
    public static class Pixiv
    {
        [Command("pixiv"), NeedCoins(5)]
        public static async void GetIllustDetail(SoraMessage e, int id)
        {
            var detail = await Illust.Get(id);
            if (detail == null)
                await e.ReplyToOriginal("数据获取失败，请稍后再试");
            else
            {
                ArrayList msg = new();
                foreach (var img in detail.Images)
                    msg.Add(CQCode.CQImage(ImageUrls.ToPixivCat(img.Medium)));
                msg.Add(detail.ToString());
                await e.ReplyToOriginal("数据获取中，请稍后");
                await e.Reply(msg.ToArray());
                await e.RemoveCoins(5);
            }
        }

        [Command("pixivug"), NeedCoins(5)]
        public static async void GetIllustUgoira(SoraMessage e, int id)
        {
            var detail = await Illust.Get(id);
            if (detail == null)
                await e.ReplyToOriginal("数据获取失败，请稍后再试");
            else
            {
                var meta = await detail.GetUgoiraMetadata();
                var task = new DownloadTask(meta.ZipUrl)
                {
                    Referer = detail.Url
                };
                task.OnTaskWaiting += (sender) =>
                {
                    Task.Run(() => e.Reply($"开始下载 Pixiv Illust {id} 的数据文件"));
                };
                task.OnTaskError += (sender, result) =>
                {
                    Task.Run(() => e.Reply(new StringBuilder()
                        .AppendLine($"Pixiv Illust {id} 数据文件下载失败：")
                        .Append(result.Exception.GetFormatString())
                        .ToString()));
                };
                task.OnTaskFinish += (sender, result) =>
                {
                    Task.Run(async () =>
                    {
                        await e.Reply($"Pixiv Illust {id} 数据文件下载完毕，开始合成动图");
                        using var zip = ZipPackage.OpenStream(new MemoryStream(result.DownloadBytes));
                        using var gifStream = new MemoryStream();
                        using var gif = new AnimatedGifCreator(gifStream);
                        foreach (var frame in meta.Frames)
                        {
                            var stream = zip.GetFileStream(frame.File);
                            await gif.AddFrameAsync(Image.FromStream(stream), frame.Delay);
                        }
                        gif.Dispose();
                        gifStream.Seek(0, SeekOrigin.Begin);
                        var file = Path.GetTempFileName();
                        await File.WriteAllBytesAsync(file, gifStream.ToArray());
                        await e.Reply(CQCode.CQImage("file:///" + file));
                        await e.RemoveCoins(5);
                    });
                };
                task.Start();
            }
        }
    }
}
