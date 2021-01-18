using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Tools
{
    public static class Hitokoto
    {
        const string API = "https://v1.hitokoto.cn/";

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

        public static HitokotaObject Get()
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
                CreatedAt = new DateTime(1970, 1, 1, 8, 0, 0, 0).AddSeconds((long)data["created_at"])
            };
        }
    }

    public struct HitokotaObject
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
