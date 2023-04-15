using Ritsukage.Library.Service;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Entities.Info;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
using Sora.Enumeration;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Group = Sora.Entities.Group;

namespace Ritsukage.QQ
{
    public partial class SoraMessage
    {
        public static partial class AdditionalMethod
        {
            public static List<SoraSegment> ToSoraSegment(string msg)
            {
                List<SoraSegment> codes = new();
                int n = 0;
                int i = msg.IndexOf("[", n);
                while (i < msg.Length && i >= n)
                {
                    var e = msg.IndexOf("]", i);
                    codes.Add(SoraSegment.Text(Escape(msg[n..i])));
                    if (e >= i)
                    {
                        var m = GetCQRegex().Match(msg[i..(e + 1)]);
                        if (m.Success)
                        {
                            Dictionary<string, string> param = new();
                            if (m.Groups["data"].Success && !string.IsNullOrWhiteSpace(m.Groups["data"].Value))
                            {
                                var p = m.Groups["data"].Value.Split(",");
                                foreach (var kv in p)
                                {
                                    var x = kv.Split("=");
                                    param[x[0]] = Escape(x[1]);
                                }
                            }
                            switch (m.Groups["type"].Value)
                            {
                                case "text":
                                    codes.Add(SoraSegment.Text(param["text"]));
                                    break;
                                case "face":
                                    codes.Add(SoraSegment.Face(int.Parse(param["id"])));
                                    break;
                                case "image":
                                    codes.Add(SoraSegment.Image(param["file"] ?? param["url"]));
                                    break;
                                case "record":
                                    codes.Add(SoraSegment.Record(param["file"] ?? param["url"]));
                                    break;
                                case "video":
                                    codes.Add(SoraSegment.Video(param["file"] ?? param["url"]));
                                    break;
                                case "at":
                                    if (param["qq"] == "all")
                                        codes.Add(SoraSegment.AtAll());
                                    else
                                        codes.Add(SoraSegment.At(long.Parse(param["qq"])));
                                    break;
                                case "share":
                                    codes.Add(SoraSegment.Share(param["url"], param["title"], param["content"], param["image"]));
                                    break;
                            }
                        }
                        else
                            codes.Add(SoraSegment.Text(Escape(msg[i..(e + 1)])));
                        i = msg.IndexOf("[", n = e + 1);
                    }
                    else
                    {
                        codes.Add(SoraSegment.Text("["));
                        i = msg.IndexOf("[", n = i + 1);
                    }
                }
                codes.Add(SoraSegment.Text(Escape(msg[n..])));
                return codes;
            }

            public static string ToCQString(IEnumerable<SoraSegment> codes)
            {
                var sb = new StringBuilder();
                foreach (var code in codes)
                {
                    switch (code.MessageType)
                    {
                        case SegmentType.Text:
                            sb.Append(Encode(((TextSegment)code.Data).Content));
                            break;
                        case SegmentType.Face:
                            sb.Append($"[CQ:face,id={((FaceSegment)code.Data).Id}]");
                            break;
                        case SegmentType.Image:
                            sb.Append($"[CQ:image,file={Encode(((ImageSegment)code.Data).ImgFile)}]");
                            break;
                        case SegmentType.Record:
                            sb.Append($"[CQ:record,file={Encode(((ImageSegment)code.Data).ImgFile)}]");
                            break;
                        case SegmentType.At:
                            sb.Append($"[CQ:at,qq={((AtSegment)code.Data).Target}]");
                            break;
                        case SegmentType.Share:
                            ShareSegment s = (ShareSegment)code.Data;
                            sb.Append($"[CQ:share,url={Encode(s.Url)},title={Encode(s.Title)},content={Encode(s.Content)},image={Encode(s.ImageUrl)}]");
                            break;
                    }
                }
                return sb.ToString();
            }

            [GeneratedRegex(@"^\[CQ:(?<type>[^,]+)(,(?<data>[^]]+))?\]$")]
            private static partial Regex GetCQRegex();
        }

        /// <summary>
        /// 源事件
        /// </summary>
        public BaseSoraEventArgs Event { get; init; }

        /// <summary>
        /// 当前事件的API执行实例
        /// </summary>
        public SoraApi SoraApi => Event.SoraApi;

        /// <summary>
        /// 当前事件名
        /// </summary>
        public string EventName => Event.EventName;

        /// <summary>
        /// 事件产生时间
        /// </summary>
        public DateTime Time => Event.Time;

        /// <summary>
        /// 接收当前事件的机器人UID
        /// </summary>
        public long LoginUid => Event.LoginUid;

        /// <summary>
        /// 消息内容
        /// </summary>
        public MessageContext Message { get; init; }

        /// <summary>
        /// 是否为群聊消息
        /// </summary>
        public bool IsGroupMessage { get; init; }

        /// <summary>
        /// 消息发送者实例
        /// </summary>
        public User Sender { get; init; }

        /// <summary>
        /// 消息来源群组实例
        /// </summary>
        public Group SourceGroup { get; init; }

        /// <summary>
        /// 发送者信息
        /// </summary>
        public GroupSenderInfo GroupSenderInfo { get; init; }

        /// <summary>
        /// 发送者信息
        /// </summary>
        public PrivateSenderInfo PrivateSenderInfo { get; init; }

        /// <summary>
        /// 是否来源于匿名群成员
        /// </summary>
        public bool IsAnonymousMessage { get; init; }

        /// <summary>
        /// 匿名用户实例
        /// </summary>
        public Anonymous Anonymous { get; init; }

