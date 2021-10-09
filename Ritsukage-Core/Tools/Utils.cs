using Downloader;
using Newtonsoft.Json.Linq;
using Ritsukage.Tools.Console;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class Utils
    {
        public static readonly Regex UrlRegex = new Regex(@"((http|ftp|https)://)((\[::\])|([a-zA-Z0-9\._-]+(\.[a-zA-Z]{2,6})?)|([0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}))(:[0-9]{1,5})?((/[a-zA-Z0-9\._-]+|/)*(\?[a-zA-Z0-9\&%_\./-~-]*)?)?");
        public static string[] MatchUrls(string text)
            => UrlRegex.Matches(text).Where(x => x.Success).Select(x => x.Value).ToArray();

        public static string ToSignNumberString(int num)
            => num < 0 ? num.ToString() : ("+" + num);

        public static string ToUrlParameter(Dictionary<string, object> param = null)
        {
            if (param == null)
                return string.Empty;
            var sb = new List<string>();
            foreach (var p in param)
                sb.Add($"{UrlEncode(p.Key)}={UrlEncode(p.Value == null ? string.Empty : p.Value.ToString())}");
            return string.Join("&", sb);
        }

        public static readonly DateTime BaseUTC = new DateTime(1970, 1, 1, 8, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime GetDateTime(double ts)
            => BaseUTC.AddSeconds(ts);

        public static long GetTimeStamp()
            => (long)(DateTime.UtcNow - BaseUTC).TotalSeconds;

        const string TaobaoTimeStampApi = "http://api.m.taobao.com/rest/api3.do?api=mtop.common.getTimestamp";
        public static long GetNetworkTimeStamp()
        {
            var data = HttpGET(TaobaoTimeStampApi);
            if (string.IsNullOrWhiteSpace(data))
                if (long.TryParse((string)JToken.Parse(data)["data"]["t"], out var t))
                    return t;
            return GetTimeStamp();
        }

        public static string RemoveEmptyLine(string text)
        {
            char splitChar = '\n';
            switch (Environment.NewLine)
            {
                case "\r":
                    text = text.Replace('\n', '\r');
                    splitChar = '\r';
                    break;
                case "\r\n":
                case "\n":
                    text = text.Replace('\r', '\n');
                    splitChar = '\n';
                    break;
            }
            return string.Join(Environment.NewLine,
                text.Split(splitChar, StringSplitOptions.RemoveEmptyEntries)
                .GroupBy(x => x).Select(x => x.Key));
        }

        public static string UrlRemoveParam(string url)
        {
            var m = Regex.Match(url, @"^[^\?]+");
            if (m.Success)
            {
                if (m.Value.EndsWith("/"))
                    return m.Value[0..^1];
                else
                    return m.Value;
            }
            return url;
        }

        static Regex _UrlEncodeParser = new Regex("%[a-f0-9]{2}");
        public static string UrlEncode(string url)
        {
            var encode = System.Web.HttpUtility.UrlEncode(url, Encoding.UTF8);
            return _UrlEncodeParser.Replace(encode, (s) => s.Value.ToUpper());
        }

        public static string GetQQHeadImageUrl(long qq) => "http://q.qlogo.cn/headimg_dl?spec=640&img_type=png&dst_uin=" + qq;

        public static string GetQQGroupHeadImageUrl(long group) => $"http://p.qlogo.cn/gh/{group}/{group}/";

        public static async Task<string> GetShortUrl(string url)
            => await Task.Run(() =>
            {
                if (!string.IsNullOrWhiteSpace(Program.Config.SuoLinkToken))
                {
                    var data = JObject.Parse(HttpGET($"http://api.suolink.cn/api.htm?format=json&key={Program.Config.SuoLinkToken}&expireDate={DateTime.Now.AddDays(3).Date:yyyy-MM-dd}&url=" + UrlEncode(url)));
                    return (string)data["url"];
                }
                return url;
            });

        public static async Task<string> GetOriginalUrl(string url)
            => await Task.Run(() => ExpandShortUrl(url));
        private static string ExpandShortUrl(string shortUrl)
        {
            string nativeUrl = shortUrl;
            try
            {
                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(shortUrl);
                req.AllowAutoRedirect = false;  // 禁止自动跳转
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
                if (response.StatusCode == HttpStatusCode.Found)
                    nativeUrl = response.Headers["Location"];
            }
            catch
            {
                nativeUrl = shortUrl;
            }
            return nativeUrl;
        }

        public static async Task<Stream> GetFileStream(string url, string referer = null)
        {
            var config = new DownloadConfiguration()
            {
                BufferBlockSize = 4096,
                ChunkCount = 5,
                OnTheFlyDownload = false,
                ParallelDownload = true
            };
            if (!string.IsNullOrWhiteSpace(referer))
            {
                config.RequestConfiguration = new RequestConfiguration()
                {
                    Referer = referer
                };
            }
            var downloader = new DownloadService(config);
            return await downloader.DownloadFileTaskAsync(url);
        }

        public static async Task<Stream> GetFileAsync(string url, string referer = null)
        {
            using HttpClient hc = new HttpClient();
            var resp = await hc.GetAsync(url);
            if (!string.IsNullOrEmpty(referer))
                resp.Headers.Add("referer", referer);
            var stream = await resp.Content.ReadAsStreamAsync();
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        /*
        public static async Task<Stream> GetFileAsync(string url, int thread)
        {
            if (!await CheckAllowBreakpoint(url))
            {
                ConsoleLog.Warning("Http", $"The target ({url}) does not support breakpoint continuation. It is processed by single thread operation");
                return await GetFileAsync(url);
            }
            else if (thread < 1)
            {
                ConsoleLog.Warning("Http", $"The number of threads should not be less than 1.");
                return await GetFileAsync(url);
            }
            else
            {
                var length = await GetContentLength(url);
                var threadCount = ((length / thread) < (1024 * 1024)) ? Math.Max(1, length / (1024 * 1024)) : thread;
                var partLength = length / threadCount;
                ArrayList tasks = new();
                List<Stream> streams = new();
                for (var i = 0; i < threadCount; i++)
                {
                    var stream = new MemoryStream();
                    streams.Add(stream);
                    var t = Task.Run(async () =>
                    {
                        var request = CreateHttpWebRequest(url);
                        long total = -1;
                        if (i == threadCount - 1)
                        {
                            request.AddRange(i * partLength, length);
                            total = length - i * partLength;
                        }
                        else
                        {
                            request.AddRange(i * partLength, (i + 1) * partLength - 1);
                            total = (i + 1) * partLength - 1 - i * partLength;
                        }
                        var response = await request.GetResponseAsync();
                        var responseStream = response.GetResponseStream();
                        var buffer = new byte[1024];
                        int osize = -1;
                        while ((osize = stream.Read(buffer, 0, buffer.Length)) > 0)
                            stream.Write(buffer, 0, osize);
                        responseStream.Close();
                        responseStream.Dispose();
                        response.Close();
                        if (stream.Length < total)
                            throw new WebException($"Download thread #{i} failed.");
                    });
                    tasks.Add(t);
                }
                Task.WaitAll((Task[])tasks.ToArray(typeof(Task)));
                MemoryStream ms = new();
                for (var i = 0; i < threadCount; i++)
                {
                    ms.Seek(i * partLength, SeekOrigin.Begin);
                    streams[i].Seek(0, SeekOrigin.Begin);
                    ((MemoryStream)streams[i]).WriteTo(ms);
                    streams[i].Close();
                    streams[i].Dispose();
                }
                return ms;
            }
        }
        */

        public static async Task<bool> CheckAllowBreakpoint(string url)
        {
            var request = CreateHttpWebRequest(url);
            request.AddRange(0, 1);
            request.Timeout = 10000;
            HttpWebResponse response;
            try
            {
                response = (HttpWebResponse)await request.GetResponseAsync();
            }
            catch (WebException ex)
            {
                response = (HttpWebResponse)ex.Response;
            }
            return response != null && response.StatusCode == HttpStatusCode.PartialContent;
        }

        public static HttpWebRequest CreateHttpWebRequest(string url)
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls13 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.ServerCertificateValidationCallback = delegate { return true; };
            return request;
        }

        public static async Task<long> GetContentLength(string url)
        {
            var request = CreateHttpWebRequest(url);
            var response = await request.GetResponseAsync();
            var length = response.ContentLength;
            request.Abort();
            response.Close();
            return length;
        }

        public static string GetUserAgent(string os = "app")
            => os switch
            {
                "app" => "Mozilla/5.0 BiliDroid/5.51.1 (bbcallen@gmail.com)",
                "pc" => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/82.0.4056.0 Safari/537.36 Edg/82.0.431.0",
                _ => "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/74.0.3729.169 Safari/537.36",
            };

        public static void SetHttpHeaders(HttpWebRequest request, string os = "app", string cookie = "")
        {
            request.Accept = "application/json, text/plain, */*";
            request.Headers.Add("Accept-Encoding", "gzip, deflate, br");
            request.Headers.Add("Accept-Language", "zh-CN,zh;q=0.9");
            request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
            request.UserAgent = GetUserAgent(os);
            if (!string.IsNullOrEmpty(cookie))
                request.Headers.Add("cookie", cookie);
        }

        public static string HttpGET(string Url, string postDataStr = "", long timeout = 20000,
            string cookie = "", string referer = "", string origin = "")
        {
            HttpWebRequest request = null;
            try
            {
                request = CreateHttpWebRequest(Url + (string.IsNullOrWhiteSpace(postDataStr) ? "" : ("?" + postDataStr)));
                request.Timeout = (int)timeout;
                SetHttpHeaders(request, "pc", cookie);
                if (!string.IsNullOrWhiteSpace(referer))
                    request.Referer = referer;
                if (!string.IsNullOrWhiteSpace(origin))
                    request.Headers.Add("Origin", origin);
                return HttpGET(request);
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("HTTP", new StringBuilder().Append("Target Url: ")
                    .AppendLine(Url).Append(ConsoleLog.ErrorLogBuilder(e, true)));
            }
            return string.Empty;
        }
        public static string HttpPOST(string Url, string postDataStr, long timeout = 20000,
           string cookie = "", string referer = "", string origin = "", string contentType = "")
        {
            HttpWebRequest request = null;
            try
            {
                request = CreateHttpWebRequest(Url);
                request.Timeout = (int)timeout;
                SetHttpHeaders(request, "pc", cookie);
                if (!string.IsNullOrWhiteSpace(referer))
                    request.Referer = referer;
                if (!string.IsNullOrWhiteSpace(origin))
                    request.Headers.Add("Origin", origin);
                return HttpPOST(request, postDataStr, contentType);
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("HTTP", new StringBuilder().Append("Target Url: ")
                    .AppendLine(Url).Append(ConsoleLog.ErrorLogBuilder(e, true)));
            }
            return string.Empty;
        }
        public static string HttpPUT(string Url, string postDataStr, long timeout = 20000,
           string cookie = "", string referer = "", string origin = "", string contentType = "")
        {
            HttpWebRequest request = null;
            try
            {
                request = CreateHttpWebRequest(Url);
                request.Timeout = (int)timeout;
                SetHttpHeaders(request, "pc", cookie);
                if (!string.IsNullOrWhiteSpace(referer))
                    request.Referer = referer;
                if (!string.IsNullOrWhiteSpace(origin))
                    request.Headers.Add("Origin", origin);
                return HttpPUT(request, postDataStr, contentType);
            }
            catch (Exception e)
            {
                request?.Abort();
                ConsoleLog.Error("HTTP", new StringBuilder().Append("Target Url: ")
                    .AppendLine(Url).Append(ConsoleLog.ErrorLogBuilder(e, true)));
            }
            return string.Empty;
        }

        public static string HttpGET(HttpWebRequest request)
        {
            request.AutomaticDecompression = DecompressionMethods.All;
            request.Method = "GET";
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream rs = response.GetResponseStream();
            using StreamReader sr = new StreamReader(rs, Encoding.UTF8);
            string retString = sr.ReadToEnd();
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }
        public static string HttpPOST(HttpWebRequest request, string content = "", string contentType = "")
        {
            request.AutomaticDecompression = DecompressionMethods.All;
            request.Method = "POST";
            if (!string.IsNullOrWhiteSpace(contentType))
                request.ContentType = contentType;
            request.ContentLength = content.Length;
            byte[] byteResquest = Encoding.UTF8.GetBytes(content);
            using Stream stream = request.GetRequestStream();
            stream.Write(byteResquest, 0, byteResquest.Length);
            stream.Close();
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream rs = response.GetResponseStream();
            using StreamReader sr = new StreamReader(rs, Encoding.UTF8);
            string retString = sr.ReadToEnd();
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }
        public static string HttpPUT(HttpWebRequest request, string content = "", string contentType = "")
        {
            request.AutomaticDecompression = DecompressionMethods.All;
            request.Method = "PUT";
            if (!string.IsNullOrWhiteSpace(contentType))
                request.ContentType = contentType;
            request.ContentLength = content.Length;
            byte[] byteResquest = Encoding.UTF8.GetBytes(content);
            using Stream stream = request.GetRequestStream();
            stream.Write(byteResquest, 0, byteResquest.Length);
            stream.Close();
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            using Stream rs = response.GetResponseStream();
            using StreamReader sr = new StreamReader(rs, Encoding.UTF8);
            string retString = sr.ReadToEnd();
            response.Close();
            response.Dispose();
            request.Abort();
            return retString;
        }

        public static StringBuilder CreateStringBuilder(this string s) => new StringBuilder(s);
    }
}
