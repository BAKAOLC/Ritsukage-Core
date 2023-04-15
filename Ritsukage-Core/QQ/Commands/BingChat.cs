using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Ritsukage.Library.Microsoft.BingChat;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class BingChat
    {
        static string Token => Program.Config.BingChatCookieToken;

        static Library.Microsoft.BingChat Client;
        static readonly Dictionary<long, Session> Sessions = new();

        enum SessionGetFrom
        {
            Unknown = -1,
            Create,
            Record
        }

        static async Task<(SessionGetFrom, Session)> GetOrCreateSession(long id)
        {
            try
            {
                if (Client == null)
                {
                    if (!string.IsNullOrEmpty(Token))
                        Client = new(Token);
                    else
                        return (SessionGetFrom.Unknown, null);
                }
                if (Sessions.TryGetValue(id, out var session))
                {
                    return (SessionGetFrom.Record, session);
                }
                else
                {
                    session = await Client.CreateConversation();
                    Sessions.Add(id, session);
                    return (SessionGetFrom.Create, session);
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("BingChat", ex.GetFormatString());
                return (SessionGetFrom.Unknown, null);
            }
        }

        [Command("bingchat"), OnlyForGroup(981468376, 783243086)]
        [CommandDescription("进行BingChat对话")]
        [ParameterDescription(1, "prompt", "对话内容")]
        public static void Normal(SoraMessage e, string prompt = "")
        {
            if (string.IsNullOrEmpty(prompt))
                InnerCreate(e);
            else
                InnerChat(e, e.Message.GetText()[9..]);
        }

        [Command("sbingchat"), OnlyForSuperUser]
        [CommandDescription("进行BingChat对话")]
        [ParameterDescription(1, "prompt", "对话内容")]
        public static void SuperUser(SoraMessage e, string prompt = "")
        {
            if (string.IsNullOrEmpty(prompt))
                InnerCreate(e);
            else
                InnerChat(e, e.Message.GetText()[10..]);
        }

        static async void InnerCreate(SoraMessage e)
        {
            (var sessionGetFrom, var session) = await GetOrCreateSession(e.Sender.Id);
            switch (sessionGetFrom)
            {
                case SessionGetFrom.Create:
                    await e.ReplyToOriginal("[BingChat] 已创建BingChat对话");
                    break;
                case SessionGetFrom.Record:
                    await session.ResetConversation();
                    await e.ReplyToOriginal("[BingChat] 已重置对话状态");
                    break;
                case SessionGetFrom.Unknown:
                default:
                    await e.ReplyToOriginal("[BingChat] 发生未知异常，请稍后再试");
                    break;
            }
        }

        static async void InnerChat(SoraMessage e, string chat)
        {
            (var sessionGetFrom, var session) = await GetOrCreateSession(e.Sender.Id);
            switch (sessionGetFrom)
            {
                case SessionGetFrom.Create:
                    await e.ReplyToOriginal("[BingChat] 尚未创建BingChat对话，已自动为您创建对话，等待服务器回复中……");
                    break;
                case SessionGetFrom.Record:
                    await e.ReplyToOriginal("[BingChat] 请等待服务器回复……");
                    break;
                case SessionGetFrom.Unknown:
                default:
                    await e.ReplyToOriginal("[BingChat] 发生未知异常，请稍后再试");
                    return;
            }
            try
            {
                var reply = await session.Ask(chat);
                if (!string.IsNullOrEmpty(reply))
                    await e.ReplyToOriginal("[BingChat] " + ProcessReply(reply));
                else
                    await e.ReplyToOriginal("[BingChat] 答复为空");
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("BingChat", ex.GetFormatString());
                await e.ReplyToOriginal("[BingChat] 会话异常，请尝试重置会话或稍后再试");
            }
        }

        static readonly Regex PrefixQuote = new(@"(\[\d+\]:[^\n]+\n)|(\[\^\d+\^\]\[\d+\])");
        static string ProcessReply(string reply)
            => PrefixQuote.Replace(reply, string.Empty);
    }
}