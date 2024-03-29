﻿using Newtonsoft.Json.Linq;
using Ritsukage.Library.Bilibili.Model;
using Ritsukage.Library.Data;
using Ritsukage.Library.Graphic;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using SixLabors.ImageSharp.Formats.Png;
using Sora.Entities.Segment;
using Sora.Entities.Segment.DataModel;
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
    public static partial class SmartBilibiliLink
    {
        [Event(typeof(GroupMessageEventArgs))]
        public static async void Receiver(object sender, GroupMessageEventArgs args)
        {
            var data = await Database.FindAsync<QQGroupSetting>(x => x.Group == args.SourceGroup.Id);
            if (data != null && data.SmartBilibiliLink)
            {
                try
                {
                    Trigger(args);
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Event Manager", new StringBuilder()
                        .AppendLine("触发Event时发生错误")
                        .AppendLine($"Event Type\t: {typeof(GroupMessageEventArgs)}")
                        .AppendLine($"Method\t\t: {Receiver}")
                        .Append($"Exception\t: {ex.GetFormatString(true)}"));
                }
            }
        }

        const int DelayTime = 10;
        static readonly object _lock = new();
        static readonly Dictionary<long, Dictionary<string, Dictionary<string, DateTime>>> Delay = new();

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
            var cqJson = args.Message.MessageBody.Where(x => x.MessageType == Sora.Enumeration.SegmentType.Json);
            if (cqJson.Any())
            {
                var data = JObject.Parse(((CodeSegment)cqJson.First().Data).Content);
                if ((string)data["desc"] == "哔哩哔哩")
                    baseStr = (string)data["meta"]["detail_1"]["qqdocurl"];
            }
            #endregion
            #region RawText Match
            #region User
            m = GetUserRegex().Match(baseStr);
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
            m = GetAVRegex().Match(baseStr);
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
            m = GetBVRegex().Match(baseStr);
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
            m = GetLiveRoomRegex().Match(baseStr);
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
            m = GetDynamicRegex().Match(baseStr);
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
            foreach (Match match in matches.Cast<Match>())
            {
                m = GetShortLinkRegex().Match(match.Value);
                if (m.Success)
                {
                    var sv = m.Groups["data"].Value;
                    if (GetAVRegex().IsMatch(sv) || GetBVRegex().IsMatch(sv))
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
                m = GetUserRegex().Match(url);
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
                m = GetVideoRegex().Match(url);
                if (m.Success)
                {
                    var id = m.Groups["id"].Value;
                    #region AV
                    m = GetAVRegex().Match(id);
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
                    m = GetBVRegex().Match(id);
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
                m = GetLiveRoomRegex().Match(url);
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
                m = GetDynamicRegex().Match(url);
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
            var img = await DownloadManager.Download(user.FaceUrl, enableAria2Download: true, enableSimpleDownload: true);
            await e.Reply(SoraMessage.BuildMessageBody(string.IsNullOrEmpty(img) ? "[图像下载失败]" : SoraSegment.Image(img),
                new StringBuilder().AppendLine().Append(user.BaseToString()).ToString()));
        }

        static async void SendVideoInfo(GroupMessageEventArgs e, Video video)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending video info: {video.AV}");
            var img = await DownloadManager.Download(video.PicUrl, enableAria2Download: true, enableSimpleDownload: true);
            await e.Reply(SoraMessage.BuildMessageBody(string.IsNullOrEmpty(img) ? "[图像下载失败]" : SoraSegment.Image(img),
                new StringBuilder().AppendLine().Append(video.BaseToString()).ToString()));
        }

        static async void SendLiveRoomInfo(GroupMessageEventArgs e, LiveRoom room)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending live room info: {room.Id}");
            string cover = await DownloadManager.Download(string.IsNullOrWhiteSpace(room.UserCoverUrl) ? room.KeyFrame : room.UserCoverUrl,
                enableAria2Download: true, enableSimpleDownload: true);
            await e.Reply(SoraMessage.BuildMessageBody(string.IsNullOrEmpty(cover) ? "[图像下载失败]" : SoraSegment.Image(cover),
                new StringBuilder().AppendLine().Append(room.BaseToString()).ToString()));
        }

        static async void SendDynamicInfo(GroupMessageEventArgs e, Dynamic dynamic)
        {
            ConsoleLog.Debug("Smart Bilibili Link", $"Sending dynamic info: {dynamic.Id}");
            ArrayList msg = new();
            var pics = await DownloadManager.Download(dynamic.Pictures, enableAria2Download: true, enableSimpleDownload: true);
            foreach (var pic in pics)
            {
                if (string.IsNullOrEmpty(pic))
                    msg.Add("[图像下载失败]");
                else
                {
                    GraphicUtils.LimitGraphicScale(pic, 2048, 2048);
                    msg.Add(SoraSegment.Image(pic));
                }
                msg.Add(Environment.NewLine);
            }
            msg.Add(dynamic.BaseToString());
            await e.Reply(SoraMessage.BuildMessageBody(msg.ToArray()));
            var np = await dynamic.GetNinePicture();
            if (np != null)
            {
                var name = Path.GetTempFileName();
                var output = File.OpenWrite(name);
                var encoder = new PngEncoder();
                encoder.Encode(np, output);
                output.Dispose();
                await e.Reply(SoraSegment.Image(name));
            }
        }

        [GeneratedRegex("^((https?://)?b23\\.tv/)(?<data>[0-9a-zA-Z]+)")]
        private static partial Regex GetShortLinkRegex();
        [GeneratedRegex("^((https?://)?space\\.bilibili\\.com/)(?<id>\\d+)")]
        private static partial Regex GetUserRegex();
        [GeneratedRegex("^[Aa][Vv](?<av>\\d+)$")]
        private static partial Regex GetAVRegex();
        [GeneratedRegex("^[Bb][Vv](?<bv>1[1-9a-km-zA-HJ-NP-Z]{2}4[1-9a-km-zA-HJ-NP-Z]1[1-9a-km-zA-HJ-NP-Z]7[1-9a-km-zA-HJ-NP-Z]{2})$")]
        private static partial Regex GetBVRegex();
        [GeneratedRegex("^((https?://)?www\\.bilibili\\.com/video/)(?<id>[0-9a-zA-Z]+)")]
        private static partial Regex GetVideoRegex();
        [GeneratedRegex("^((https?://)?live\\.bilibili\\.com/)(?<id>\\d+)")]
        private static partial Regex GetLiveRoomRegex();
        [GeneratedRegex("^((https?://)?t\\.bilibili\\.com/)(?<id>\\d+)")]
        private static partial Regex GetDynamicRegex();
    }
}
