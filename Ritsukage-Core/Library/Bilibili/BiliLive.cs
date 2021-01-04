using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Net;

namespace Ritsukage.Library.Bilibili
{
    class BiliLive
    {
        private const string PostDanmakuUrl = "http://api.live.bilibili.com/msg/send";
        private const string StartLiveUrl = "https://api.live.bilibili.com/room/v1/Room/startLive";
        private const string StopLiveUrl = "https://api.live.bilibili.com/room/v1/Room/stopLive";
        private const string InfoUpdateUrl = "https://api.live.bilibili.com/room/v1/Room/update";
        private const string LiveRoomUrl = "https://live.bilibili.com";

        public static string SendDanmaku(int roomid, string msg, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDanmakuUrl);
                Utils.SetHttpHeaders(request, "pc", cookie);
                request.Host = "api.live.bilibili.com";
                request.Referer = "https://live.bilibili.com/" + roomid;
                request.Headers.Add("Origin", "https://live.bilibili.com");
                long t = Utils.GetTimeStamp();
                string jct = Bilibili.GetJCT(cookie);
                string content = $"color=16777215&fontsize=25&mode=1&bubble=0&msg={Utils.UrlEncode(msg)}&rnd={t}&roomid={roomid}&csrf={jct}&csrf_token={jct}";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Sora.Tool.ConsoleLog.ErrorLogBuilder(e);
            }
            return "";
        }

        public static string StartLive(int roomid, int area, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(StartLiveUrl);
                Utils.SetHttpHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", "https://link.bilibili.com");
                request.Referer = "https://link.bilibili.com/p/center/index";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&platform=pc&area_v2={area}&csrf_token={jct}&csrf={jct}";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Sora.Tool.ConsoleLog.ErrorLogBuilder(e);
            }
            return "";
        }
        public static string StopLive(int roomid, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(StopLiveUrl);
                Utils.SetHttpHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", "https://link.bilibili.com");
                request.Referer = "https://link.bilibili.com/p/center/index";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&platform=pc&csrf_token={jct}&csrf={jct}";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Sora.Tool.ConsoleLog.ErrorLogBuilder(e);
            }
            return "";
        }
        public static string UpdateLiveArea(int roomid, int area, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
                Utils.SetHttpHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", LiveRoomUrl);
                request.Referer = $"{LiveRoomUrl}/{roomid}";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&area_id={area}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Sora.Tool.ConsoleLog.ErrorLogBuilder(e);
            }
            return "";
        }
        public static string UpdateLiveTitle(int roomid, string title, string cookie)
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(InfoUpdateUrl);
                Utils.SetHttpHeaders(request, "pc", cookie);
                request.Headers.Add("Origin", LiveRoomUrl);
                request.Referer = $"{LiveRoomUrl}/{roomid}";
                string jct = Bilibili.GetJCT(cookie);
                string content = $"room_id={roomid}&title={Utils.UrlEncode(title)}&platform=pc&csrf_token={jct}&csrf={jct}&visit_id=";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                Sora.Tool.ConsoleLog.ErrorLogBuilder(e);
            }
            return "";
        }

        public static int GetUserLiveRoom(int uid)
        {
            var j = JObject.Parse(Utils.HttpGET("https://api.live.bilibili.com/room/v1/Room/getRoomInfoOld?mid=" + uid));
            if ((int)j["code"] == 0)
                return (int)j["data"]["roomid"];
            return 0;
        }
    }
}
