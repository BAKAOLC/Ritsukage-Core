﻿using Ritsukage.Library.Data;
using Ritsukage.Library.Service;
using Ritsukage.Tools.Console;
using Sora.Entities.Segment;
using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.QQ.Service
{
    [Service]
    public static class TipService
    {
        public static Task Init()
        {
            new Thread(CheckMethod) { IsBackground = true }.Start();
            return Task.CompletedTask;
        }

        static readonly TimeSpan CheckSpan = TimeSpan.FromMilliseconds(500);

        static readonly object _lock = new();
        public static void CheckMethod()
        {
            while (true)
            {
                Thread.Sleep(CheckSpan);
                lock (_lock)
                {
                    Task.Run(async () =>
                    {
                        var now = DateTime.Now;
                        var bots = Program.QQServer.GetBotList();
                        var msgs = await TipMessageService.GetTipMessages(TipMessage.TipTargetType.QQGroup, now);
                        foreach (var msg in msgs)
                            foreach (var bot in bots)
                            {
                                var api = Program.QQServer.GetSoraApi(bot);
                                if (await api.CheckHasGroup(msg.TargetID))
                                {
                                    ConsoleLog.Debug("TipMessage", $"Send tip message to group {msg.TargetID} with bot {bot}");
                                    var m = new ArrayList
                                    {
                                        "[Tip Message]",
                                        Environment.NewLine
                                    };
                                    int n = 0;
                                    int i = msg.Message.IndexOf("[@all]", n);
                                    while (i < msg.Message.Length && i >= n)
                                    {
                                        m.Add(msg.Message[n..i]);
                                        m.Add(SoraSegment.AtAll());
                                        i = msg.Message.IndexOf("[@all]", n = i + 6);
                                    }
                                    m.Add(msg.Message[n..]);
                                    await api.SendGroupMessage(msg.TargetID, SoraMessage.BuildMessageBody(m.ToArray()));
                                }
                            }
                        await TipMessageService.RefreshTipMessages(now);
                    });
                }
            }
        }
    }
}
