using Ritsukage.Library.Pixiv.Model;
using Sora.Entities.CQCodes;
using System.Collections;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class Pixiv
    {
        [Command("pixiv"), NeedCoins(5)]
        public static async void GetIllustDetail(SoraMessage e, int id)
        {
            var detail = await Illust.Get(id);
            ArrayList msg = new();
            foreach (var img in detail.Images)
                msg.Add(CQCode.CQImage(ImageUrls.ToPixivCat(img.Medium)));
            msg.Add(new StringBuilder().AppendLine()
                .AppendLine(detail.Title)
                .AppendLine($"Author: {detail.Author}")
                .AppendLine(detail.Caption)
                .AppendLine($"Tags: {string.Join(" | ", detail.Tags)}")
                .AppendLine($"Publish Date: {detail.CreateDate:yyyy-MM-dd hh:mm:ss}")
                .AppendLine($"Bookmarks: {detail.TotalBookmarks} Comments:{detail.TotalComments} Views:{detail.TotalView}")
                .Append(detail.Url)
                .ToString());
            await e.Reply(msg.ToArray());
            await e.RemoveCoins(5);
        }
    }
}
