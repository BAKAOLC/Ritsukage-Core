using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Ritsukage.Library.Bilibili.Model
{
    public class Video
    {
        #region 属性
        /// <summary>
        /// AV号
        /// </summary>
        public long AV;
        /// <summary>
        /// BV号
        /// </summary>
        public string BV;
        /// <summary>
        /// 弹幕池CID
        /// </summary>
        public long CID;
        /// <summary>
        /// 封面Url
        /// </summary>
        public string PicUrl;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 简介
        /// </summary>
        public string Desc;
        /// <summary>
        /// 视频数量
        /// </summary>
        public int Count;
        /// <summary>
        /// 视频总长度
        /// </summary>
        public TimeSpan Duration;
        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PubDate;
        /// <summary>
        /// 版权所有
        /// </summary>
        public bool CopyRight;
        /// <summary>
        /// 分区ID
        /// </summary>
        public int AreaId;
        /// <summary>
        /// 分区名称
        /// </summary>
        public string AreaName;
        /// <summary>
        /// 用户ID
        /// </summary>
        public int UserId;
        /// <summary>
        /// 用户名称
        /// </summary>
        public string UserName;
        /// <summary>
        /// 用户头像Url
        /// </summary>
        public string UserFaceUrl;
        /// <summary>
        /// 视频数据统计
        /// </summary>
        public VideoStatistic Statistic;
        /// <summary>
        /// 视频分P
        /// </summary>
        public VideoPage[] Pages;

        public string Url { get => "https://www.bilibili.com/video/" + BV; }
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public override string ToString()
            => PicUrl + "\n"
                + Title + "    av" + AV + "  " + BV + "\n"
                + "UP：" + UserName + $"(https://space.bilibili.com/{UserId})" + "\n"
                + (string.IsNullOrEmpty(AreaName) ? "" : ("分区：" + AreaName + "\n"))
                + $"播放量：{Statistic.View} 弹幕：{Statistic.Danmaku} 评论：{Statistic.Reply}" + "\n"
                + $" 收藏：{Statistic.Favorite} 投币：{Statistic.Coin} 分享：{Statistic.Share} 点赞：{Statistic.Like}" + "\n"
                + "发布时间：" + PubDate.ToString("yyyy-MM-dd hh:mm:ss") + "\n"
                + Desc;
        #endregion

        #region 构造
        public static Video Get(long av)
        {
            var info = JObject.Parse(Utils.HttpGET("http://api.bilibili.com/x/web-interface/view?aid=" + av));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            return GetByJson(info["data"]);
        }
        public static Video Get(string bv)
        {
            var info = JObject.Parse(Utils.HttpGET("http://api.bilibili.com/x/web-interface/view?bvid=" + bv));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            return GetByJson(info["data"]);
        }
        public static Video GetByJson(JToken data)
        {
            var video = new Video()
            {
                AV = (long)data["aid"],
                BV = (string)data["bvid"],
                CID = (long)data["cid"],
                PicUrl = (string)data["pic"],
                Title = (string)data["title"],
                Desc = RemoveEmptyLine(((string)data["desc"]).Replace("<br/>", "\n").Replace("\r", "")),
                Count = (int)data["videos"],
                PubDate = DateTimeOffset.FromUnixTimeSeconds((long)data["pubdate"]).LocalDateTime,
                CopyRight = (int)data["copyright"] == 1 ? true : false,
                AreaId = (int)data["tid"],
                AreaName = (string)data["tname"],
                Duration = new TimeSpan(0, 0, (int)data["duration"]),
                UserId = (int)data["owner"]["mid"],
                UserName = (string)data["owner"]["name"],
                UserFaceUrl = (string)data["owner"]["face"],
            };
            video.Statistic = new VideoStatistic()
            {
                Id = (long)data["stat"]["aid"],
                View = (int)data["stat"]["view"],
                Danmaku = (int)data["stat"]["danmaku"],
                Reply = (int)data["stat"]["reply"],
                Favorite = (int)data["stat"]["favorite"],
                Coin = (int)data["stat"]["coin"],
                Share = (int)data["stat"]["share"],
                Like = (int)data["stat"]["like"],
            };
            var videos = (JArray)data["pages"];
            if (videos != null)
            {
                video.Pages = new VideoPage[videos.Count];
                for (var i = 0; i < videos.Count; i++)
                    video.Pages[i] = new VideoPage()
                    {
                        CID = (long)videos[i]["cid"],
                        Index = (int)videos[i]["page"],
                        Name = (string)videos[i]["part"],
                        Duration = new TimeSpan(0, 0, (int)videos[i]["duration"]),
                    };
            }
            return video;
        }
        #endregion

        #region 静态方法
        static string RemoveEmptyLine(string text)
            => string.Join("", text.Split("\n", StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(x => x).Select(x => x.Key).ToArray());
        #endregion
    }

    public struct VideoStatistic
    {
        /// <summary>
        /// 视频ID
        /// </summary>
        public long Id;
        /// <summary>
        /// 播放量
        /// </summary>
        public int View;
        /// <summary>
        /// 弹幕数
        /// </summary>
        public int Danmaku;
        /// <summary>
        /// 评论数
        /// </summary>
        public int Reply;
        /// <summary>
        /// 收藏数
        /// </summary>
        public int Favorite;
        /// <summary>
        /// 投币数
        /// </summary>
        public int Coin;
        /// <summary>
        /// 分享数
        /// </summary>
        public int Share;
        /// <summary>
        /// 点赞数
        /// </summary>
        public int Like;
    }

    public struct VideoPage
    {
        /// <summary>
        /// 弹幕池CID
        /// </summary>
        public long CID;
        /// <summary>
        /// 分P编号
        /// </summary>
        public int Index;
        /// <summary>
        /// 分P名称
        /// </summary>
        public string Name;
        /// <summary>
        /// 视频长度
        /// </summary>
        public TimeSpan Duration;
    }

    public static class VideoExtensions
    {
        public static string PutCoin(long av, string cookie)
        {
            string jct = Bilibili.GetJCT(cookie);
            string param = string.Join("&",
                "aid=" + av,
                "multiply=1",
                "select_like=1",
                "cross_domain=true",
                "csrf=" + jct);
            var data = JObject.Parse(Utils.HttpPOST("https://api.bilibili.com/x/web-interface/coin/add", param, 5000, cookie,
                "https://www.bilibili.com/video/av" + av));
            if ((int)data["code"] == 0)
                return "投币成功";
            else
                throw new Exception((string)data["message"]);
        }
        public static string PutCoin(this Video video, string cookie)
            => PutCoin(video.AV, cookie);
    }
}
