using Ritsukage.Library.Pixiv.Model;
using Sora.Entities.CQCodes;
using System.Collections;

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
    }
}
