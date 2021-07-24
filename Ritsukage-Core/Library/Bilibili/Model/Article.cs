using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class Article
    {
        #region 属性
        public int Id;
        public int UserId;
        public string UserName;
        public string Title;
        public int Coin;
        public ArticleStatistic Statistic;

        public string Url { get => "https://www.bilibili.com/read/cv" + Id; }
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public override string ToString()
            => new StringBuilder()
            .AppendLine(Title)
            .AppendLine("作者：" + UserName + $"(UID:{UserId})")
            .AppendLine($"观看：{Statistic.View}  收藏：{Statistic.Favorite}  点赞：{Statistic.Like}")
            .AppendLine($"投币：{Statistic.Coin}  评论：{Statistic.Reply}  分享：{Statistic.Share}")
            .Append(Url)
            .ToString();
        #endregion

        #region 构造
        public static Article Get(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/article/viewinfo?id=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["msg"]);
            /*
            ConsoleLog.Debug("Bilibili",
                new StringBuilder("[Article Info Parser] Parser: ")
                .AppendLine().Append(info["data"].ToString()).ToString());
            */
            return new Article()
            {
                Id = id,
                UserId = (int)info["data"]["mid"],
                UserName = (string)info["data"]["author_name"],
                Title = (string)info["data"]["title"],
                Coin = (int)info["data"]["coin"],
                Statistic = new ArticleStatistic()
                {
                    View = (int)info["data"]["stats"]["view"],
                    Favorite = (int)info["data"]["stats"]["favorite"],
                    Like = (int)info["data"]["stats"]["like"],
                    Reply = (int)info["data"]["stats"]["reply"],
                    Share = (int)info["data"]["stats"]["share"],
                    Coin = (int)info["data"]["stats"]["coin"],
                    Dynamic = (int)info["data"]["stats"]["dynamic"],
                },
            };
        }
        #endregion
    }

    public struct ArticleStatistic
    {
        /// <summary>
        /// 观看数
        /// </summary>
        public int View;
        /// <summary>
        /// 收藏数
        /// </summary>
        public int Favorite;
        /// <summary>
        /// 点赞数
        /// </summary>
        public int Like;
        /// <summary>
        /// 评论数
        /// </summary>
        public int Reply;
        /// <summary>
        /// 分享数
        /// </summary>
        public int Share;
        /// <summary>
        /// 投币数
        /// </summary>
        public int Coin;
        /// <summary>
        /// 动态数
        /// </summary>
        public int Dynamic;
    }
}
