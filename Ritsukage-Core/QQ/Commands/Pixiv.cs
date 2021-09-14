using Ritsukage.Library.Pixiv.Extension;
using Ritsukage.Library.Pixiv.Model;
using Ritsukage.Tools.Console;
using Sora.Entities.CQCodes;
using System;
using System.Collections;
using System.IO;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Pixiv")]
    public static class Pixiv
    {
        [Command("pixiv"), NeedCoins(5)]
        [CommandDescription("获取指定Pixiv作品信息")]
        [ParameterDescription(1, "Issust ID", "接口来自 https://github.com/mixmoe/HibiAPI")]
        public static async void GetIllustDetail(SoraMessage e, int id)
        {
            try
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
            catch (Exception ex)
            {
                ConsoleLog.Debug("QQ Command - Pixiv", ex.GetFormatString(true));
                await e.ReplyToOriginal("发生异常错误，任务已终止");
            }
        }

        [Command("pixivu"), OnlyForSuperUser]
        [CommandDescription("获取指定Pixiv Ugoira")]
        [ParameterDescription(1, "Issust ID", "接口来自 https://github.com/mixmoe/HibiAPI")]
        public static async void GetIllustUgoira(SoraMessage e, int id)
        {
            try
            {
                var detail = await Illust.Get(id);
                if (detail == null)
                    await e.ReplyToOriginal("数据获取失败，请稍后再试");
                else
                {
                    await e.ReplyToOriginal("数据获取中，请稍后");
                    var ugoira = await detail.GetUgoira();
                    if (ugoira == null)
                    {
                        await e.ReplyToOriginal("目标作品并非动图");
                    }
                    else
                    {
                        await e.ReplyToOriginal("数据获取成功，正在进行压缩...");
                        var img = await ugoira.LimitGifScale(500, 500);
                        var file = await img.SaveGifToTempFile();
                        await e.Reply(CQCode.CQImage(file));
                    }
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Debug("QQ Command - Pixiv", ex.GetFormatString(true));
                await e.ReplyToOriginal("发生异常错误，任务已终止");
            }
        }
    }
}
