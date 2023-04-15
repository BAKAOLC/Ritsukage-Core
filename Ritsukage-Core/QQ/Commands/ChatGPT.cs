using ChatGPT.Net.Enums;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using static Ritsukage.Library.OpenAI.ChatGPT;

namespace Ritsukage.QQ.Commands
{
    [CommandGroup("Utils")]
    public static class ChatGPT
    {
        static string Token => Program.Config.OpenAISessionToken;
        static bool IsPro => Program.Config.OpenAIIsPro;

        static Library.OpenAI.ChatGPT Api;
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
                if (Api == null)
                {
                    if (!string.IsNullOrEmpty(Token))
                        Api = new(Token, IsPro ? AccountType.Pro : AccountType.Free);
                    else
                        return (SessionGetFrom.Unknown, null);
                }
                if (Sessions.TryGetValue(id, out var session))
                {
                    return (SessionGetFrom.Record, session);
                }
                else
                {
                    session = await Api.CreateConversation();
                    Sessions.Add(id, session);
                    return (SessionGetFrom.Create, session);
                }
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("ChatGPT", ex.GetFormatString());
                return (SessionGetFrom.Unknown, null);
            }
        }

        [Command("chatgpt"), OnlyForGroup(981468376, 783243086)]
        [CommandDescription("进行ChatGPT对话")]
        [ParameterDescription(1, "prompt", "对话内容")]
        public static void Normal(SoraMessage e, string prompt = "")
        {
            if (string.IsNullOrEmpty(prompt))
                InnerCreate(e);
            else
                InnerChat(e, e.Message.GetText()[8..]);
        }

        [Command("schatgpt"), OnlyForSuperUser]
        [CommandDescription("进行ChatGPT对话")]
        [ParameterDescription(1, "prompt", "对话内容")]
        public static void SuperUser(SoraMessage e, string prompt = "")
        {
            if (string.IsNullOrEmpty(prompt))
                InnerCreate(e);
            else
                InnerChat(e, e.Message.GetText()[9..]);
        }

        static async void InnerCreate(SoraMessage e)
        {
            (var sessionGetFrom, var session) = await GetOrCreateSession(e.Sender.Id);
            switch (sessionGetFrom)
            {
                case SessionGetFrom.Create:
                    await e.ReplyToOriginal("[ChatGPT] 已创建ChatGPT对话");
                    break;
                case SessionGetFrom.Record:
                    await session.ResetConversation();
                    await e.ReplyToOriginal("[ChatGPT] 已重置对话状态");
                    break;
                case SessionGetFrom.Unknown:
                default:
                    await e.ReplyToOriginal("[ChatGPT] 发生未知异常，请稍后再试");
                    break;
            }
        }

        static async void InnerChat(SoraMessage e, string chat)
        {
            (var sessionGetFrom, var session) = await GetOrCreateSession(e.Sender.Id);
            switch (sessionGetFrom)
            {
                case SessionGetFrom.Create:
                    await e.ReplyToOriginal("[ChatGPT] 尚未创建ChatGPT对话，已自动为您创建对话，等待服务器回复中……");
                    break;
                case SessionGetFrom.Record:
                    await e.ReplyToOriginal("[ChatGPT] 请等待服务器回复……");
                    break;
                case SessionGetFrom.Unknown:
                default:
                    await e.ReplyToOriginal("[ChatGPT] 发生未知异常，请稍后再试");
                    return;
            }
            try
            {
                var reply = await session.Ask(chat);
                if (!string.IsNullOrEmpty(reply))
                    await e.ReplyToOriginal("[ChatGPT] " + reply);
                else
                    await e.ReplyToOriginal("[ChatGPT] 答复为空");
            }
            catch (Exception ex)
            {
                ConsoleLog.Error("ChatGPT", ex.GetFormatString());
                await e.ReplyToOriginal("[ChatGPT] 会话异常，请尝试重置会话或稍后再试");
            }
        }
    }
}