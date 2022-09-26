using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using Sora.Entities.Segment;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Ritsukage.QQ.Events
{
    [EventGroup]
    public static class SmartPixivLink
    {
        [Event(typeof(GroupMessageEventArgs))]
        public static async void Receiver(object sender, GroupMessageEventArgs args)
        {
            var data = await Database.FindAsync<QQGroupSetting>(x => x.Group == args.SourceGroup.Id);
            if (data != null && data.SmartPixivLink)
                Trigger(args);
        }

        const int DelayTime = 30;
        static readonly object _lock = new();
        static readonly Dictionary<long, Dictionary<int, DateTime>> Delay = new();

        const string Host = "www.pixiv.net/";
        const string IllustID = "illust_id=";
        const string Artworks = "artworks/";
        static readonly Regex MatchID = new Regex(@"^\d+");

        static void Trigger(GroupMessageEventArgs args)
        {
            Dictionary<int, DateTime> record;
            lock (_lock)
            {
                if (!Delay.TryGetValue(args.SourceGroup.Id, out record))
                    Delay.Add(args.SourceGroup.Id, record = new());
            }
            List<int> ids = new();
            foreach (var url in Utils.MatchUrls(args.Message.RawText))
            {
                int index = url.IndexOf(Host);
                if (index >= 0)
                {
                    string sub = url[(Host.Length + index)..];
                    index = sub.IndexOf(IllustID);
                    if (index >= 0)
                        sub = sub[(IllustID.Length + index)..];
                    else
                    {
                        index = sub.IndexOf(Artworks);
                        if (index >= 0)
                            sub = sub[(Artworks.Length + index)..];
                    }
                    var match = MatchID.Match(sub);
                    if (match.Success)
                        ids.Add(int.Parse(match.Value));
                }
            }
            var illusts = ids.Distinct().ToArray();
            if (illusts.Length > 0)
            {
                foreach (var illust in illusts)
                {
                    if (!record.ContainsKey(illust) || (DateTime.Now - record[illust]).TotalSeconds >= DelayTime)
                    {
                        record[illust] = DateTime.Now;
                    }
                }
                try
                {
                    Commands.Pixiv.GetIllustDetail(illusts,
                        async (msg) => await args.SourceGroup.SendGroupMessage(
                            SoraMessage.BuildMessageBody((new object[] { SoraSegment.Reply(args.Message.MessageId) }).Concat(msg).ToArray())),
                        async (msg) => await args.SourceGroup.SendGroupMessage(SoraMessage.BuildMessageBody(msg)));
                }
                catch
                {
                }
            }
        }
    }
}
