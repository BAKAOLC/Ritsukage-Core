using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Tools;
using Sora.Entities.CQCodes;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Ritsukage.QQ.Events
{
    [EventGroup]
    public static class SmartBilibiliLink
    {
        [Event(typeof(GroupMessageEventArgs))]
        public static async void Receiver(object sender, GroupMessageEventArgs args)
        {
            var data = await Database.Data.Table<QQGroupSetting>()?.Where(x => x.Group == args.SourceGroup.Id).FirstOrDefaultAsync();
            if (data != null && data.SmartBilibiliLink)
                Trigger(args);
        }

        const int DelayTime = 10;
        static readonly object _lock = new();
        static readonly Dictionary<long, Dictionary<string, Dictionary<string, DateTime>>> Delay = new();

        static readonly Regex Regex_ShortLink = new Regex(@"^(https?://b23\.tv/)(?<data>[0-9a-zA-Z]+)");

        const string BilibiliVideo = "https://www.bilibili.com/video/";
        static readonly Regex Regex_AV = new Regex(@"^[Aa][Vv](?<av>\d+)$");
        static readonly Regex Regex_BV = new Regex(@"^[Bb][Vv](?<bv>1[1-9a-km-zA-HJ-NP-Z]{2}4[1-9a-km-zA-HJ-NP-Z]1[1-9a-km-zA-HJ-NP-Z]7[1-9a-km-zA-HJ-NP-Z]{2})$");
        static readonly Regex Regex_Video = new Regex(@"^(https?://www\.bilibili\.com/video/)(?<id>[0-9a-zA-Z]+)");

        const string BilibiliLiveRoom = "https://live.bilibili.com/";
        static readonly Regex Regex_LiveRoom = new Regex(@"^(https?://live\.bilibili\.com/)(?<id>\d+)");

        static async void Trigger(GroupMessageEventArgs args)
        {
            Dictionary<string, Dictionary<string, DateTime>> record;
            lock (_lock)
            {
                if (!Delay.TryGetValue(args.SourceGroup.Id, out record))
                {
                    Delay.Add(args.SourceGroup.Id, record = new()
                    {
                        { "video", new() },
                        { "live", new() }
                    });
                }
            }
            Match m;
            string baseStr = args.Message.RawText;
            #region RawText Match
            #region AV
            m = Regex_AV.Match(baseStr);
            if (m.Success)
            {
                var av = m.Groups["av"].Value;
                if (int.TryParse(m.Groups["av"].Value, out var _av))
                {
                    try
                    {
                        if (!record["video"].ContainsKey(av) || (DateTime.Now - record["video"][av]).TotalSeconds >= DelayTime)
                        {
                            record["video"][av] = DateTime.Now;
                            var video = Video.Get(_av);
                            SendVideoInfo(args, video);
                        }
                    }
                    catch
                    {
                    }
                }
                return;
            }
            #endregion
            #region BV
            m = Regex_BV.Match(baseStr);
            if (m.Success)
            {
                var bv = m.Groups["bv"].Value;
                try
                {
                    if (!record["video"].ContainsKey(baseStr) || (DateTime.Now - record["video"][bv]).TotalSeconds >= DelayTime)
                    {
                        record["video"][bv] = DateTime.Now;
                        var video = Video.Get(baseStr);
                        SendVideoInfo(args, video);
                    }
                }
                catch
                {
                }
                return;
            }
            #endregion
            #region Live
            m = Regex_LiveRoom.Match(baseStr);
            if (m.Success)
            {
                var o = m.Groups["id"].Value;
                if (int.TryParse(o, out var id))
                {
                    try
                    {
                        if (!record["live"].ContainsKey(o) || (DateTime.Now - record["live"][o]).TotalSeconds >= DelayTime)
                        {
                            record["live"][o] = DateTime.Now;
                            var room = LiveRoom.Get(id);
                            SendLiveRoomInfo(args, room);
                        }
                    }
                    catch
                    {
                    }
                }
                return;
            }
            #endregion
            #endregion
            #region Generate Url List
            List<string> urls = new();
            var matches = Utils.UrlRegex.Matches(baseStr);
            foreach (Match match in matches)
            {
                m = Regex_ShortLink.Match(match.Value);
                if (m.Success)
                {
                    var sv = m.Groups["data"].Value;
                    if (Regex_AV.IsMatch(sv) || Regex_BV.IsMatch(sv))
                        urls.Add(BilibiliVideo + sv);
                    else
                        urls.Add(await Utils.GetOriginalUrl(m.Value));
                }
                else
                    urls.Add(match.Value);
            }
            #endregion
            #region Url Parser
            foreach (var url in urls)
            {
                #region Video
                m = Regex_Video.Match(url);
                if (m.Success)
                {
                    var id = m.Groups["id"].Value;
                    #region AV
                    m = Regex_AV.Match(id);
                    if (m.Success)
                    {
                        var av = m.Groups["av"].Value;
                        if (int.TryParse(m.Groups["av"].Value, out var _av))
                        {
                            try
                            {
                                if (!record["video"].ContainsKey(av) || (DateTime.Now - record["video"][av]).TotalSeconds >= DelayTime)
                                {
                                    record["video"][av] = DateTime.Now;
                                    var video = Video.Get(_av);
                                    SendVideoInfo(args, video);
                                }
                            }
                            catch
                            {
                            }
                        }
                        continue;
                    }
                    #endregion
                    #region BV
                    m = Regex_BV.Match(id);
                    if (m.Success)
                    {
                        var bv = m.Groups["bv"].Value;
                        try
                        {
                            if (!record["video"].ContainsKey(bv) || (DateTime.Now - record["video"][bv]).TotalSeconds >= DelayTime)
                            {
                                record["video"][bv] = DateTime.Now;
                                var video = Video.Get(bv);
                                SendVideoInfo(args, video);
                            }
                        }
                        catch
                        {
                        }
                        continue;
                    }
                    #endregion
                    continue;
                }
                #endregion
                #region Live
                m = Regex_LiveRoom.Match(url);
                if (m.Success)
                {
                    var o = m.Groups["id"].Value;
                    if (int.TryParse(o, out var id))
                    {
                        try
                        {
                            if (!record["live"].ContainsKey(o) || (DateTime.Now - record["live"][o]).TotalSeconds >= DelayTime)
                            {
                                record["live"][o] = DateTime.Now;
                                var room = LiveRoom.Get(id);
                                SendLiveRoomInfo(args, room);
                            }
                        }
                        catch
                        {
                        }
                    }
                    continue;
                }
                #endregion
            }
            #endregion
        }

        static async void SendVideoInfo(GroupMessageEventArgs e, Video video)
            => await e.Reply(CQCode.CQImage(video.PicUrl), new StringBuilder()
                    .AppendLine()
                    .AppendLine(video.BaseToString())
                    .ToString());

        static async void SendLiveRoomInfo(GroupMessageEventArgs e, LiveRoom room)
        {
            string cover = string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl;
            await e.Reply(CQCode.CQImage(cover), new StringBuilder()
                .AppendLine()
                .AppendLine(room.BaseToString())
                .ToString());
        }
    }
}
