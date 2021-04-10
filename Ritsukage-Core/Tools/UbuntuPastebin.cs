using System;
using System.IO;
using System.Net;
using System.Text;

namespace Ritsukage.Tools
{
    public static class UbuntuPastebin
    {
        const string Url = "https://paste.ubuntu.com";

        static HttpWebRequest GetWebRequest()
        {
            var wr = Utils.CreateHttpWebRequest(Url);
            wr.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/88.0.4324.190 Safari/537.36";
            wr.Accept = "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9";
            wr.ContentType = "application/x-www-form-urlencoded";
            wr.Referer = Url;
            wr.Timeout = 60000;
            wr.Headers.Add("DNT", "1");
            wr.Headers.Add("Upgrade-Insecure-Requests", "1");
            return wr;
        }

        public static string Paste(string text, string syntax = "text", string poster = "bot")
        {
            var request = GetWebRequest();
            var content = $"poster={poster}&syntax={syntax}&expiration=&content=" + Utils.UrlEncode(text);
            request.AutomaticDecompression = DecompressionMethods.All;
            request.Method = "POST";
            request.ContentLength = content.Length;
            byte[] byteResquest = Encoding.UTF8.GetBytes(content);
            using Stream stream = request.GetRequestStream();
            stream.Write(byteResquest, 0, byteResquest.Length);
            stream.Close();
            using HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            string result = response.ResponseUri.ToString();
            var status = response.StatusCode;
            response.Close();
            response.Dispose();
            request.Abort();
            if (status != HttpStatusCode.OK)
                throw new Exception("paste failed");
            else
                return result;
        }
    }
}
