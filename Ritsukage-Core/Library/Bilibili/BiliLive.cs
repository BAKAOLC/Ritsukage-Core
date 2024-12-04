using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System.Net;

namespace Ritsukage.Library.Bilibili
{
    internal class BiliLive
    {
        private const string PostDanmakuUrl = "http://api.live.bilibili.com/msg/send";
        private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
        private const string StopLiveUrl = "https://api.live.bilibili.com/room/v1/Room/stopLive";
        private const string InfoUpdateUrl = "https://api.live.bilibili.com/room/v1/Room/update";
        private const string LiveRoomUrl = "https://live.bilibili.com";

        public static string SendDanmaku(int roomid, string msg, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(PostDanmakuUrl);
            Utils.SetHttpHeaders(request, "pc", cookie);
            request.Host = "api.live.bilibili.com";
            request.Referer = "https://live.bilibili.com/" + roomid;
            request.Headers.Add("Origin", "https://live.bilibili.com");
            var t = Utils.GetTimeStamp();
            var jct = Bilibili.GetJCT(cookie);
            var content =
                $"color=16777215&fontsize=25&mode=1&bubble=0&msg={Utils.UrlEncode(msg)}&rnd={t}&roomid={roomid}&csrf={jct}&csrf_token={jct}";
            return Utils.HttpPOST(request, content);
        }

        public static string StartLive(int roomid, int area, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(StartLiveUrl);
            Utils.SetHttpHeaders(request, "pc", cookie);
            request.Headers.Add("Origin", "https://link.bilibili.com");
            request.Referer = "https://link.bilibili.com/p/center/index";
            var jct = Bilibili.GetJCT(cookie);
            var content = $"room_id={roomid}&platform=pc&area_v2={area}&csrf_token={jct}&csrf={jct}";
            return Utils.HttpPOST(request, content);
        }

        public static string StopLive(int roomid, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(StopLiveUrl);
            Utils.SetHttpHeaders(request, "pc", cookie);
            request.Headers.Add("Origin", "https://link.bilibili.com");
            request.Referer = "https://link.bilibili.com/p/center/index";
            var jct = Bilibili.GetJCT(cookie);
            var content = $"room_id={roomid}&platform=pc&csrf_token={jct}&csrf={jct}";
            return Utils.HttpPOST(request, content);
        }

        public static string UpdateLiveArea(int roomid, int area, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
            Utils.SetHttpHeaders(request, "pc", cookie);
            request.Headers.Add("Origin", LiveRoomUrl);
            request.Referer = $"{LiveRoomUrl}/{roomid}";
            var jct = Bilibili.GetJCT(cookie);
            var content = $"room_id={roomid}&area_id={area}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
            return Utils.HttpPOST(request, content);
        }

        public static string UpdateLiveTitle(int roomid, string title, string cookie)
        {
            var request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
            Utils.SetHttpHeaders(request, "pc", cookie);
            request.Headers.Add("Origin", LiveRoomUrl);
            request.Referer = $"{LiveRoomUrl}/{roomid}";
            var jct = Bilibili.GetJCT(cookie);
            var content =
                $"room_id={roomid}&title={Utils.UrlEncode(title)}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
            return Utils.HttpPOST(request, content);
        }

        public static int GetUserLiveRoom(int uid)
        {
            try
            {
                var j = JObject.Parse(Utils.HttpGET("https://api.bilibili.com/x/space/acc/info?jsonp=jsonp&mid=" +
                                                    uid));
                if ((int)j["code"] == 0)
                    return (int)j["data"]["live_room"]?["roomid"];
            }
            catch
            {
            }

            return 0;
        }
    }
}