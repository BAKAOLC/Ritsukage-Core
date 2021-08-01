using Downloader;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.CQCodes;
using Sora.Enumeration.ApiType;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Ritsukage.Library.Graphic.GifEdit;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Gif Generator"), NeedCoins(5)]
    public static class GifGenerator
    {
        static async Task<string> GetImageUrl(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return null;
            }
            await e.ReplyToOriginal("请稍后");
            return (await e.SoraApi.GetImage(imglist.First().ImgFile)).url;
        }

        static async Task<Image<Rgba32>> DownloadImage(string url)
        {
            var downloader = new DownloadService(new DownloadConfiguration()
            {
                BufferBlockSize = 4096,
                ChunkCount = 5,
                OnTheFlyDownload = false,
                ParallelDownload = true
            });
            var stream = await downloader.DownloadFileTaskAsync(url);
            stream.Seek(0, SeekOrigin.Begin);
            return ReadGif(stream);
        }

        static async Task SendGif(SoraMessage e, Image<Rgba32> gif)
        {
            var file = Path.GetTempFileName();
            SaveGif(gif, file);
            await e.Reply(CQCode.CQImage(file));
            await e.RemoveCoins(5);
        }

        static async Task Worker(SoraMessage e, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            try
            {
                var url = await GetImageUrl(e);
                if (url == null)
                    return;
                var gif = await DownloadImage(url);
                var product = func.Invoke(gif);
                await SendGif(e, product);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("gif倒流")]
        [CommandDescription("生成倒序播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void Reverse(SoraMessage e)
            => await Worker(e, CreateReverse);

        [Command("gif左行")]
        [CommandDescription("生成往左移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveLeft(SoraMessage e)
            => await Worker(e, CreateMoveLeft);

        [Command("gif右行")]
        [CommandDescription("生成往右移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveRight(SoraMessage e)
            => await Worker(e, CreateMoveRight);

        [Command("gif上行")]
        [CommandDescription("生成往上移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveUp(SoraMessage e)
            => await Worker(e, CreateMoveUp);

        [Command("gif下行")]
        [CommandDescription("生成往下移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveDown(SoraMessage e)
            => await Worker(e, CreateMoveDown);
    }
}
