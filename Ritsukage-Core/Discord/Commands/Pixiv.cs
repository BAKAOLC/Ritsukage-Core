using Discord.Commands;
using Ritsukage.Library.Pixiv.Model;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class Pixiv : ModuleBase<SocketCommandContext>
    {
        [Command("pixiv")]
        public async Task GetIllustDetail(int id)
        {
            if (await Context.User.CheckCoins(5))
            {
                var msg = await ReplyAsync("``数据检索中……``");
                var detail = await Illust.Get(id);
                if (detail == null)
                    await msg.ModifyAsync(x => x.Content = "数据获取失败，请稍后再试");
                else
                {
                    StringBuilder sb = new();
                    foreach (var img in detail.Images)
                        sb.AppendLine(ImageUrls.ToPixivCat(img.Medium));
                    sb.AppendLine(detail.Title)
                    .AppendLine($"Author: {detail.Author}")
                    .AppendLine(detail.Caption)
                    .AppendLine($"Tags: {string.Join(" | ", detail.Tags)}")
                    .AppendLine($"Publish Date: {detail.CreateDate:yyyy-MM-dd hh:mm:ss}")
                    .AppendLine($"Bookmarks: {detail.TotalBookmarks} Comments:{detail.TotalComments} Views:{detail.TotalView}")
                    .Append(detail.Url);
                    await msg.ModifyAsync(x => x.Content = msg.ToString());
                    await Context.User.RemoveCoins(5);
                }
            }
            else
                await ReplyAsync("幻币数量不足");
        }
    }
}