using Ritsukage.Library.Netease.CloudMusic;
using Ritsukage.Tools;
using Sora.Entities.CQCodes;
using System.Text;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Music")]
    public static class NeteaseCloudMusic
    {
        [Command("music")]
        [CommandDescription("搜索曲目")]
        [ParameterDescription(1, "关键词", "接口来自 https://music.163.com")]
        public static async void Search(SoraMessage e, string keyword)
        {
            var search = await CloudMusicApi.SearchSong(SoraMessage.Escape(keyword));
            if (search != null && search.Length > 0)
                Play(e, search[0].Id);
            else
                await e.ReplyToOriginal("未搜索到相关结果");
        }

        [Command("music")]
        [CommandDescription("播放指定曲目", "接口来自 https://music.163.com")]
        [ParameterDescription(1, "歌曲ID")]
        public static async void Play(SoraMessage e, long id)
        {
            var detail = await CloudMusicApi.GetSongDetail(id);
            if (detail == null)
                await e.ReplyToOriginal("曲目信息获取失败");
            else
            {
                var url = await CloudMusicApi.GetSongUrl(id, 128000);
                if (url.Id == detail.Id && url.Id == id)
                {
                    await e.Reply(CQCode.CQImage(await DownloadManager.Download(detail.Album.GetPicUrl(512, 512), enableSimpleDownload: true)),
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
                    await e.Reply(CQCode.CQImage(await DownloadManager.Download(detail.Album.GetPicUrl(512, 512), enableSimpleDownload: true)),
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