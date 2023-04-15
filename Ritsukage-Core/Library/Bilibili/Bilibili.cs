using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Ritsukage.Library.Bilibili
{
    partial class Bilibili
    {
        private const string LoginPage = "https://passport.bilibili.com/login";
        private const string GetLoginUrl = "https://passport.bilibili.com/qrcode/getLoginUrl";
        private const string GetLoginInfoUrl = "https://passport.bilibili.com/qrcode/getLoginInfo";

        public static string NewLoginRequest() => Utils.HttpGET(GetLoginUrl);

        public static string GetLoginInfo(string oauthKey)
            => Utils.HttpPOST(GetLoginInfoUrl, $"oauthKey={oauthKey}&gourl=https%3A%2F%2Fwww.bilibili.com%2F");

        private const string PostDynamicUrl = "https://api.vc.bilibili.com/dynamic_svr/v1/dynamic_svr/create";
        public static string SendDynamic(string msg, string cookie = "")
            => Utils.HttpPOST(PostDynamicUrl, $"dynamic_id=0&type=4&rid=0&content={Utils.UrlEncode(msg)}&extension=%7B%22emoji_type%22%3A1%7D&at_uids=&ctrl=%5B%5D&csrf_token={GetJCT(cookie)}", 20000, cookie, "https://t.bilibili.com/", "https://t.bilibili.com");

        public static string GetJCT(string cookie = "")
            => GetJCTMatchRegex().Match(GetEmptyCharRegex().Replace(cookie, "")).Value;

        public static void QRCodeLoginRequest(Action<Image<Rgba32>> GetQRCode, Action IsScanned, Action<string> LoginSuccess, Action<string> LoginFailed)
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

        [GeneratedRegex(@"\s")]
        private static partial Regex GetEmptyCharRegex();
        [GeneratedRegex("(?<=bili_jct=)[^;]+")]
        private static partial Regex GetJCTMatchRegex();
    }
}