using Discord.Commands;
using Ritsukage.Library.Netease.CloudMusic;
using Ritsukage.Tools;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Discord.Commands
{
    public class NeteaseCloudMusic : ModuleBase<SocketCommandContext>
    {
        [Command("music")]
        public async Task Search(string keyword)
        {
            var search = await CloudMusicApi.SearchSong(keyword);
            if (search != null && search.Length > 0)
                await Play(search[0].Id);
            else
                await ReplyAsync("未搜索到相关结果");
        }

        [Command("music")]
        public async Task Play(long id)
        {
            var msg = await ReplyAsync("曲目搜索中……");
            var detail = await CloudMusicApi.GetSongDetail(id);
            if (detail == null)
                await msg.ModifyAsync(x => x.Content = "曲目信息获取失败");
            else
            {
                var url = await CloudMusicApi.GetSongUrl(id);
                if (url.Id == detail.Id && url.Id == id)
                {
                    await msg.ModifyAsync(x => x.Content = new StringBuilder()
                    .AppendLine(detail.Album.GetPicUrl(512, 512))    
                    .AppendLine("♬ " + detail.Name)    
                    .AppendLine("✎ " + string.Join(" / ", detail.Artists))    
                    .AppendLine(detail.Url)    
                    .Append("√ 曲目链接：" + url.Url)    
                    .ToString());
                }
                else
                {
                    await msg.ModifyAsync(x => x.Content = new StringBuilder()
                    .AppendLine(detail.Album.GetPicUrl(512, 512))
                    .AppendLine("♬ " + detail.Name)
                    .AppendLine("✎ " + string.Join(" / ", detail.Artists))
                    .AppendLine(detail.Url)
                    .Append("× 解析曲目链接失败")
                    .ToString());
                }
            }
        }
    }
}