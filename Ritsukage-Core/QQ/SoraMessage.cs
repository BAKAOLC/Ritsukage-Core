using Ritsukage.Library.Service;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Entities.CQCodes;
using Sora.Entities.CQCodes.CQCodeModel;
using Sora.Entities.Info;
using Sora.Enumeration;
using Sora.Enumeration.ApiType;
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
    public class SoraMessage
    {
        public static class AdditionalMethod
        {
            readonly static Regex _CQ_Regex = new Regex(@"^\[CQ:(?<type>[^,]+)(,(?<data>[^]]+))?\]$");

            public static CQCode[] ToCQCodes(string msg)
            {
                List<CQCode> codes = new();
                int n = 0;
                int i = msg.IndexOf("[", n);
                while (i < msg.Length && i >= n)
                {
                    var e = msg.IndexOf("]", i);
                    codes.Add(CQCode.CQText(Escape(msg[n..i])));
                    if (e >= i)
                    {
                        var m = _CQ_Regex.Match(msg[i..(e + 1)]);
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
                                    codes.Add(CQCode.CQText(param["text"]));
                                    break;
                                case "face":
                                    codes.Add(CQCode.CQFace(int.Parse(param["id"])));
                                    break;
                                case "image":
                                    codes.Add(CQCode.CQImage(param["file"] ?? param["url"]));
                                    break;
                                case "record":
                                    codes.Add(CQCode.CQRecord(param["file"] ?? param["url"]));
                                    break;
                                case "video":
                                    codes.Add(CQCode.CQVideo(param["file"] ?? param["url"]));
                                    break;
                                case "at":
                                    if (param["qq"] == "all")
                                        codes.Add(CQCode.CQAtAll());
                                    else
                                        codes.Add(CQCode.CQAt(long.Parse(param["qq"])));
                                    break;
                                case "share":
                                    codes.Add(CQCode.CQShare(param["url"], param["title"], param["content"], param["image"]));
                                    break;
                            }
                        }
                        else
                            codes.Add(CQCode.CQText(Escape(msg[i..(e + 1)])));
                        i = msg.IndexOf("[", n = e + 1);
                    }
                    else
                    {
                        codes.Add(CQCode.CQText("["));
                        i = msg.IndexOf("[", n = i + 1);
                    }
                }
                codes.Add(CQCode.CQText(Escape(msg[n..])));
                return codes.ToArray();
            }

            public static string ToCQString(IEnumerable<CQCode> codes)
            {
                var sb = new StringBuilder();
                foreach (var code in codes)
                {
                    switch (code.Function)
                    {
                        case CQFunction.Text:
                            sb.Append(Encode(((Text)code.CQData).Content));
                            break;
                        case CQFunction.Face:
                            sb.Append($"[CQ:face,id={((Face)code.CQData).Id}]");
                            break;
                        case CQFunction.Image:
                            sb.Append($"[CQ:image,file={Encode(((Image)code.CQData).ImgFile)}]");
                            break;
                        case CQFunction.Record:
                            sb.Append($"[CQ:record,file={Encode(((Image)code.CQData).ImgFile)}]");
                            break;
                        case CQFunction.At:
                            sb.Append($"[CQ:at,qq={((At)code.CQData).Traget}]");
                            break;
                        case CQFunction.Share:
                            Share s = (Share)code.CQData;
                            sb.Append($"[CQ:share,url={Encode(s.Url)},title={Encode(s.Title)},content={Encode(s.Content)},image={Encode(s.ImageUrl)}]");
                            break;
                    }
                }
                return sb.ToString();
            }
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
        public Message Message { get; init; }

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

        public async ValueTask Recall()
        {
            if (Event is GroupMessageEventArgs gm)
                await gm.RecallSourceMessage();
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> Repeat()
        {
            if (Event is GroupMessageEventArgs gm)
                return await gm.Repeat();
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Repeat();
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> Reply(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
                return await gm.Reply(msg);
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Reply(msg);
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> ReplyToOriginal(params object[] msg)
        {
            msg = (new object[] { CQCode.CQReply(Message.MessageId) }).Concat(msg).ToArray();
            if (Event is GroupMessageEventArgs gm)
                return await gm.Reply(msg);
            else if (Event is PrivateMessageEventArgs pm)
                return await pm.Reply(msg);
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> AutoAtReply(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
            {
                msg = (new object[] { gm.Sender.CQCodeAt() }).Concat(msg).ToArray();
                await gm.Reply(msg);
            }
            else if (Event is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> SendPrivateMessage(params object[] msg)
            => await Sender.SendPrivateMessage(msg);

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
    }
}
