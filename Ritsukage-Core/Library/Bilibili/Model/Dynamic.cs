using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Linq;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class Dynamic
    {
        #region 属性
        public ulong Id;
        public int UserId;
        public string UserName;
        public string UserFaceUrl;
        public int View;
        public int Repost;
        public int Reply { get => Card == null ? 0 : Card.Reply; }
        public int Like;
        public DateTime Time;
        public string[] Pictures
        {
            get
            {
                if (OriginalCard != null)
                {
                    if (OriginalCard.Content.Pictures != null)
                        return OriginalCard.Content.Pictures;
                    else if (OriginalCard.Video != null)
                        return new[] { OriginalCard.Video.PicUrl };
                    else if (OriginalCard.Audio != null)
                        return new[] { OriginalCard.Audio.CoverUrl };
                }
                else
                {
                    if (Card.Content.Pictures != null)
                        return Card.Content.Pictures;
                    else if (Card.Video != null)
                        return new[] { Card.Video.PicUrl };
                    else if (Card.Audio != null)
                        return new[] { Card.Audio.CoverUrl };
                }
                return Array.Empty<string>();
            }
        }
        public DynamicCard Card;
        public DynamicCard OriginalCard = null;

        public string Url { get => "https://t.bilibili.com/" + Id; }
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public Dynamic GetOriginal() => OriginalCard == null ? null : Get(OriginalCard.Id);

        public string GetInfo()
            => new StringBuilder()    
            .AppendLine($"动态UP：{UserName}(UID:{UserId})")    
            .AppendLine("发布时间：" + Time.ToString("yyyy-MM-dd HH:mm:ss"))    
            .Append($"查看数：{View}  转发：{Repost}  评论：{Reply}  点赞：{Like}")
            .ToString();
        public string BaseToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetInfo());
            if (OriginalCard == null)
                sb.AppendLine(Card.BaseToString());
            else if (Card.Vote == null)
            {
                sb.AppendLine(Card.BaseToString());
                sb.AppendLine(OriginalCard.BaseToString());
            }
            else
            {
                sb.AppendLine(Card.BaseToStringWithNoVote());
                sb.AppendLine(OriginalCard.BaseToString());
            }
            sb.Append(Url);
            return sb.ToString();
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine(GetInfo());
            if (OriginalCard == null)
                sb.AppendLine(Card.ToString());
            else if (Card.Vote == null)
            {
                sb.AppendLine(Card.ToString());
                sb.AppendLine(OriginalCard.ToString());
            }
            else
            {
                sb.AppendLine(Card.ToStringWithNoVote());
                sb.AppendLine(OriginalCard.ToString());
            }
            sb.Append(Url);
            return sb.ToString();
        }
        #endregion

        #region 构造
        public static Dynamic Get(ulong id)
        {
            var data = JObject.Parse(Utils.HttpGET("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/get_dynamic_detail?dynamic_id=" + id));
            if ((string)data["message"] != "")
                throw new Exception((string)data["message"]);
            return GetByJson(data["data"]["card"]);
        }
        public static Dynamic GetByJson(JToken data)
        {
            var card = JObject.Parse((string)data["card"]);
            var dynamic = new Dynamic()
            {
                Id = (ulong)data["desc"]["dynamic_id"],
                UserId = (int)data["desc"]["user_profile"]["info"]["uid"],
                UserName = (string)data["desc"]["user_profile"]["info"]["uname"],
                UserFaceUrl = (string)data["desc"]["user_profile"]["info"]["face"],
                View = (int)data["desc"]["view"],
                Repost = (int)data["desc"]["repost"],
                Like = (int)data["desc"]["like"],
                Time = Utils.GetDateTime((long)data["desc"]["timestamp"]),
            };
            #region 动态自身解析
            dynamic.Card = new DynamicCard()
            {
                Id = dynamic.Id,
                Type = DynamicCardType.Normal,
                OwnerName = dynamic.UserName,
            };
            if (card["item"] != null) //相簿 & 普通动态
            {
                dynamic.Card.Reply = (int)card["item"]["reply"];
                if (card["item"]["pictures"] != null)
                {
                    var pics = (JArray)card["item"]["pictures"];
                    dynamic.Card.Content = new DynamicContent()
                    {
                        Text = (string)card["item"]["description"],
                        Pictures = new string[pics.Count],
                    };
                    for (var i = 0; i < pics.Count; i++)
                        dynamic.Card.Content.Pictures[i] = (string)pics[i]["img_src"];
                }
                else if (card["item"]["content"] != null)
                    dynamic.Card.Content = new DynamicContent() { Text = (string)card["item"]["content"] };
                else
                    dynamic.Card.Content = new DynamicContent() { Text = (string)card["item"]["description"] };
            }
            else if (card["duration"] != null) //视频
            {
                dynamic.Card.Type = DynamicCardType.Video;
                dynamic.Card.Reply = (int)card["stat"]["reply"];
                dynamic.Card.Content = new DynamicContent() { Text = (string)card["dynamic"] };
                dynamic.Card.Video = Video.GetByJson(card);
            }
            else if (card["words"] != null) //专栏
            {
                dynamic.Card.Type = DynamicCardType.Article;
                dynamic.Card.Reply = (int)card["stats"]["reply"];
                dynamic.Card.Content = new DynamicContent() { Text = (string)card["dynamic"] };
                dynamic.Card.Article = Article.Get((int)card["id"]);
            }
            else if (card["intro"] != null) //音频
            {
                dynamic.Card.Type = DynamicCardType.Audio;
                dynamic.Card.Reply = (int)card["replyCnt"];
                dynamic.Card.Content = new DynamicContent() { Text = (string)card["intro"] };
                dynamic.Card.Audio = Audio.GetByJson(card);
            }
            if (data["extension"] != null && data["extension"]["vote"] != null) //投票
            {
                dynamic.Card.Type = DynamicCardType.Vote;
                dynamic.Card.Vote = Vote.Get((int)data["extension"]["vote_cfg"]["vote_id"]);
            }
            #endregion
            #region 源动态解析
            if (card["origin"] != null)
            {
                var original = JObject.Parse((string)card["origin"]);
                dynamic.OriginalCard = new DynamicCard()
                {
                    Id = ulong.Parse((string)data["desc"]["pre_dy_id_str"]),
                    Type = DynamicCardType.Normal,
                    IsForwarded = true,
                };
                if (original["item"] != null) //相簿 & 普通动态
                {
                    dynamic.OriginalCard.Reply = (int)original["item"]["reply"];
                    if (original["item"]["pictures"] != null)
                    {
                        dynamic.OriginalCard.OwnerName = (string)original["user"]["name"];
                        var pics = (JArray)original["item"]["pictures"];
                        dynamic.OriginalCard.Content = new DynamicContent()
                        {
                            Text = (string)original["item"]["description"],
                            Pictures = new string[pics.Count],
                        };
                        for (var i = 0; i < pics.Count; i++)
                            dynamic.OriginalCard.Content.Pictures[i] = (string)pics[i]["img_src"];
                    }
                    else
                    {
                        dynamic.OriginalCard.OwnerName = (string)original["user"]["uname"];
                        if (original["item"]["content"] != null)
                            dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["item"]["content"] };
                        else
                            dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["item"]["description"] };
                    }
                }
                else if (original["duration"] != null) //视频
                {
                    dynamic.OriginalCard.OwnerName = (string)original["owner"]["name"];
                    dynamic.OriginalCard.Type = DynamicCardType.Video;
                    dynamic.OriginalCard.Reply = (int)original["stat"]["reply"];
                    dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["dynamic"] };
                    dynamic.OriginalCard.Video = Video.GetByJson(original);
                }
                else if (original["words"] != null) //专栏
                {
                    dynamic.OriginalCard.OwnerName = (string)original["author"]["name"];
                    dynamic.OriginalCard.Type = DynamicCardType.Article;
                    dynamic.OriginalCard.Reply = (int)original["stats"]["reply"];
                    dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["dynamic"] };
                    dynamic.OriginalCard.Article = Article.Get((int)original["id"]);
                }
                else if (original["intro"] != null) //音频
                {
                    dynamic.OriginalCard.OwnerName = (string)original["upper"];
                    dynamic.OriginalCard.Type = DynamicCardType.Audio;
                    dynamic.OriginalCard.Reply = (int)original["replyCnt"];
                    dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["intro"] };
                    dynamic.OriginalCard.Audio = Audio.GetByJson(original);
                }
                if (dynamic.Card.Type == DynamicCardType.Vote) //投票
                {
                    dynamic.OriginalCard.Type = DynamicCardType.Vote;
                    dynamic.OriginalCard.Vote = dynamic.Card.Vote;
                }
            }
            #endregion
            return dynamic;
        }
        #endregion

        #region 静态方法
        public static Dynamic[] GetDynamicList(int uid, ulong offset = 0)
        {
            var data = JObject.Parse(Utils.HttpGET("https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/space_history?host_uid=" + uid + "&offset_dynamic_id=" + offset));
            var list = (JArray)data["data"]["cards"];
            if (list == null)
                return null;
            var result = new Dynamic[list.Count];
            for (var i = 0; i < list.Count; i++)
                result[i] = GetByJson(list[i]);
            return result;
        }
        #endregion
    }

    public class DynamicCard
    {
        /// <summary>
        /// 动态ID
        /// </summary>
        public ulong Id;
        /// <summary>
        /// 发送者昵称
        /// </summary>
        public string OwnerName;
        /// <summary>
        /// 回复数
        /// </summary>
        public int Reply;
        /// <summary>
        /// 动态类型
        /// </summary>
        public DynamicCardType Type;
        /// <summary>
        /// 视频
        /// </summary>
        public Video Video;
        /// <summary>
        /// 专栏
        /// </summary>
        public Article Article;
        /// <summary>
        /// 音频
        /// </summary>
        public Audio Audio;
        /// <summary>
        /// 投票
        /// </summary>
        public Vote Vote;
        /// <summary>
        /// 正文
        /// </summary>
        public DynamicContent Content;

        public bool IsForwarded = false;

        public string BaseToStringWithNoVote()
        {
            var sb = new StringBuilder();
            sb.Append((IsForwarded ? "//@" + OwnerName + "：" : "") + (Content != null ? Content.BaseToString() : ""));
            if (Video != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了视频：");
                sb.Append(Video.BaseToString());
            }
            if (Article != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了专栏：");
                sb.Append(Article.ToString());
            }
            if (Audio != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了音频：");
                sb.Append(Audio.BaseToString());
            }
            return sb.ToString();
        }
        public string BaseToString()
        {
            var sb = new StringBuilder(BaseToStringWithNoVote());
            if (Vote != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发起了投票：");
                sb.Append(Vote.BaseToString());
            }
            return sb.ToString();
        }
        public string ToStringWithNoVote()
        {
            var sb = new StringBuilder();
            sb.Append((IsForwarded ? "//@" + OwnerName + "：" : "") + (Content != null ? Content.ToString() : ""));
            if (Video != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了视频：");
                sb.Append(Video.ToString());
            }
            if (Article != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了专栏：");
                sb.Append(Article.ToString());
            }
            if (Audio != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发布了音频：");
                sb.Append(Audio.ToString());
            }
            return sb.ToString();
        }
        public override string ToString()
        {
            var sb = new StringBuilder(ToStringWithNoVote());
            if (Vote != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发起了投票：");
                sb.Append(Vote.ToString());
            }
            return sb.ToString();
        }
    }

    public class DynamicContent
    {
        /// <summary>
        /// 正文
        /// </summary>
        public string Text;
        /// <summary>
        /// 图像
        /// </summary>
        public string[] Pictures;

        public string BaseToString() => Text;
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(Text);
            if (Pictures != null)
            {
                foreach (var pic in Pictures)
                {
                    sb.AppendLine();
                    sb.Append(pic);
                }
            }
            return sb.ToString();
        }
    }

    public enum DynamicCardType
    {
        /// <summary>
        /// 普通动态 / 相簿
        /// </summary>
        Normal,
        /// <summary>
        /// 视频
        /// </summary>
        Video,
        /// <summary>
        /// 专栏
        /// </summary>
        Article,
        /// <summary>
        /// 音频
        /// </summary>
        Audio,
        /// <summary>
        /// 投票
        /// </summary>
        Vote,
    }

    public static class DynamicExtensions
    {
        public static string Like(ulong id, bool like, string cookie)
        {
            string jct = Bilibili.GetJCT(cookie);
            if (int.TryParse(cookie.Split(";").Where(x => x.StartsWith("DedeUserID=")).First().Substring(11), out var uid))
            {
                string param = string.Join("&",
                    "uid=" + uid,
                    "dynamic_id=" + id,
                    "up=" + (like ? 1 : 2),
                    "csrf_token=" + jct);
                var data = JObject.Parse(Utils.HttpPOST("https://api.vc.bilibili.com/dynamic_like/v1/dynamic_like/thumb", param, 5000, cookie));
                if ((int)data["code"] == 0)
                    return "操作执行成功";
                else
                    throw new Exception((string)data["message"]);
            }
            throw new Exception("异常的账户数据记录，请重新登录");
        }
        public static string Like(this Dynamic dynamic, bool like, string cookie)
            => Like(dynamic.Id, like, cookie);
    }
}
