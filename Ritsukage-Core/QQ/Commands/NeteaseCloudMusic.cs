using Ritsukage.Library.Netease.CloudMusic;
using Sora.Entities.CQCodes;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup]
    public static class NeteaseCloudMusic
    {
        [Command("music")]
        public static async void Search(SoraMessage e, string keyword)
        {
            var search = await CloudMusicApi.SearchSong(keyword);
            if (search != null && search.Length > 0)
                Play(e, search[0].Id);
            else
                await e.AutoAtReply("未搜索到相关结果");
        }

        [Command("music")]
        public static async void Play(SoraMessage e, long id)
        {
            var detail = await CloudMusicApi.GetSongDetail(id);
            if (detail == null)
                await e.AutoAtReply("曲目信息获取失败");
            else
            {
                var url = await CloudMusicApi.GetSongUrl(id);
                if (url.Id == detail.Id && url.Id == id)
                {
                    await e.Reply(CQCode.CQImage(detail.Album.GetPicUrl(512, 512)),
                        new StringBuilder().AppendLine()
                        .AppendLine("♬ " + detail.Name)
                        .AppendLine("✎ " + string.Join(" / ", detail.Artists))
                        .AppendLine(detail.Url)
                        .Append("√ 曲目链接已解析，正在下载中……")
                        .ToString());
                    await e.Reply(CQCode.CQRecord(url.Url));
                }
                else
                {
                    await e.Reply(CQCode.CQImage(detail.Album.GetPicUrl(512, 512)),
                        new StringBuilder().AppendLine()
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