using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Netease.CloudMusic.Model
{
    public struct SongUrl
    {
        public long Id { get; init; }

        public int Br { get; init; }

        public long Size { get; init; }

        public string Type { get; init; }

        public string Url { get; init; }

        public SongUrl(JToken data)
        {
            Id = (long)data["id"];
            Br = (int)data["br"];
            Size = (long)data["size"];
            Type = (string)data["type"];
            Url = (string)data["url"];
        }
    }
}
