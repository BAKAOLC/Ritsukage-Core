using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Library.Graphic;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp.Formats.Png;
using Sora.Entities.CQCodes;
using Sora.EventArgs.SoraEvent;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var data = await Database.FindAsync<QQGroupSetting>(x => x.Group == args.SourceGroup.Id);
            if (data != null && data.SmartBilibiliLink)
                Trigger(args);
        }

        const int DelayTime = 10;
        static readonly object _lock = new();
        static readonly Dictionary<long, Dictionary<string, Dictionary<string, DateTime>>> Delay = new();

        #region Regex define
        static readonly Regex Regex_ShortLink = new Regex(@"^((https?://)?b23\.tv/)(?<data>[0-9a-zA-Z]+)");
        static readonly Regex Regex_User = new Regex(@"^((https?://)?space\.bilibili\.com/)(?<id>\d+)");
        static readonly Regex Regex_AV = new Regex(@"^[Aa][Vv](?<av>\d+)$");
        static readonly Regex Regex_BV = new Regex(@"^[Bb][Vv](?<bv>1[1-9a-km-zA-HJ-NP-Z]{2}4[1-9a-km-zA-HJ-NP-Z]1[1-9a-km-zA-HJ-NP-Z]7[1-9a-km-zA-HJ-NP-Z]{2})$");
        static readonly Regex Regex_Video = new Regex(@"^((https?://)?www\.bilibili\.com/video/)(?<id>[0-9a-zA-Z]+)");
        static readonly Regex Regex_LiveRoom = new Regex(@"^((https?://)?live\.bilibili\.com/)(?<id>\d+)");
        static readonly Regex Regex_Dynamic = new Regex(@"^((https?://)?t\.bilibili\.com/)(?<id>\d+)");
        #endregion

        const string BilibiliVideo = "https://www.bilibili.com/video/";

        static async void Trigger(GroupMessageEventArgs args)
        {
            Dictionary<string, Dictionary<string, DateTime>> record;
            lock (_lock)
            {
                if (!Delay.TryGetValue(args.SourceGroup.Id, out record))
                {
                    Delay.Add(args.SourceGroup.Id, record = new()
                    {
                        { "user", new() },
                        { "video", new() },
                        { "live", new() },
                        { "dynamic", new() }
                    });
                }
            }
            Match m;
            string baseStr = args.Message.RawText;
            #region 小程序
            var cqJson = args.Message.MessageList.Where(x => x.Function == Sora.Enumeration.CQFunction.Json).FirstOrDefault();
            if (cqJson != null)
            {
                var data = JObject.Parse(((Sora.Entities.CQCodes.CQCodeModel.Code)cqJson.CQData).Content);
                if ((string)data["desc"] == "哔哩哔哩")
                    baseStr = (string)data["meta"]["detail_1"]["qqdocurl"];
            }
            #endregion
            #region RawText Match
            #region User
            m = Regex_User.Match(baseStr);
            if (m.Success)
            {
                var o = m.Groups["id"].Value;
                if (int.TryParse(o, out var id))
                {
                    try
                    {
                        if (!record["user"].ContainsKey(o) || (DateTime.Now - record["user"][o]).TotalSeconds >= DelayTime)
                        {
                            record["user"][o] = DateTime.Now;
                            SendUserInfo(args, User.Get(id));
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
                    }
                }
                return;
            }
            #endregion
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
                            SendVideoInfo(args, Video.Get(_av));
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
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
                        SendVideoInfo(args, Video.Get(baseStr));
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
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
                            SendLiveRoomInfo(args, LiveRoom.Get(id));
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
                    }
                }
                return;
            }
            #endregion
            #region Dynamic
            m = Regex_Dynamic.Match(baseStr);
            if (m.Success)
            {
                var o = m.Groups["id"].Value;
                if (ulong.TryParse(o, out var id))
                {
                    try
                    {
                        if (!record["dynamic"].ContainsKey(o) || (DateTime.Now - record["dynamic"][o]).TotalSeconds >= DelayTime)
                        {
                            record["dynamic"][o] = DateTime.Now;
                            SendDynamicInfo(args, Dynamic.Get(id));
                        }
                    }
                    catch (Exception ex)
                    {
                        ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString(true));
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
                #region User
                m = Regex_User.Match(url);
                if (m.Success)
                {
                    var o = m.Groups["id"].Value;
                    if (int.TryParse(o, out var id))
                    {
                        try
                        {
                            if (!record["user"].ContainsKey(o) || (DateTime.Now - record["user"][o]).TotalSeconds >= DelayTime)
                            {
                                record["user"][o] = DateTime.Now;
                                SendUserInfo(args, User.Get(id));
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
                        }
                    }
                    continue;
                }
                #endregion
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
                                    SendVideoInfo(args, Video.Get(_av));
                                }
                            }
                            catch (Exception ex)
                            {
                                ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
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
                                SendVideoInfo(args, Video.Get(bv));
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
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
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString());
                        }
                    }
                    continue;
                }
                #endregion
                #region Dynamic
                m = Regex_Dynamic.Match(url);
                if (m.Success)
                {
                    var o = m.Groups["id"].Value;
                    if (ulong.TryParse(o, out var id))
                    {
                        try
                        {
                            if (!record["dynamic"].ContainsKey(o) || (DateTime.Now - record["dynamic"][o]).TotalSeconds >= DelayTime)
                            {
                                record["dynamic"][o] = DateTime.Now;
                                SendDynamicInfo(args, Dynamic.Get(id));
                            }
                        }
                        catch (Exception ex)
                        {
                            ConsoleLog.Error("Smark Bilibili Link", ex.GetFormatString(true));
                        }
                    }
                    continue;
                }
                #endregion
            }
            #endregion
        }

        static async void SendUserInfo(GroupMessageEventArgs e, User user)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending user info: {user.Id}");
            var img = await DownloadManager.Download(user.FaceUrl, enableSimpleDownload: true);
            await e.Reply(string.IsNullOrEmpty(img) ? "[图像下载失败]" : CQCode.CQImage(img), new StringBuilder()
                .AppendLine().Append(user.BaseToString()).ToString());
        }

        static async void SendVideoInfo(GroupMessageEventArgs e, Video video)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending video info: {video.AV}");
            var img = await DownloadManager.Download(video.PicUrl, enableSimpleDownload: true);
            await e.Reply(string.IsNullOrEmpty(img) ? "[图像下载失败]" : CQCode.CQImage(img), new StringBuilder()
                    .AppendLine().Append(video.BaseToString()).ToString());
        }

        static async void SendLiveRoomInfo(GroupMessageEventArgs e, LiveRoom room)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending live room info: {room.Id}");
            string cover = await DownloadManager.Download(string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl, enableSimpleDownload: true);
            await e.Reply(string.IsNullOrEmpty(cover) ? "[图像下载失败]" : CQCode.CQImage(cover), new StringBuilder()
                .AppendLine().Append(room.BaseToString()).ToString());
        }

        static async void SendDynamicInfo(GroupMessageEventArgs e, Dynamic dynamic)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending dynamic info: {dynamic.Id}");
            ArrayList msg = new();
            var pics = await DownloadManager.Download(dynamic.Pictures, enableSimpleDownload: true);
            foreach (var pic in pics)
            {
                if (string.IsNullOrEmpty(pic))
                    msg.Add("[图像下载失败]");
                else
                {
                    ImageUtils.LimitImageScale(pic, 2048, 2048);
                    msg.Add(CQCode.CQImage(pic));
                }
                msg.Add(Environment.NewLine);
            }
            msg.Add(dynamic.BaseToString());
            await e.Reply(msg.ToArray());
            var np = await dynamic.GetNinePicture();
            if (np != null)
            {
                var name = Path.GetTempFileName();
                var output = File.OpenWrite(name);
                var encoder = new PngEncoder();
                encoder.Encode(np, output);
                output.Dispose();
                await e.Reply(CQCode.CQImage(name));
            }
        }
    }
}