        public SoraMessage(GroupMessageEventArgs args)
        {
            Event = args;
            Message = args.Message;
            IsGroupMessage = true;
            Sender = args.Sender;
            SourceGroup = args.SourceGroup;
            GroupSenderInfo = args.SenderInfo;
            IsAnonymousMessage = args.IsAnonymousMessage;
            Anonymous = args.Anonymous;
        }

        public SoraMessage(PrivateMessageEventArgs args)
        {
            Event = args;
            Message = args.Message;
            IsGroupMessage = false;
            Sender = args.Sender;
            PrivateSenderInfo = args.SenderInfo;
            IsAnonymousMessage = false;
        }

        public override string ToString()
            => Message.ToString();

        public async ValueTask Recall()
        {
            if (Sender.Id == LoginUid)
                await Message.RecallMessage();
            else if (Event is GroupMessageEventArgs gm)
                await gm.RecallSourceMessage();
        }

        public async ValueTask<(ApiStatus apiStatus, int messageId)> Repeat()
        {
            if (Event is GroupMessageEventArgs gm)
                return await gm.Repeat();
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Repeat();
            return (default(ApiStatus), -1);
        }

        public async ValueTask<(ApiStatus apiStatus, int messageId)> Reply(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
                return await gm.Reply(BuildMessageBody(msg));
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Reply(BuildMessageBody(msg));
            return (default(ApiStatus), -1);
        }

        public async ValueTask<(ApiStatus apiStatus, int messageId)> ReplyToOriginal(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
                return await gm.Reply(BuildMessageBody(SoraSegment.Reply(Message.MessageId), msg));
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Reply(BuildMessageBody(SoraSegment.Reply(Message.MessageId), msg));
            return (default(ApiStatus), -1);
        }

        public async ValueTask<(ApiStatus apiStatus, int messageId)> AutoAtReply(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
            {
                await gm.Reply(BuildMessageBody(gm.Sender.At(), msg));
            }
            else if (Event is PrivateMessageEventArgs pm)
                await pm.Reply(BuildMessageBody(msg));
            return (default(ApiStatus), -1);
        }

        public async ValueTask<(ApiStatus apiStatus, int messageId)> SendPrivateMessage(params object[] msg)
            => await Sender.SendPrivateMessage(BuildMessageBody(msg));

        public async Task<UserCoins> GetCoins()
            => await CoinsService.GetUserCoins("qq", Sender.Id);

        public async Task<bool> CheckCoins(long count, bool disableFree = false)
            => await CoinsService.CheckUserCoins("qq", Sender.Id, count, disableFree);

        public async Task<UserCoins> AddCoins(long count)
            => await CoinsService.AddUserCoins("qq", Sender.Id, count);

        public async Task<UserCoins> RemoveCoins(long count, bool disableFree = false)
            => await CoinsService.RemoveUserCoins("qq", Sender.Id, count, disableFree);

        public async Task<bool> CheckCooldown(string tag, int seconds)
            => await CooldownService.CheckCooldown("qq", Sender.Id, tag, seconds, false);

        public async Task UpdateCooldown(string tag)
            => await CooldownService.UpdateCooldown("qq", Sender.Id, tag, false);

        public async Task<bool> CheckGroupCooldown(string tag, int seconds)
            => await CooldownService.CheckCooldown("qq", SourceGroup.Id, tag, seconds, true);

        public async Task UpdateGroupCooldown(string tag)
            => await CooldownService.UpdateCooldown("qq", SourceGroup.Id, tag, true);

        public static string Escape(string s) => System.Web.HttpUtility.HtmlDecode(s);

        public static string Encode(string s) => s.Replace("&", "&amp;").Replace("[", "&#91;").Replace("]", "&#93;").Replace(",", "&#44;");

        public static MessageBody BuildMessageBody(IEnumerable<SoraSegment> segments)
            => segments.ToMessageBody();

        public static MessageBody BuildMessageBody(IEnumerable<object> segments)
            => BuildMessageBody(BuildSoraSegment(segments));

        public static MessageBody BuildMessageBody(params object[] msg)
            => BuildMessageBody(BuildSoraSegment(msg));

        static IEnumerable<SoraSegment> InnerBuildSoraSegment(IEnumerable<object> data)
        {
            var result = new List<SoraSegment>();
            foreach (var obj in data)
            {
                if (obj is IEnumerable<SoraSegment> segs)
                    foreach (var o in InnerBuildSoraSegment(segs))
                        result.Add(o);
                else if (obj is IEnumerable<object> e)
                    foreach (var o in InnerBuildSoraSegment(e))
                        result.Add(o);
                else if (obj is SoraSegment seg)
                    result.Add(seg);
                else
                    result.Add(SoraSegment.Text(obj.ToString()));
            }
            return result;
        }

        static IEnumerable<SoraSegment> InnerBuildSoraSegment(IEnumerable<SoraSegment> data)
            => InnerBuildSoraSegment(data.Cast<object>());

        public static List<SoraSegment> BuildSoraSegment(params object[] msg)
        {
            var result = new List<SoraSegment>();
            foreach (var obj in msg)
            {
                if (obj is IEnumerable<SoraSegment> segs)
                    foreach (var o in InnerBuildSoraSegment(segs))
                        result.Add(o);
                else if(obj is IEnumerable<object> e)
                    foreach (var o in InnerBuildSoraSegment(e))
                        result.Add(o);
                else if (obj is SoraSegment seg)
                    result.Add(seg);
                else
                    result.Add(SoraSegment.Text(obj.ToString()));
            }
            return result;
        }
    }
}
