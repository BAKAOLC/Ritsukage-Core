using Ritsukage.Library.Graphic;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.Segment;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static Ritsukage.Library.Graphic.GraphicDataDefinition;
using static Ritsukage.Library.Graphic.ImageEdit;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Image Edit"), NeedCoins(5)]
    public static class ImageEdit
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

        static async Task<string[]> GetImageUrls(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count() <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return null;
            }
            await e.ReplyToOriginal("请稍后");
            return imglist.Select(async x => (await e.SoraApi.GetImage(x.ImgFile)).url).Select(x => x.Result).ToArray();
        }

        static async Task<Stream> DownloadImage(string url)
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
            return stream;
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
            return ms;
        }

        static async Task SendImage(SoraMessage e, Image<Rgba32> image, IImageFormat format)
        {
            var file = Path.GetTempFileName();
            SaveImage(image, format, file);
            await e.Reply(SoraSegment.Image(file));
            await e.RemoveCoins(5);
        }

        static async Task SendImages(SoraMessage e, Image<Rgba32>[] images, IImageFormat format)
        {
            string[] paths = new string[images.Length];
            for (int i = 0; i < images.Length; i++)
            {
                var file = Path.GetTempFileName();
                SaveImage(images[i], format, file);
                paths[i] = file;
            }
            await e.Reply(paths.Select(x => SoraSegment.Image(x)).ToArray());
            await e.RemoveCoins(5);
        }

        static async Task Worker(SoraMessage e, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            try
            {
                var url = await GetImageUrl(e);
                if (url == null)
                    return;
                var stream = await DownloadImage(url);
                var image = LoadImage(stream, out IImageFormat format);
                var product = func.Invoke(image);
                await SendImage(e, product, format);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("镜像左")]
        [CommandDescription("修改为左右对称的图像（左侧镜像到右侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorLeft(SoraMessage e)
            => await Worker(e, MirrorLeft);

        [Command("镜像右")]
        [CommandDescription("修改为左右对称的图像（右侧镜像到左侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorRight(SoraMessage e)
            => await Worker(e, MirrorRight);

        [Command("镜像上")]
        [CommandDescription("修改为上下对称的图像（上侧镜像到下侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorTop(SoraMessage e)
            => await Worker(e, MirrorTop);

        [Command("镜像下")]
        [CommandDescription("修改为上下对称的图像（下侧镜像到上侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorBottom(SoraMessage e)
            => await Worker(e, MirrorBottom);

        [Command("反色")]
        [CommandDescription("修改为反色的图像")]
        [ParameterDescription(1, "图像")]
        public static async void WorkReserve(SoraMessage e)
            => await Worker(e, x => x.ColorReverse());

        [Command("灰度化")]
        [CommandDescription("修改为灰度化(基于比例混合算法)的图像")]
        [ParameterDescription(1, "图像")]
        public static async void WorkGraying(SoraMessage e)
            => await Worker(e, x => x.ColorGraying());

        [Command("外围消除")]
        [CommandDescription("将图像指定范围外的像素修改为透明色")]
        [ParameterDescription(1, "范围(<=0时取图像短轴作为半径范围)")]
        [ParameterDescription(1, "图像")]
        public static async void WorkFillCircleOutRangeColor(SoraMessage e, int size = 0)
            => await Worker(e, x => FillCircleOutRangeColor(x, size, TransparentColor));

        [Command("马赛克")]
        [CommandDescription("修改为马赛克处理后的图像")]
        [ParameterDescription(1, "马赛克大小")]
        [ParameterDescription(2, "像素取值偏移X")]
        [ParameterDescription(3, "像素取值偏移Y")]
        [ParameterDescription(4, "图像")]
        public static async void WorkMosaic(SoraMessage e, int size = 2, int px = 0, int py = 0)
        {
            try
            {
                var url = await GetImageUrl(e);
                if (url == null)
                    return;
                var stream = await DownloadImage(url);
                var image = LoadImage(stream, out IImageFormat format);
                var product = Mosaic(image, size, px, py);
                await SendImage(e, product, format);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("生成旋转图")]
        [CommandDescription("生成原图像大小的旋转图")]
        [ParameterDescription(1, "单次旋转周期内图像重复次数")]
        [ParameterDescription(2, "单帧时长（n*0.01s）（提供动图时此参数无效）")]
        [ParameterDescription(3, "图像")]
        public static async void WorkGenerateRotateImageWithOriginalSize(SoraMessage e, int repeat = 1, int frameDelay = 1)
        {
            try
            {
                if (repeat < 1)
                {
                    await e.ReplyToOriginal("Repeat值不可小于1");
                }
                else if (frameDelay < 1)
                {
                    await e.ReplyToOriginal("Frame Delay值不可小于1");
                }
                else
                {
                    var url = await GetImageUrl(e);
                    if (url == null)
                        return;
                    var stream = await DownloadImage(url);
                    var image = LoadImage(stream);
                    var product = GenerateRotateImageWithOriginalSize(image, repeat, frameDelay);
                    await SendImage(e, product, GifFormat.Instance);
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("生成完整旋转图")]
        [CommandDescription("生成整个图像能完整显示的旋转图")]
        [ParameterDescription(1, "单次旋转周期内图像重复次数")]
        [ParameterDescription(2, "单帧时长（n*0.01s）（提供动图时此参数无效）")]
        [ParameterDescription(3, "图像")]
        public static async void WorkGenerateRotateImage(SoraMessage e, int repeat = 1, int frameDelay = 1)
        {
            try
            {
                if (repeat < 1)
                {
                    await e.ReplyToOriginal("Repeat值不可小于1");
                }
                else if (frameDelay < 1)
                {
                    await e.ReplyToOriginal("Frame Delay值不可小于1");
                }
                else
                {
                    var url = await GetImageUrl(e);
                    if (url == null)
                        return;
                    var stream = await DownloadImage(url);
                    var image = LoadImage(stream);
                    var product = GenerateRotateImage(image, repeat, frameDelay);
                    await SendImage(e, product, GifFormat.Instance);
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("合并九图")]
        [CommandDescription("合并九图")]
        [ParameterDescription(1, "图像")]
        [ParameterDescription(2, "图像")]
        [ParameterDescription(3, "图像")]
        [ParameterDescription(4, "图像")]
        [ParameterDescription(5, "图像")]
        [ParameterDescription(6, "图像")]
        [ParameterDescription(7, "图像")]
        [ParameterDescription(8, "图像")]
        [ParameterDescription(9, "图像")]
        public static async void WorkMergeNinePicture(SoraMessage e)
        {
            try
            {
                var urls = await GetImageUrls(e);
                if (urls == null || urls.Length == 0)
                    return;
                else if (urls.Length != 9)
                    await e.ReplyToOriginal("需要九张图");
                var imgs = new Image<Rgba32>[9];
                for (int i = 0; i < 9; i++)
                {
                    var stream = await DownloadImage(urls[i]);
                    imgs[i] = LoadImage(stream, out IImageFormat format);
                }
                var product = MergeNinePicture(imgs);
                if (product != null)
                    await SendImage(e, product, PngFormat.Instance);
                else
                    await e.ReplyToOriginal("暂不支持合并图像大小不一致的九图图像");
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }

        [Command("拆分九图")]
        [CommandDescription("拆分九图")]
        [ParameterDescription(1, "图像")]
        public static async void WorkCropNinePicture(SoraMessage e)
        {
            try
            {
                var url = await GetImageUrl(e);
                if (url == null)
                    return;
                var stream = await DownloadImage(url);
                var image = LoadImage(stream, out IImageFormat format);
                if (image.Width % 3 == 0 || image.Height % 3 == 0)
                {
                    var imgs = SplitNinePicture(image);
                    await SendImages(e, imgs, format);
                }
                else
                    await e.ReplyToOriginal("暂不支持拆分非3的倍数宽高的图像");
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }
    }
}
