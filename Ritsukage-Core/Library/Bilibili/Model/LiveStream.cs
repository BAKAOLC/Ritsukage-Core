using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ritsukage.Library.Bilibili.Model
{
    public class LiveStream
    {
        #region 属性
        /// <summary>
        /// 用户uid
        /// </summary>
        public int UserId;

        /// <summary>
        /// 房间号
        /// </summary>
        public int Id;

        /// <summary>
        /// 直播状态
        /// </summary>
        public LiveStatus LiveStatus;

        /// <summary>
        /// 开播时间
        /// </summary>
        public DateTime LiveStartTime;

        /// <summary>
        /// 直播流类型
        /// </summary>
        public LiveStreamType Type;

        /// <summary>
        /// 直播流
        /// </summary>
        public List<LiveStreamThread> Thread;
        #endregion

        #region 方法
        public User GetUserInfo() => User.Get(UserId);

        public LiveRoom GetLiveRoom() => LiveRoom.Get(Id);

        public static LiveStream Get(int id, LiveStreamType type = LiveStreamType.Web)
        {
            var info = JObject.Parse(Utils.HttpGET("https://api.live.bilibili.com/room/v1/Room/room_init?id=" + id));
            if ((int)info["code"] != 0)
                throw new Exception((string)info["message"]);
            var stream = new LiveStream()
            {
                UserId = (int)info["data"]["uid"],
                Id = (int)info["data"]["room_id"],
                LiveStatus = (LiveStatus)(int)info["data"]["live_status"],
                Type = type,
                Thread = new()
        };
            if (stream.LiveStatus == LiveStatus.Live)
                stream.LiveStartTime = Utils.GetDateTime((long)info["data"]["live_time"]);
            else
                return stream;
            string b = $"https://api.live.bilibili.com/xlive/web-room/v1/playUrl/playUrl?https_url_req=1&ptype=16&platform={(type == LiveStreamType.Web ? "web" : "h5")}&cid={stream.Id}";
            var info2 = JObject.Parse(Utils.HttpGET(b));
            if ((int)info2["code"] != 0)
                throw new Exception((string)info2["message"]);
            foreach (var data in (JArray)info2["data"]["quality_description"])
            {
                var thread = JObject.Parse(Utils.HttpGET(b + "&qn=" + data["qn"]));
                if ((int)thread["code"] != 0)
                    throw new Exception((string)info2["message"]);
                stream.Thread.Add(new LiveStreamThread((LiveStreamQuality)(int)thread["data"]["current_qn"], (JArray)thread["data"]["durl"]));
            }
            return stream;
        }

        public override string ToString()
        {
            var s = new StringBuilder().Append($"房间号：{Id}");
            foreach (var thread in Thread)
                s.AppendLine().Append(thread.ToString());
            return s.ToString();
        }
        #endregion
    }

    public enum LiveStreamType
    {
        Web,
        H5
    }

    public enum LiveStreamQuality
    {
        HD = 150,
        FHD = 250,
        BD = 400,
        Origin = 10000
    }

    public struct LiveStreamThread
    {
        public LiveStreamQuality Quality { get; init; }
        public string[] Url { get; init; }

        public LiveStreamThread(LiveStreamQuality quality, params string[] url)
        {
            Quality = quality;
            Url = url;
        }

        public LiveStreamThread(LiveStreamQuality quality, JArray durl)
        {
            Quality = quality;
            List<string> lst = new();
            foreach (var data in durl)
                lst.Add((string)data["url"]);
            Url = lst.ToArray();
        }

        public override string ToString()
        {
            var s = new StringBuilder().Append(Quality.ToString());
            foreach (var u in Url)
                s.AppendLine().Append(u);
            return s.ToString();
        }
    }
}
