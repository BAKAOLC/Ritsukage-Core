using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using QueryArgs = System.Collections.Generic.Dictionary<string, object>;

namespace Ritsukage.Library.Bilibili
{
    internal static class BilibiliWbiSign
    {
        private const string Api_Nav = "https://api.bilibili.com/x/web-interface/nav";

        private static readonly int[] MixinKeyEncTab =
        {
            46, 47, 18, 2, 53, 8, 23, 32, 15, 50, 10, 31, 58, 3, 45, 35, 27, 43, 5, 49,
            33, 9, 42, 19, 29, 28, 14, 39, 12, 38, 41, 13, 37, 48, 7, 16, 24, 55, 40,
            61, 26, 17, 0, 1, 60, 51, 30, 4, 22, 25, 54, 21, 56, 59, 6, 63, 57, 62, 11,
            36, 20, 34, 44, 52,
        };

        private static readonly Lazy<HttpClient> HttpClientInstance = new(() =>
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Add("User-Agent",
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36 Edg/114.0.1823.82");
            return client;
        });

        private static HttpClient HttpClient => HttpClientInstance.Value;

        private static DateTime LastUpdated;

        private static string ImgUrl = string.Empty;
        private static string SubUrl = string.Empty;

        public static async Task<string> GetQueryString(QueryArgs parameters)
        {
            var salt = await GetMixinKey();
            parameters["wts"] = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            parameters = parameters.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value);
            var queryString = Utils.ToUrlParameter(parameters);
            var sign = MD5(queryString + salt);
            parameters["w_rid"] = sign;
            return Utils.ToUrlParameter(parameters);
        }

        private static async Task UpdateSignKey()
        {
            if (DateTime.Now - LastUpdated < TimeSpan.FromMinutes(10))
                return;
            var json = await HttpClient.GetStringAsync(Api_Nav);
            var data = JObject.Parse(json)["data"]!;
            ImgUrl = data["wbi_img"]!["img_url"]!.Value<string>()!;
            ImgUrl = ImgUrl.Replace("https://i0.hdslb.com/bfs/wbi/", string.Empty).Replace(".png", string.Empty);
            SubUrl = data["wbi_img"]!["sub_url"]!.Value<string>()!;
            SubUrl = SubUrl.Replace("https://i0.hdslb.com/bfs/wbi/", string.Empty).Replace(".png", string.Empty);
            LastUpdated = DateTime.Now;
        }

        private static async Task<string> GetMixinKey()
        {
            await UpdateSignKey();
            var original = ImgUrl + SubUrl;
            var mixin = new char[original.Length];
            for (var i = 0; i < MixinKeyEncTab.Length; i++) mixin[i] = original[MixinKeyEncTab[i]];
            return new(mixin, 0, 32);
        }

        private static string MD5(string input)
        {
            var inputBytes = Encoding.UTF8.GetBytes(input);
            var hashBytes = System.Security.Cryptography.MD5.HashData(inputBytes);
            var sb = new StringBuilder();
            foreach (var b in hashBytes) sb.Append(b.ToString("x2"));
            return sb.ToString();
        }
    }
}