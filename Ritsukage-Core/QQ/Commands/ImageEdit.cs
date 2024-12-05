using Ritsukage.Library.Graphic;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using Sora.Entities.Segment;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sora.Entities;
using Sora.Enumeration;
using static Ritsukage.Library.Graphic.GraphicEdit;
using static Ritsukage.Library.Graphic.GraphicUtils;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration.ApiType;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Image Edit")]
    public static class ImageEdit
    {
        private static async Task<string[]> GetReplyImageUrls(SoraMessage e)
        {
            var replySegment = e.Message.MessageBody.FirstOrDefault(x => x.MessageType == SegmentType.Reply);
            if (replySegment == null)
            {
                await e.ReplyToOriginal("未检测到任何图片").ConfigureAwait(false);
                return [];
            }

            if (replySegment.Data is not ReplySegment reply)
            {
                await e.ReplyToOriginal("检测到回复消息字段，但未能解析").ConfigureAwait(false);
                return [];
            }

            var (apiStatus, message, _, _, _, _) = await e.SoraApi.GetMessage(reply.Target).ConfigureAwait(false);

            if (apiStatus.RetCode != ApiStatusType.Ok)
            {
                await e.ReplyToOriginal("获取回复消息失败").ConfigureAwait(false);
                return [];
            }

            var imglist = message.GetAllImage();
            if (!imglist.Any())
            {
                await e.ReplyToOriginal("未检测到任何图片").ConfigureAwait(false);
                return [];
            }

            await e.ReplyToOriginal("请稍后").ConfigureAwait(false);
            return imglist.Select(async x => (await e.SoraApi.GetImage(x.ImgFile)).url).Select(x => x.Result).ToArray();
        }

        private static async Task<string> GetImageUrl(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (!imglist.Any())
            {
                //await e.ReplyToOriginal("未检测到任何图片");
                var replyImageUrls = await GetReplyImageUrls(e);
                return replyImageUrls.Length == 0 ? null : replyImageUrls.FirstOrDefault();
            }

            await e.ReplyToOriginal("请稍后");
            return (await e.SoraApi.GetImage(imglist.First().ImgFile)).url;
        }

        private static async Task<string[]> GetImageUrls(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (!imglist.Any())
            {
                //await e.ReplyToOriginal("未检测到任何图片");
                var replyImageUrls = await GetReplyImageUrls(e);
                return replyImageUrls.Length == 0 ? null : replyImageUrls;
            }

            await e.ReplyToOriginal("请稍后");
            return imglist.Select(async x => (await e.SoraApi.GetImage(x.ImgFile)).url).Select(x => x.Result).ToArray();
        }

        private static async Task<string> DownloadImage(string url)
        {
            return await DownloadManager.Download(url, enableAria2Download: true);
        }

        private static async Task SendImage(SoraMessage e, Image<Rgba32> image, IImageFormat format)
        {
            var file = Path.GetTempFileName();
            SaveImage(image, format, file);
            await e.Reply(SoraSegment.Image(file));
        }

        private static async Task SendImages(SoraMessage e, Image<Rgba32>[] images, IImageFormat format)
        {
            var paths = new string[images.Length];
            for (var i = 0; i < images.Length; i++)
            {
                var file = Path.GetTempFileName();
                SaveImage(images[i], format, file);
                paths[i] = file;
            }

            await e.Reply(paths.Select(x => SoraSegment.Image(x)).ToArray());
        }

        private static async Task Worker(SoraMessage e, Func<Image<Rgba32>, Image<Rgba32>> func)
        {
            try
            {
                var url = await GetImageUrl(e);
                if (url == null)
                    return;
                var path = await DownloadImage(url);
                var image = LoadImage(path, out var format);
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
        {
            await Worker(e, MirrorLeft);
        }

        [Command("镜像右")]
        [CommandDescription("修改为左右对称的图像（右侧镜像到左侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorRight(SoraMessage e)
        {
            await Worker(e, MirrorRight);
        }

        [Command("镜像上")]
        [CommandDescription("修改为上下对称的图像（上侧镜像到下侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorTop(SoraMessage e)
        {
            await Worker(e, MirrorTop);
        }

        [Command("镜像下")]
        [CommandDescription("修改为上下对称的图像（下侧镜像到上侧）")]
        [ParameterDescription(1, "图像")]
        public static async void WorkMirrorBottom(SoraMessage e)
        {
            await Worker(e, MirrorBottom);
        }

        [Command("反色")]
        [CommandDescription("修改为反色的图像")]
        [ParameterDescription(1, "图像")]
        public static async void WorkReserve(SoraMessage e)
        {
            await Worker(e, x => x.ColorReverse());
        }

        [Command("灰度化")]
        [CommandDescription("修改为灰度化(基于比例混合算法)的图像")]
        [ParameterDescription(1, "图像")]
        public static async void WorkGraying(SoraMessage e)
        {
            await Worker(e, x => x.ColorGraying());
        }

        [Command("边缘检测")]
        [CommandDescription("基于Sobel算子进行边缘检测")]
        [ParameterDescription(1, "图像")]
        public static async void WorkDetectEdges(SoraMessage e)
        {
            await Worker(e, x => x.DetectEdges());
        }

        [Command("外围消除")]
        [CommandDescription("将图像指定范围外的像素修改为透明色")]
        [ParameterDescription(1, "范围(<=0时取图像短轴作为半径范围)")]
        [ParameterDescription(1, "图像")]
        public static async void WorkFillCircleOutRangeColor(SoraMessage e, int size = 0)
        {
            await Worker(e, x => FillCircleOutRangeColor(x, size, TransparentColor));
        }

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
                var path = await DownloadImage(url);
                var image = LoadImage(path, out var format);
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
        public static async void WorkGenerateRotateImageWithOriginalSize(SoraMessage e, int repeat = 1,
            int frameDelay = 1)
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
                    var path = await DownloadImage(url);
                    var image = LoadImage(path);
                    var product = GenerateRotateImageWithOriginalSize(image, repeat, frameDelay);
                    await SendImage(e, product, ImageFormat.Gif);
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
                    var path = await DownloadImage(url);
                    var image = LoadImage(path);
                    var product = GenerateRotateImage(image, repeat, frameDelay);
                    await SendImage(e, product, ImageFormat.Gif);
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
                for (var i = 0; i < 9; i++)
                {
                    var path = await DownloadImage(urls[i]);
                    imgs[i] = LoadImage(path);
                }

                var product = MergeNinePicture(imgs);
                if (product != null)
                    await SendImage(e, product, ImageFormat.Default);
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
                var path = await DownloadImage(url);
                var image = LoadImage(path, out var format);
                if (image.Width % 3 == 0 || image.Height % 3 == 0)
                {
                    var imgs = SplitNinePicture(image);
                    await SendImages(e, imgs, format);
                }
                else
                {
                    await e.ReplyToOriginal("暂不支持拆分非3的倍数宽高的图像");
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Image Edit", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.Message);
            }
        }
    }
}