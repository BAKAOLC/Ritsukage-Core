using Newtonsoft.Json.Linq;
using PyLibSharp.Requests;
using Ritsukage.Tools.Console;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class Utils
    {
        public static readonly Regex UrlRegex = new Regex(@"((http|ftp|https)://)((\[::\])|([a-zA-Z0-9\._-]+(\.[a-zA-Z]{2,6})?)|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?((/[a-zA-Z0-9\._-]+|/)*(\?[a-zA-Z0-9\&%_\./-~-]*)?)?");

        public static long GetTimeStamp()
            => (long)(DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalSeconds;

        public static string UrlEncode(string url)
            => System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);

        public static string GetQQHeadImageUrl(long qq) => "http://q.qlogo.cn/headimg_dl?spec=640&img_type=png&dst_uin=" + qq;

        public static string GetQQGroupHeadImageUrl(long group) => "http://p.qlogo.cn/gh/" + group + "/" + group + "/";

        public static async Task<string> GetShortUrl(string url)
            => await Task.Run(() =>
            {
                var data = JObject.Parse(HttpGET("https://api.uomg.com/api/long2dwz?dwzapi=mrwso&url=" + UrlEncode(url)));
                if ((int)data["code"] == 1)
                    return (string)data["ae_url"];
                return url;
            });

        public static async Task<string> GetOriginalUrl(string url)
            => await Task.Run(() =>
            {
                var data = JObject.Parse(HttpGET("https://api.uomg.com/api/dwz2long?url=" + UrlEncode(url)));
                if ((int)data["code"] == 1)
                    return (string)data["ae_url"];
                return url;
            });

        public static async Task<Stream> GetFileAsync(string url)
        {
            using HttpClient hc = new HttpClient();
            var resp = await hc.GetAsync(url);
            var stream = await resp.Content.ReadAsStreamAsync();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        public static string HttpGET(string url, string postDataStr = "", int timeout = 20000,
            string cookie = "", string referer = "", string origin = "")
        {
            ReqParams @params = new();
            @params.Timeout = timeout;
            if (!string.IsNullOrWhiteSpace(cookie))
                @params.Header.Add(HttpRequestHeader.Cookie, cookie);
            if (!string.IsNullOrWhiteSpace(referer))
                @params.Header.Add(HttpRequestHeader.Referer, referer);
            if (!string.IsNullOrWhiteSpace(origin))
                @params.CustomHeader.Add("Origin", origin);
            try
            {
                return Requests.Get(url + (string.IsNullOrWhiteSpace(postDataStr) ? "" : ("?" + postDataStr)), @params).Text;
            }
            catch (AggregateException ex)
            {
                foreach (var e in ex.InnerExceptions)
                    ConsoleLog.Error("HTTP", ConsoleLog.ErrorLogBuilder(e));
            }
            catch (Exception e)
            {
                ConsoleLog.Error("HTTP", ConsoleLog.ErrorLogBuilder(e));
            }
            return string.Empty;
        }

        public static void SetHttpHeaders(HttpWebRequest request, string os = "app", string cookie = "")
        {
            request.Accept = "application/json, text/plain, */*";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = os switch
            {
                "app" => "Mozilla/5.0 BiliDroid/5.51.1 (bbcallen@gmail.com)",
                "pc" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0",
                _ => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36",
            };
            if (cookie != "")
                request.Headers.Add("cookie", cookie);
        }
        public static string HttpGET(HttpWebRequest request)
        {
            request.Method = "GET";
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string retString;
            if (response.ContentEncoding == "gzip")
            {
                GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using StreamReader reader = new StreamReader(gzip, Encoding.UTF8);
                retString = reader.ReadToEnd();
            }
            else
            {
                using Stream rs = response.GetResponseStream();
                using StreamReader sr = new StreamReader(rs, Encoding.UTF8);
                retString = sr.ReadToEnd();
            }
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }
        public static string HttpPOST(HttpWebRequest request, string content = "")
        {
            request.Method = "POST";
            request.ContentLength = content.Length;
            byte[] byteResquest = Encoding.UTF8.GetBytes(content);
            using Stream stream = request.GetRequestStream();
            stream.Write(byteResquest, 0, byteResquest.Length);
            stream.Close();
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string retString;
            if (response.ContentEncoding == "gzip")
            {
                GZipStream gzip = new GZipStream(response.GetResponseStream(), CompressionMode.Decompress);
                using StreamReader reader = new StreamReader(gzip, Encoding.UTF8);
                retString = reader.ReadToEnd();
            }
            else
            {
                using Stream rs = response.GetResponseStream();
                using StreamReader sr = new StreamReader(rs, Encoding.UTF8);
                retString = sr.ReadToEnd();
            }
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }
        public static string HttpPOST(string Url, string postDataStr, long timeout = 20000,
           string cookie = "", string referer = "", string origin = "")
        {
            HttpWebRequest request = null;
            try
            {
                if (Url.StartsWith("https", StringComparison.OrdinalIgnoreCase))
                {
                    ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback((object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors) =>
                    {
                        return true;
                    });
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                }
                request = (HttpWebRequest)WebRequest.Create(Url);
                request.Timeout = (int)timeout;
                SetHttpHeaders(request, "pc", cookie);
                if (!string.IsNullOrWhiteSpace(referer))
                    request.Referer = referer;
                if (!string.IsNullOrWhiteSpace(origin))
                    request.Headers.Add("Origin", origin);
                return HttpPOST(request, postDataStr);
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("HTTP", ConsoleLog.ErrorLogBuilder(e));
            }
            return string.Empty;
        }
    }
}
