using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.Entities.CQCodes;
using System;
using System.IO;
using System.Linq;
using static Ritsukage.Library.Graphic.GifEdit;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Gif Generator"), NeedCoins(5)]
    public static class GifGenerator
    {
        [Command("gif倒流")]
        [CommandDescription("生成倒序播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void Reverse(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return;
            }
            await e.ReplyToOriginal("请稍后");
            try
            {
                var imgdata = await e.SoraApi.GetImage(imglist.First().ImgFile);
                var stream = await Utils.GetFileAsync(imgdata.url);
                var gif = ReadGif(stream);
                var reverse = CreateReverse(gif);
                var file = Path.GetTempFileName();
                SaveGif(reverse, file);
                await e.Reply(CQCode.CQImage(file));
                await e.RemoveCoins(5);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.GetFormatString());
            }
        }

        [Command("gif左行")]
        [CommandDescription("生成往左移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveLeft(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return;
            }
            await e.ReplyToOriginal("请稍后");
            try
            {
                var imgdata = await e.SoraApi.GetImage(imglist.First().ImgFile);
                var stream = await Utils.GetFileAsync(imgdata.url);
                var gif = ReadGif(stream);
                var reverse = CreateMoveLeft(gif);
                var file = Path.GetTempFileName();
                SaveGif(reverse, file);
                await e.Reply(CQCode.CQImage(file));
                await e.RemoveCoins(5);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.GetFormatString());
            }
        }

        [Command("gif右行")]
        [CommandDescription("生成往右移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveRight(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return;
            }
            await e.ReplyToOriginal("请稍后");
            try
            {
                var imgdata = await e.SoraApi.GetImage(imglist.First().ImgFile);
                var stream = await Utils.GetFileAsync(imgdata.url);
                var gif = ReadGif(stream);
                var reverse = CreateMoveRight(gif);
                var file = Path.GetTempFileName();
                SaveGif(reverse, file);
                await e.Reply(CQCode.CQImage(file));
                await e.RemoveCoins(5);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.GetFormatString());
            }
        }

        [Command("gif上行")]
        [CommandDescription("生成往上移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveUp(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return;
            }
            await e.ReplyToOriginal("请稍后");
            try
            {
                var imgdata = await e.SoraApi.GetImage(imglist.First().ImgFile);
                var stream = await Utils.GetFileAsync(imgdata.url);
                var gif = ReadGif(stream);
                var reverse = CreateMoveUp(gif);
                var file = Path.GetTempFileName();
                SaveGif(reverse, file);
                await e.Reply(CQCode.CQImage(file));
                await e.RemoveCoins(5);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.GetFormatString());
            }
        }

        [Command("gif下行")]
        [CommandDescription("生成往下移动播放的gif")]
        [ParameterDescription(1, "图像")]
        public static async void MoveDown(SoraMessage e)
        {
            var imglist = e.Message.GetAllImage();
            if (imglist.Count <= 0)
            {
                await e.ReplyToOriginal("未检测到任何图片");
                return;
            }
            await e.ReplyToOriginal("请稍后");
            try
            {
                var imgdata = await e.SoraApi.GetImage(imglist.First().ImgFile);
                var stream = await Utils.GetFileAsync(imgdata.url);
                var gif = ReadGif(stream);
                var reverse = CreateMoveDown(gif);
                var file = Path.GetTempFileName();
                SaveGif(reverse, file);
                await e.Reply(CQCode.CQImage(file));
                await e.RemoveCoins(5);
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("Gif Generator", ex.GetFormatString());
                await e.ReplyToOriginal("因发生异常导致图像生成失败，", ex.GetFormatString());
            }
        }
    }
}
