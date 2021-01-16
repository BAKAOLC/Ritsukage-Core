using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Drawing;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.Library.Bilibili
{
    class Bilibili
    {
        private const string LoginPage = "https://passport.bilibili.com/login";
        private const string GetLoginUrl = "https://passport.bilibili.com/qrcode/getLoginUrl";
        private const string GetLoginInfoUrl = "https://passport.bilibili.com/qrcode/getLoginInfo";

        public static string NewLoginRequest() => Utils.HttpGET(GetLoginUrl);

        public static string GetLoginInfo(string oauthKey)
        {
            string content = $"oauthKey={oauthKey}&gourl=https%3A%2F%2Fwww.bilibili.com%2F";
            return Utils.HttpPOST(GetLoginInfoUrl, content);
        }

        private const string PostDynamicUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create";
        public static string SendDynamic(string msg, string cookie = "")
        {
            HttpWebRequest request = null;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(PostDynamicUrl);
                Utils.SetHttpHeaders(request, "app", cookie);
                request.Host = "api.vc.bilibili.com";
                request.Referer = "https://t.bilibili.com/";
                request.Headers.Add("Origin", "https://t.bilibili.com");
                long t = Utils.GetTimeStamp();
                string jct = GetJCT(cookie);
                string content = $"dynamic_id=0&type=4&rid=0&content={Utils.UrlEncode(msg)}&extension=%7B%22emoji_type%22%3A1%7D&at_uids=&ctrl=%5B%5D&csrf_token={jct}";
                return Utils.HttpPOST(request, content);
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("Bilibili", ConsoleLog.ErrorLogBuilder(e));
            }
            return "";
        }

        private readonly static Regex RemoveEmptyChars = new Regex(@"\s");
        private readonly static Regex MatchJCT = new Regex("(?<=bili_jct=)[^;]+");
        public static string GetJCT(string cookie = "")
            => MatchJCT.Match(RemoveEmptyChars.Replace(cookie, "")).Value;

        public static void QRCodeLoginRequest(Action<Bitmap> GetQRCode, Action IsScanned, Action<string> LoginSuccess, Action<string> LoginFailed)
        {
            Task.Run(() =>
            {
                var request = JObject.Parse(NewLoginRequest());
                if ((bool)request["status"])
                {
                    var ts = DateTimeOffset.FromUnixTimeSeconds((long)request["ts"]).LocalDateTime;
                    var code = (string)request["data"]["oauthKey"];
                    var scanned = false;
                    Task.Run(() => GetQRCode?.Invoke(QRCodeTool.Generate((string)request["data"]["url"])));
                    while (true)
                    {
                        if ((DateTime.Now - ts).TotalSeconds > 300)
                        {
                            Task.Run(() => LoginFailed?.Invoke("请求已过期，请重新发起登录请求"));
                            break;
                        }
                        Thread.Sleep(1000);
                        var data = JObject.Parse(GetLoginInfo(code));
                        if ((bool)data["status"])
                        {
                            var result = (string)data["data"]["url"];
                            var resultData = string.Join(";", result.Substring(result.IndexOf("?") + 1).Split("&"));
                            Task.Run(() => LoginSuccess?.Invoke(resultData));
                            break;
                        }
                        else if ((int)data["data"] == -5 && !scanned)
                        {
                            scanned = true;
                            Task.Run(() => IsScanned?.Invoke());
                        }
                    }
                }
                else
                {
                    Task.Run(() => LoginFailed?.Invoke("发起登录请求失败，请稍后重试"));
                }
            });
        }
    }
}