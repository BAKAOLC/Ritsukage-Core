using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.Segment;
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
            if (imglist.Count() <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return null;
            }
            await e.ReplyToOriginal("请稍后");
            return (await e.SoraApi.GetImage(imglist.First().ImgFile)).url;
        }

        static async Task<Image<Rgba32>> DownloadImage(string url)
        {
            /*
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
            */
            var path = await DownloadManager.Download(url, enableAria2Download: true);
            var fs = File.OpenRead(path);
            var ms = new MemoryStream();
            var buffer = new byte[4096];
            int osize;
            while ((osize = fs.Read(buffer, 0, 4096)) > 0)
                ms.Write(buffer, 0, osize);
            fs.Close();
            fs.Dispose();
            ms.Seek(0, SeekOrigin.Begin);
            return ReadGif(ms);
        }

        static async Task SendGif(SoraMessage e, Image<Rgba32> gif)
        {
            var file = Path.GetTempFileName();
            SaveGif(gif, file);
            await e.Reply(SoraSegment.Image(file));
            await e.RemoveCoins(5);
        }

        static Func<Image<Rgba32>, Image<Rgba32>> Stack(Func<Image<Rgba32>, Image<Rgba32>> func1, Func<Image<Rgba32>, Image<Rgba32>> func2)
            => (gif) => func2.Invoke(func1.Invoke(gif));

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

        //0b10000 倒流
        //0b01000 左行
        //0b00100 右行
        //0b00010 上行
        //0b00001 下行
        [Command]
        [CommandDescription("生成gif")]
        [ParameterDescription(1, "行动模式", "0b11111 位数依次对应倒流、左、右、上、下操作")]
        [ParameterDescription(2, "图像")]
        public static async void Gif(SoraMessage e, int rot)
        {
            Func<Image<Rgba32>, Image<Rgba32>> func = null;
            if ((rot & 0b10000) == 0b10000) //倒流
                func = func == null ? CreateReverse : Stack(func, CreateReverse);
            if ((rot & 0b1000) == 0b1000) //左行
                func = func == null ? CreateMoveLeft : Stack(func, CreateMoveLeft);
            if ((rot & 0b100) == 0b100) //右行
                func = func == null ? CreateMoveRight : Stack(func, CreateMoveRight);
            if ((rot & 0b10) == 0b10) //上行
                func = func == null ? CreateMoveUp : Stack(func, CreateMoveUp);
            if ((rot & 0b1) == 0b1) //下行
                func = func == null ? CreateMoveDown : Stack(func, CreateMoveDown);
            await Worker(e, func);
        }
    }
}
