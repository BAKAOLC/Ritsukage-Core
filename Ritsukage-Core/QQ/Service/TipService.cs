using Ritsukage.Library.Data;
using Ritsukage.Library.Service;
using Ritsukage.Tools.Console;
using Sora.Entities;
using Sora.Entities.Base;
using Sora.Entities.Segment;
using System;
using System.Collections;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Timer = System.Timers.Timer;

namespace Ritsukage.QQ.Service
{
    [Service]
    public static class TipService
    {

        static Timer Timer;

        static readonly int CheckInterval = 500;

        public static Task Init()
        {
            Timer = new()
            {
                Interval = CheckInterval
            };
            Timer.Elapsed += (s, e) => CheckMethod();
            Timer.Start();
            return Task.CompletedTask;
        }

        static async Task<TipMessage[]> GetTipMessages()
        {
            var now = DateTime.Now;
            var messages = await TipMessageService.GetTipMessages(TipMessage.TipTargetType.QQGroup, now).ConfigureAwait(false);
            await TipMessageService.RefreshTipMessages(now).ConfigureAwait(false);
            return messages;
        }

        static MessageBody BuildMessage(TipMessage message)
        {
            var m = new ArrayList
            {
                "[Tip Message]",
                Environment.NewLine
            };
            int n = 0;
            int i = message.Message.IndexOf("[@all]", n);
            while (i < message.Message.Length && i >= n)
            {
                m.Add(message.Message[n..i]);
                m.Add(SoraSegment.AtAll());
                i = message.Message.IndexOf("[@all]", n = i + 6);
            }
            m.Add(message.Message[n..]);
            return SoraMessage.BuildMessageBody(m.ToArray());
        }

        static void SendMessage(TipMessage message, params long[] bots)
        {
            var m = BuildMessage(message);
            foreach (var bot in bots)
            {
                var api = Program.QQServer.GetSoraApi(bot);
                Task.Run(async () => {
                    if (await api.CheckHasGroup(message.TargetID))
                    {
                        ConsoleLog.Debug("TipMessage", $"Send tip message to group {message.TargetID} with bot {bot}");
                        await api.SendGroupMessage(message.TargetID, m).ConfigureAwait(false);
                    }
                }).ConfigureAwait(false);
            }
        }

        static async void CheckMethod()
        {
            var messages = await GetTipMessages().ConfigureAwait(false);
            if (messages.Any())
            {
                var bots = Program.QQServer.GetBotList();
                foreach (var message in messages)
                {
                    SendMessage(message, bots);
                }
            }
        }
    }
}
