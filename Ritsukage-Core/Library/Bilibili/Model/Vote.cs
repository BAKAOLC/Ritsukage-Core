using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class Vote
    {
        #region 属性
        /// <summary>
        /// Id
        /// </summary>
        public int Id;
        /// <summary>
        /// 发起者Id
        /// </summary>
        public int UserId;
        /// <summary>
        /// 发起者昵称
        /// </summary>
        public string UserName;
        /// <summary>
        /// 发起者头像
        /// </summary>
        public string UserFaceUrl;
        /// <summary>
        /// 标题
        /// </summary>
        public string Title;
        /// <summary>
        /// 说明
        /// </summary>
        public string Desc;
        /// <summary>
        /// 可选数量
        /// </summary>
        public int ChooseNumber;
        /// <summary>
        /// 参与人数
        /// </summary>
        public int Join;
        /// <summary>
        /// 结束时间
        /// </summary>
        public DateTime EndTime;
        /// <summary>
        /// 投票选项
        /// </summary>
        public VoteOption[] Options;
        #endregion

        #region 方法
        public string BaseToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("投票：" + Title);
            if (!string.IsNullOrWhiteSpace(Desc))
                sb.AppendLine(Desc);
            sb.AppendLine("参与人数：" + Join);
            sb.AppendLine("截止时间：" + EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append("投票选项" + (ChooseNumber == 1 ? "(单选)" : $"(多选，最多可选择{ChooseNumber}项)："));
            for (var i = 0; i < Options.Length; i++)
            {
                sb.AppendLine();
                sb.Append("    * " + Options[i].BaseToString());
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("投票：" + Title);
            if (!string.IsNullOrWhiteSpace(Desc))
                sb.AppendLine(Desc);
            sb.AppendLine("参与人数：" + Join);
            sb.AppendLine("截止时间：" + EndTime.ToString("yyyy-MM-dd HH:mm:ss"));
            sb.Append("投票选项" + (ChooseNumber == 1 ? "(单选)" : $"(多选，最多可选择{ChooseNumber}项)："));
            for (var i = 0; i < Options.Length; i++)
            {
                sb.AppendLine();
                sb.Append("    * " + Options[i].ToString());
            }
            return sb.ToString();
        }
        #endregion

        #region 构造
        public static Vote Get(int id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.vc.bilibili.com/vote_svr/v1/vote_svr/vote_info?vote_id=" + id));
            if ((int)info["code"] != 0)
                throw new NullReferenceException($"投票id{id}不存在");
            /*
            ConsoleLog.Debug("Bilibili",
                new StringBuilder("[Vote Info Parser] Parser: ")
                .AppendLine().Append(info["data"].ToString()).ToString());
            */
            var vote = new Vote()
            {
                Id = (int)info["data"]["info"]["vote_id"],
                UserId = (int)info["data"]["info"]["uid"],
                UserName = (string)info["data"]["info"]["name"],
                UserFaceUrl = (string)info["data"]["info"]["face"],
                Title = (string)info["data"]["info"]["title"],
                Desc = (string)info["data"]["info"]["desc"],
                ChooseNumber = (int)info["data"]["info"]["choice_cnt"],
                Join = (int)info["data"]["info"]["cnt"],
                EndTime = Utils.GetDateTime((long)info["data"]["info"]["endtime"]),
            };
            var options = (JArray)info["data"]["info"]["options"];
            vote.Options = new VoteOption[options.Count];
            for (var i = 0; i < options.Count; i++)
                vote.Options[i] = new VoteOption()
                {
                    Id = (int)options[i]["idx"],
                    Desc = (string)options[i]["desc"],
                    ImageUrl = options[i]["img_url"] == null ? "" : (string)options[i]["img_url"],
                };
            return vote;
        }

        public static Vote CreateNullVote(int id)
            => new Vote()
            {
                Id = id,
                UserId = 0,
                UserName = "未知",
                Title = "投票已删除或不存在",
                ChooseNumber = 1,
                Join = 0,
                EndTime = DateTime.MinValue,
                Options = Array.Empty<VoteOption>()
            };
        #endregion
    }

    public struct VoteOption
    {
        /// <summary>
        /// 选项编号
        /// </summary>
        public int Id;
        /// <summary>
        /// 选项说明
        /// </summary>
        public string Desc;
        /// <summary>
        /// 获得票数
        /// </summary>
        public int Count;
        /// <summary>
        /// 图像链接
        /// </summary>
        public string ImageUrl;

        public string BaseToString() => Desc;
        public string BaseToStringWithCount() => Desc + $"  ({Count}票)";
        public override string ToString() => BaseToString() + (string.IsNullOrWhiteSpace(ImageUrl) ? "" : ("    " + ImageUrl));
        public string ToStringWithCount() => BaseToStringWithCount() + (string.IsNullOrWhiteSpace(ImageUrl) ? "" : ("    " + ImageUrl));
    }
}
