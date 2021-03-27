using Ritsukage.Library.Data;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Ritsukage.QQ.Events
{
    [EventGroup]
    public static class SmartMinecraftLink
    {
        [Event(typeof(GroupMessageEventArgs))]
        public static async void Receiver(object sender, GroupMessageEventArgs args)
        {
            var data = await Database.Data.Table<QQGroupSetting>()?.Where(x => x.Group == args.SourceGroup.Id).FirstOrDefaultAsync();
            if (data != null && data.SmartMinecraftLink)
                Trigger(args);
        }

        const int DelayTime = 10;
        static readonly object _lock = new();
        static readonly Dictionary<long, Dictionary<string, DateTime>> Delay = new();

        const string MoJira = "https://bugs.mojang.com/browse/";
        static readonly Regex Regex_MojiraID = new Regex(@"^MC(PE)?-\d+$");

        static async void Trigger(GroupMessageEventArgs args)
        {
            Dictionary<string, DateTime> record;
            lock (_lock)
            {
                if (!Delay.TryGetValue(args.SourceGroup.Id, out record))
                    Delay.Add(args.SourceGroup.Id, record = new());
            }
            var msg = args.Message.RawText;
            if (msg.StartsWith(MoJira))
                msg = msg[MoJira.Length..];
            var m = Regex_MojiraID.Match(msg);
            if (m.Success)
            {
                if (!record.ContainsKey(m.Value) || (DateTime.Now - record[m.Value]).TotalSeconds >= DelayTime)
                {
                    record[m.Value] = DateTime.Now;
                    try
                    {
                        await args.Reply(Commands.Minecraft.GetIssueInfo(m.Value));
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
