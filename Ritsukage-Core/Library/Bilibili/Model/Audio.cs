using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class Audio
    {
        #region 属性
        /// <summary>
        /// 音频Id
        /// </summary>
        public int Id;
        /// <summary>
        /// 原视频AV号
        /// </summary>
        public int AV;
        /// <summary>
        /// 原视频BV号
        /// </summary>
        public string BV;
        /// <summary>
        /// 原视频弹幕池号
        /// </summary>
        public int CID;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId;
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName;
        /// <summary>
        /// 作者
        /// </summary>
        public string Author;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 封面
        /// </summary>
        public string CoverUrl;
        /// <summary>
        /// 简介
        /// </summary>
        public string Intro;
        /// <summary>
        /// 歌词
        /// </summary>
        public string Lyric;
        /// <summary>
        /// 上传时间
        /// </summary>
        public DateTime UploadTime;
        /// <summary>
        /// 长度
        /// </summary>
        public TimeSpan Duration;
        /// <summary>
        /// 投币数
        /// </summary>
        public int Coin;
        /// <summary>
        /// 数据
        /// </summary>
        public AudioStatistic Statistic;

        public string Url { get => "https://www.bilibili.com/audio/au" + Id; }
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public Video GetOriginalVideo() => Video.Get(BV);

        public string BaseToString()
            => new StringBuilder()
            .AppendLine(Title + "    作者：" + Author + "    时长：" + Duration.ToString())
            .AppendLine(Intro)
            .AppendLine("原视频：" + BV)
            .AppendLine($"播放：{Statistic.Play}  收藏：{Statistic.Collect}  评论：{Statistic.Comment}  分享：{Statistic.Share}")
            .AppendLine("发布时间：" + UploadTime.ToString("yyyy-MM-dd HH:mm:ss"))
            .Append(Url)
            .ToString();
        public override string ToString()
            => new StringBuilder()
            .AppendLine(CoverUrl)
            .Append(BaseToString())
            .ToString();
        #endregion

        #region 构造
        public static Audio Get(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://www.bilibili.com/audio/music-service-c/web/song/info?sid=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["msg"]);
            return GetByJson(info["data"]);
        }
        public static Audio GetByJson(JToken data)
        {
            ConsoleLog.Debug("Bilibili",
                new StringBuilder("[Audio Info Parser] Parser: ")
                .AppendLine().Append(data.ToString()).ToString());
            return new Audio()
            {
                Id = (int)data["id"],
                AV = (int)data["aid"],
                BV = (string)data["bvid"],
                CID = (int)data["cid"],
                UserId = (int)data["uid"],
                UserName = (string)data["uname"],
                Author = (string)data["author"],
                Title = (string)data["title"],
                CoverUrl = (string)data["cover"],
                Intro = (string)data["intro"],
                Lyric = (string)data["lyric"],
                UploadTime = Utils.GetDateTime((long)data["passtime"]),
                Duration = new TimeSpan(0, 0, (int)data["duration"]),
                Coin = (int)data["coin_num"],
                Statistic = new AudioStatistic()
                {
                    Id = (int)data["statistic"]["sid"],
                    Play = (int)data["statistic"]["play"],
                    Collect = (int)data["statistic"]["collect"],
                    Comment = (int)data["statistic"]["comment"],
                    Share = (int)data["statistic"]["share"],
                },
            };
        }
        #endregion
    }

    public struct AudioStatistic
    {
        /// <summary>
        /// 音频ID
        /// </summary>
        public int Id;
        /// <summary>
        /// 播放数
        /// </summary>
        public int Play;
        /// <summary>
        /// 收藏数
        /// </summary>
        public int Collect;
        /// <summary>
        /// 评论数
        /// </summary>
        public int Comment;
        /// <summary>
        /// 分享数
        /// </summary>
        public int Share;
    }
}
