using Ritsukage.Library.Graphic;
using Ritsukage.Library.Graphic.GifGenerator;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.Segment;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Ritsukage.Library.Graphic.GraphicEdit;
using static Ritsukage.Library.Graphic.GraphicUtils;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Gif Generator")]
    public static class GifGenerator
    {
        private static async Task<string> GetImageUrl(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            return (await e.SoraApi.GetImage(imglist.First().ImgFile)).url;
        }

        private static async Task<Image<Rgba32>> DownloadNormalImage(string url)
        {
            var path = await DownloadManager.Download(url, enableAria2Download: true);
            var img = LoadImage(path);
            if (img != null) return img;
            ConsoleLog.Debug(nameof(GifGenerator),
                $"图像加载失败{Environment.NewLine}Url\t: {url}{Environment.NewLine}Path\t: {path}");
            throw new FileLoadException("图像加载失败");
        }

        private static async Task<Image<Rgba32>> DownloadGifImage(string url)
        {
            var path = await DownloadManager.Download(url, enableAria2Download: true);
            var decoder = FindDecoder(ImageFormat.Gif);
            var img = LoadImage(path, decoder);
            if (img != null) return img;
            ConsoleLog.Debug(nameof(GifGenerator),
                $"Gif图像加载失败{Environment.NewLine}Url\t: {url}{Environment.NewLine}Path\t: {path}");
            throw new FileLoadException("Gif图像加载失败");
        }

        private static async Task SendGif(SoraMessage e, Image<Rgba32> gif)
        {
            var file = Path.GetTempFileName();
            SaveImage(gif, ImageFormat.Gif, file);
            await e.Reply(SoraSegment.Image(file));
        }

        private static Func<Image<Rgba32>, Image<Rgba32>> Stack(Func<Image<Rgba32>, Image<Rgba32>> func1,
            Func<Image<Rgba32>, Image<Rgba32>> func2)
        {
            return (gif) => func2.Invoke(func1.Invoke(gif));
        }

        private static async Task<Image<Rgba32>> GetNormalImage(SoraMessage e)
        {
            var url = await GetImageUrl(e);
            if (url is null) return null;
            return await DownloadNormalImage(url);
        }

        private static async Task<Image<Rgba32>> GetGifImage(SoraMessage e)
        {
            var url = await GetImageUrl(e);
            if (url is null) return null;
            return await DownloadGifImage(url);
        }

        private static Image<Rgba32> ImageWorker(Image<Rgba32> image, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            image.FixGifFrameData();
            return func.Invoke(image);
        }

        private static async Task Worker(SoraMessage e, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            try
            {
                var image = await GetGifImage(e);
                if (image is null)
                {
                    await e.ReplyToOriginal("未检测到任何图片");
                    return;
                }

                await e.ReplyToOriginal("请稍后");
                var result = ImageWorker(image, func);
                await SendGif(e, result);
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
        {
            await Worker(e, ReserveGifFrames);
        }

        [Command("gif左行")]
        [CommandDescription("生成往左移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveLeft(SoraMessage e)
        {
            await Worker(e, SetGifLeftMotion);
        }

        [Command("gif右行")]
        [CommandDescription("生成往右移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveRight(SoraMessage e)
        {
            await Worker(e, SetGifRightMotion);
        }

        [Command("gif上行")]
        [CommandDescription("生成往上移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveUp(SoraMessage e)
        {
            await Worker(e, SetGifUpMotion);
        }

        [Command("gif下行")]
        [CommandDescription("生成往下移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveDown(SoraMessage e)
        {
            await Worker(e, SetGifDownMotion);
        }

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
                func = func == null ? ReserveGifFrames : Stack(func, ReserveGifFrames);
            if ((rot & 0b1000) == 0b1000) //左行
                func = func == null ? SetGifLeftMotion : Stack(func, SetGifLeftMotion);
            if ((rot & 0b100) == 0b100) //右行
                func = func == null ? SetGifRightMotion : Stack(func, SetGifRightMotion);
            if ((rot & 0b10) == 0b10) //上行
                func = func == null ? SetGifUpMotion : Stack(func, SetGifUpMotion);
            if ((rot & 0b1) == 0b1) //下行
                func = func == null ? SetGifDownMotion : Stack(func, SetGifDownMotion);
            await Worker(e, func);
        }

        [Command("表情包模板")]
        [CommandDescription("获取表情包模板列表")]
        public static async void GifGeneratorTemplate(SoraMessage e, string template = null)
        {
            if (string.IsNullOrWhiteSpace(template))
            {
                var list = Generator.GetTemplateList();
                var sb = new StringBuilder();
                if (!list.Any())
                {
                    sb.Append("#当前没有可用的模板");
                }
                else
                {
                    sb.AppendLine("#当前可用模板：");
                    sb.AppendJoin(Environment.NewLine, list);
                }

                await e.Reply(sb);
                return;
            }

            try
            {
                var at = e.Message.GetAllAtList().FirstOrDefault(0);
                Image<Rgba32>? image = null;
                if (at > 10000)
                    image = await DownloadNormalImage(Utils.GetQQHeadImageUrl(at));
                else
                    image = await GetNormalImage(e);
                if (image is null)
                {
                    await e.ReplyToOriginal("请指定用来生成表情包的目标图像");
                    return;
                }

                var result = Generator.Generate(image, template);
                await SendGif(e, result);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("复活")]
        [CommandDescription("使用表情包模板“复活”生成表情")]
        public static async void GifGeneratorTemplateInner0(SoraMessage e)
        {
            GifGeneratorTemplate(e, "复活");
        }

        [Command("旋转")]
        [CommandDescription("使用表情包模板“旋转”生成表情")]
        public static async void GifGeneratorTemplateInner1(SoraMessage e)
        {
            GifGeneratorTemplate(e, "旋转");
        }

        [Command("吃")]
        [CommandDescription("使用表情包模板“吃”生成表情")]
        public static async void GifGeneratorTemplateInner2(SoraMessage e)
        {
            GifGeneratorTemplate(e, "吃");
        }
    }
}