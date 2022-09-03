using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                    if (OriginalCard.Content != null && OriginalCard.Content.Pictures != null)
                        return OriginalCard.Content.Pictures;
                    else if (OriginalCard.Video != null)
                        return new[] { OriginalCard.Video.PicUrl };
                    else if (OriginalCard.Audio != null)
                        return new[] { OriginalCard.Audio.CoverUrl };
                }
                else
                {
                    if (Card.Content != null && Card.Content.Pictures != null)
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

        public async Task<Image<Rgba32>> GetNinePicture()
        {
            return await Task.Run(async () =>
            {
                if (Pictures == null || Pictures.Length < 9) return null;
                var imgs = new List<Image<Rgba32>>();
                var pics = await DownloadManager.Download(Pictures, enableAria2Download: true, enableSimpleDownload: true);
                foreach (var file in pics)
                {
                    if (string.IsNullOrEmpty(file)) return null;
                    var img = new FileImage(file);
                    switch (img.ImageFormatString)
                    {
                        case ".png":
                            imgs.Add(Image.Load<Rgba32>(img.GetBytes(), new SixLabors.ImageSharp.Formats.Png.PngDecoder()));
                            break;
                        case ".jpg":
                            imgs.Add(Image.Load<Rgba32>(img.GetBytes(), new SixLabors.ImageSharp.Formats.Jpeg.JpegDecoder()));
                            break;
                        case ".bmp":
                            imgs.Add(Image.Load<Rgba32>(img.GetBytes(), new SixLabors.ImageSharp.Formats.Bmp.BmpDecoder()));
                            break;
                        default:
                            return null;
                    }
                }
                var first = imgs.First();
                if (imgs.All(x => x.Width == first.Width && x.Height == first.Height))
                {
                    var result = new Image<Rgba32>(first.Width * 3, first.Height * 3);
                    for (int y = 0; y < 3; y++)
                    {
                        for (int x = 0; x < 3; x++)
                        {
                            int id = y * 3 + x;
                            int px = first.Width * x;
                            int py = first.Height * y;
                            DrawImage(ref result, px, py, imgs[id]);
                        }
                    }
                    return result;
                }
                return null;
            });
        }

        static void DrawImage(ref Image<Rgba32> image, int dx, int dy, Image<Rgba32> draw)
        {
            for (int x = 0; x < draw.Width; x++)
                for (int y = 0; y < draw.Height; y++)
                    image[x + dx, y + dy] = draw[x, y];
        }

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
                Id = ulong.Parse((string)data["desc"]["dynamic_id_str"]),
                UserId = (int)data["desc"]["user_profile"]["info"]["uid"],
                UserName = (string)data["desc"]["user_profile"]["info"]["uname"],
                UserFaceUrl = (string)data["desc"]["user_profile"]["info"]["face"],
                View = (int)data["desc"]["view"],
                Repost = (int)data["desc"]["repost"],
                Like = (int)data["desc"]["like"],
                Time = Utils.GetDateTime((long)data["desc"]["timestamp"]),
            };
            dynamic.Card = new DynamicCard()
            {
                Id = dynamic.Id,
                Type = DynamicCardType.Normal,
                OwnerName = dynamic.UserName,
            };
            #region 动态自身解析
            try
            {
                switch ((int)data["desc"]["type"])
                {
                    case 1: //转发
                    case 2: //图文
                    case 4: //纯文本
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
                            break;
                        }
                    case 8: //视频
                        {
                            dynamic.Card.Type = DynamicCardType.Video;
                            dynamic.Card.Reply = (int)card["stat"]["reply"];
                            dynamic.Card.Content = new DynamicContent() { Text = (string)card["dynamic"] };
                            dynamic.Card.Video = Video.GetByJson(card);
                            break;
                        }
                    case 64: //专栏
                        {
                            dynamic.Card.Type = DynamicCardType.Article;
                            dynamic.Card.Reply = (int)card["stats"]["reply"];
                            dynamic.Card.Content = new DynamicContent() { Text = (string)card["dynamic"] };
                            dynamic.Card.Article = Article.Get((int)card["id"]);
                            break;
                        }
                    case 256: //音频
                        {
                            dynamic.Card.Type = DynamicCardType.Audio;
                            dynamic.Card.Reply = (int)card["replyCnt"];
                            dynamic.Card.Content = new DynamicContent() { Text = (string)card["intro"] };
                            dynamic.Card.Audio = Audio.Get((int)card["id"]);
                            break;
                        }
                    case 2048: //分享
                        {
                            dynamic.Card.Type = DynamicCardType.Extra;
                            dynamic.Card.Reply = (int)data["desc"]["comment"];
                            dynamic.Card.Content = new DynamicContent() { Text = (string)card["vest"]["content"] };
                            dynamic.Card.Sketch = DynamicSketch.GetByJson(card["sketch"]);
                            break;
                        }
                };
                if (data["display"] != null && data["display"]["add_on_card_info"] != null) //额外信息
                {
                    dynamic.Card.Extend = DynamicDisplayExtendInfo.GetByJson(data["display"]["add_on_card_info"][0]);
                }
                if (data["extension"] != null)
                {
                    if (data["extension"]["vote"] != null) //投票
                    {
                        dynamic.Card.Type = DynamicCardType.Vote;
                        try
                        {
                            dynamic.Card.Vote = Vote.Get((int)data["extension"]["vote_cfg"]["vote_id"]);
                        }
                        catch (NullReferenceException)
                        {
                            dynamic.Card.Vote = Vote.CreateNullVote((int)data["extension"]["vote_cfg"]["vote_id"]);
                        }
                    }
                    if (data["extension"]["lott"] != null) //互动抽奖
                    {
                        dynamic.Card.Type = DynamicCardType.Lottery;
                        dynamic.Card.Lottery = DynamicLottery.GetByDynamicId(dynamic.Id);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error in parsing dynamic(ID: {dynamic.Id})", ex);
            }
            #endregion
            #region 源动态解析
            if (card["origin"] != null)
            {
                try
                {
                    var original = JObject.Parse((string)card["origin"]);
                    dynamic.OriginalCard = new DynamicCard()
                    {
                        Id = ulong.Parse((string)data["desc"]["orig_dy_id_str"]),
                        Type = DynamicCardType.Normal,
                        IsForwarded = true,
                    };
                    switch ((int)data["desc"]["orig_type"])
                    {
                        case 1: //转发
                        case 2: //图文
                        case 4: //纯文本
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
                                break;
                            }
                        case 8: //视频
                            {
                                dynamic.OriginalCard.OwnerName = (string)original["owner"]["name"];
                                dynamic.OriginalCard.Type = DynamicCardType.Video;
                                dynamic.OriginalCard.Reply = (int)original["stat"]["reply"];
                                dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["dynamic"] };
                                dynamic.OriginalCard.Video = Video.GetByJson(original);
                                break;
                            }
                        case 64: //专栏
                            {
                                dynamic.OriginalCard.OwnerName = (string)original["author"]["name"];
                                dynamic.OriginalCard.Type = DynamicCardType.Article;
                                dynamic.OriginalCard.Reply = (int)original["stats"]["reply"];
                                dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["dynamic"] };
                                dynamic.OriginalCard.Article = Article.Get((int)original["id"]);
                                break;
                            }
                        case 256: //音频
                            {
                                dynamic.OriginalCard.OwnerName = (string)original["upper"];
                                dynamic.OriginalCard.Type = DynamicCardType.Audio;
                                dynamic.OriginalCard.Reply = (int)original["replyCnt"];
                                dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["intro"] };
                                dynamic.OriginalCard.Audio = Audio.GetByJson(original);
                                break;
                            }
                        case 2048: //分享
                            {
                                dynamic.OriginalCard.OwnerName = (string)original["user"]["uname"];
                                dynamic.OriginalCard.Type = DynamicCardType.Extra;
                                dynamic.OriginalCard.Reply = 0;
                                dynamic.OriginalCard.Content = new DynamicContent() { Text = (string)original["vest"]["content"] };
                                dynamic.OriginalCard.Sketch = DynamicSketch.GetByJson(original["sketch"]);
                                break;
                            }
                    }
                    if (data["display"] != null && data["display"]["origin"] != null && data["display"]["origin"]["add_on_card_info"] != null) //额外信息
                    {
                        dynamic.OriginalCard.Extend = DynamicDisplayExtendInfo.GetByJson(data["display"]["origin"]["add_on_card_info"][0]);
                    }
                    if (dynamic.Card.Type == DynamicCardType.Vote) //投票
                    {
                        dynamic.OriginalCard.Type = DynamicCardType.Vote;
                        dynamic.OriginalCard.Vote = dynamic.Card.Vote;
                    }
                    if (card["origin_extension"] != null && card["origin_extension"]["lott"] != null)
                    {
                        dynamic.OriginalCard.Type = DynamicCardType.Lottery;
                        dynamic.OriginalCard.Lottery = DynamicLottery.GetByDynamicId(dynamic.OriginalCard.Id);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"Error in parsing dynamic(ID: {dynamic.Id})", ex);
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
        /// 互动抽奖
        /// </summary>
        public DynamicLottery Lottery;
        /// <summary>
        /// 正文
        /// </summary>
        public DynamicContent Content;
        /// <summary>
        /// 分享
        /// </summary>
        public DynamicSketch Sketch;
        /// <summary>
        /// 额外信息
        /// </summary>
        public DynamicDisplayExtendInfo Extend;

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
            if (Sketch != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 分享了：");
                sb.Append(Sketch.BaseToString());
            }
            if (Lottery != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发起了互动抽奖：");
                sb.Append(Lottery.ToString());
            }
            if (Extend != null)
            {
                sb.AppendLine();
                sb.Append(Extend.ToString());
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
            if (Sketch != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 分享了：");
                sb.Append(Sketch.ToString());
            }
            if (Lottery != null)
            {
                sb.AppendLine();
                sb.AppendLine("[]> 发起了互动抽奖：");
                sb.Append(Lottery.ToString());
            }
            if (Extend != null)
            {
                sb.AppendLine();
                sb.Append(Extend.ToString());
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

    public class DynamicSketch
    {
        public long Id;
        public string Title;
        public string Desc;
        public string CoverUrl;
        public string TargetUrl;

        DynamicSketch() { }

        public static DynamicSketch GetByJson(JToken data)
        {
            return new DynamicSketch()
            {
                Id = (long)data["sketch_id"],
                Title = (string)data["title"],
                Desc = (string)data["desc_text"],
                CoverUrl = (string)data["cover_url"],
                TargetUrl = (string)data["target_url"],
            };
        }

        public string BaseToString()
        {
            var sb = new StringBuilder();
            sb.Append(Title);
            if (!string.IsNullOrWhiteSpace(Desc))
                sb.AppendLine().Append(Desc);
            sb.AppendLine().Append(TargetUrl);
            return sb.ToString();
        }
        public override string ToString()
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrWhiteSpace(CoverUrl))
                sb.Append(CoverUrl).AppendLine();
            sb.Append(BaseToString());
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
        /// <summary>
        /// 互动抽奖
        /// </summary>
        Lottery,
        /// <summary>
        /// 其他
        /// </summary>
        Extra,
    }

    public class DynamicDisplayExtendInfo
    {
        public DynamicDisplayExtendInfoType Type;

        public string Title;

        public string DescFirst;

        public string DescSecond;

        public string JumpUrl;

        public string HeadText;

        public DateTime LivePlanStartTime;

        DynamicDisplayExtendInfo() { }

        public override string ToString()
        {
            var sb = new StringBuilder("[]> ");
            if (!string.IsNullOrWhiteSpace(HeadText))
                sb.Append(HeadText).Append("  ");
            sb.Append(Title);
            switch (Type)
            {
                case DynamicDisplayExtendInfoType.Reserve:
                    {
                        bool f1 = !string.IsNullOrWhiteSpace(DescFirst);
                        bool f2 = !string.IsNullOrWhiteSpace(DescSecond);
                        if (f1 || f2)
                            sb.AppendLine();
                        if (f1 && f2)
                            sb.Append(DescFirst).Append("  ").Append(DescSecond);
                        else if (f1)
                            sb.Append(DescFirst);
                        else if (f2)
                            sb.Append(DescSecond);
                        break;
                    }
                case DynamicDisplayExtendInfoType.Game:
                    {
                        if (!string.IsNullOrWhiteSpace(DescFirst))
                            sb.AppendLine().Append(DescFirst);
                        if (!string.IsNullOrWhiteSpace(DescSecond))
                            sb.AppendLine().Append(DescSecond);
                        break;
                    }
            }
            sb.AppendLine().Append(JumpUrl);
            return sb.ToString();
        }

        public static DynamicDisplayExtendInfo GetByJson(JToken data)
        {
            DynamicDisplayExtendInfo info = null;
            switch ((int)data["add_on_card_show_type"])
            {
                case 2:
                    {
                        data = data["attach_card"];
                        info = new DynamicDisplayExtendInfo()
                        {
                            Type = DynamicDisplayExtendInfoType.Game,
                            HeadText = (string)data["head_text"],
                            Title = (string)data["title"],
                            DescFirst = (string)data["desc_first"],
                            DescSecond = (string)data["desc_second"],
                            JumpUrl = (string)data["jump_url"],
                        };
                        break;
                    }
                case 6:
                    {
                        data = data["reserve_attach_card"];
                        info = new DynamicDisplayExtendInfo()
                        {
                            Type = DynamicDisplayExtendInfoType.Reserve,
                            Title = (string)data["title"],
                            DescFirst = (string)data["desc_first"]["text"],
                            DescSecond = (string)data["desc_second"],
                            JumpUrl = (string)data["jump_url"],
                            LivePlanStartTime = Utils.GetDateTime((long)data["livePlanStartTime"]),
                        };
                        break;
                    }
            }
            return info;
        }
    }

    public enum DynamicDisplayExtendInfoType
    {
        Game = 2,
        Reserve = 6,
    }

    public class DynamicLottery
    {
        /// <summary>
        /// 抽奖时间
        /// </summary>
        public DateTime LotteryTime;

        /// <summary>
        /// 发布时间
        /// </summary>
        public DateTime PublishTime;

        /// <summary>
        /// 一等奖内容
        /// </summary>
        public string FirstPrize;

        /// <summary>
        /// 一等奖个数
        /// </summary>
        public int FirstPrizeNum;

        /// <summary>
        /// 一等奖图像
        /// </summary>
        public string FirstPrizePic;

        /// <summary>
        /// 二等奖内容
        /// </summary>
        public string SecondPrize;

        /// <summary>
        /// 二等奖个数
        /// </summary>
        public int SecondPrizeNum;

        /// <summary>
        /// 二等奖图像
        /// </summary>
        public string SecondPrizePic;

        /// <summary>
        /// 三等奖内容
        /// </summary>
        public string ThirdPrize;

        /// <summary>
        /// 三等奖个数
        /// </summary>
        public int ThirdPrizeNum;

        /// <summary>
        /// 三等奖图像
        /// </summary>
        public string ThirdPrizePic;

        DynamicLottery() { }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"一等奖: ({FirstPrizeNum}人)").Append("  >").Append(FirstPrize);
            if (SecondPrizeNum > 0)
            {
                sb.AppendLine().AppendLine($"二等奖: ({SecondPrizeNum}人)").Append("  >").Append(SecondPrize);
                if (ThirdPrizeNum > 0)
                    sb.AppendLine().AppendLine($"三等奖: ({ThirdPrizeNum}人)").Append("  >").Append(ThirdPrize);
            }
            sb.AppendLine().Append("开奖时间: ").Append(LotteryTime.ToString("yyyy-MM-dd HH:mm:ss"));
            if (DateTime.Now > LotteryTime)
                sb.Append(" (已开奖)");
            return sb.ToString();
        }

        public static DynamicLottery GetByDynamicId(ulong id)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.vc.bilibili.com/lottery_svr/v1/lottery_svr/lottery_notice?dynamic_id=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            return GetByJson(info["data"]);
        }

        public static DynamicLottery GetByJson(JToken data)
        {
            var lottery = new DynamicLottery()
            {
                LotteryTime = Utils.GetDateTime((long)data["lottery_time"]),
                PublishTime = Utils.GetDateTime((long)data["ts"]),
                FirstPrize = (string)data["first_prize_cmt"],
                FirstPrizeNum = (int)data["first_prize"],
                FirstPrizePic = (string)data["first_prize_pic"],
                SecondPrize = (string)data["second_prize_cmt"],
                SecondPrizeNum = (int)data["second_prize"],
                SecondPrizePic = (string)data["second_prize_pic"],
                ThirdPrize = (string)data["third_prize_cmt"],
                ThirdPrizeNum = (int)data["third_prize"],
                ThirdPrizePic = (string)data["third_prize_pic"],
            };
            return lottery;
        }
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
