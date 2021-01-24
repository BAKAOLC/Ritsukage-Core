using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace Ritsukage.Tools
{
    public static class Hitokoto
    {
        const string API = "https://v1.hitokoto.cn/";
        const string API2 = "http://api.lkblog.net/ws/api.php";

        static readonly Dictionary<string, (string, string)> MessageFrom = new()
        {
            { "a", ("Anime", "动画") },
            { "b", ("Comic", "漫画") },
            { "c", ("Game", "游戏") },
            { "d", ("Novel", "小说") },
            { "e", ("Myself", "原创") },
            { "f", ("Internet", "网络") },
            { "g", ("Other", "其他") },
        };

        public static HitokotoObject Get()
        {
            var data = JObject.Parse(Utils.HttpGET(API));
            return new()
            {
                Id = (int)data["id"],
                Message = (string)data["hitokoto"],
                From = (string)data["from"] ?? string.Empty,
                FromWho = (string)data["from_who"] ?? string.Empty,
                SourceEN = MessageFrom[(string)data["type"]].Item1,
                SourceCN = MessageFrom[(string)data["type"]].Item2,
                Reviewer = (int)data["reviewer"],
                CreatedAt = Utils.GetDateTime((long)data["created_at"])
            };
        }

        public static string GetAnother()
        {
            var data = JObject.Parse(Utils.HttpGET(API2));
            return (string)data["data"];
        }
    }

    public struct HitokotoObject
    {
        public int Id { get; init; }
        public string Message { get; init; }
        public string From { get; init; }
        public string FromWho { get; init; }
        public string SourceEN { get; init; }
        public string SourceCN { get; init; }
        public int Reviewer { get; init; }
        public DateTime CreatedAt { get; init; }

        public override string ToString()
            => new StringBuilder()
            .AppendLine($"『{Message}』")
            .Append($"—— {FromWho}「{From}」")
            .ToString();
    }
}
