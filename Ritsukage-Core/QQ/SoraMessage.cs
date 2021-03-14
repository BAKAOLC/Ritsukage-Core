using Ritsukage.Library.Service;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Entities.CQCodes;
using Sora.Entities.Info;
using Sora.Enumeration.ApiType;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.QQ
{
    public class SoraMessage
    {
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
                await gm.Repeat();
            else if (Event is PrivateMessageEventArgs pm)
                await pm.Repeat();
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> Reply(params object[] msg)
        {
            if (Event is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (Event is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
            return (APIStatusType.Failed, -1);
        }

        public async ValueTask<(APIStatusType apiStatus, int messageId)> ReplyToOriginal(params object[] msg)
        {
            msg = (new object[] { CQCode.CQReply(Message.MessageId) }).Concat(msg).ToArray();
            if (Event is GroupMessageEventArgs gm)
                await gm.Reply(msg);
            else if (Event is PrivateMessageEventArgs pm)
                await pm.Reply(msg);
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

        public async ValueTask SendPrivateMessage(params object[] msg)
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
    }
}
