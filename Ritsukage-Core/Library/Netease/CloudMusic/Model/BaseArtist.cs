using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Netease.CloudMusic.Model
{
    public struct BaseArtist
    {
        public long Id { get; init; }
        public string Name { get; init; }

        public BaseArtist(long id, string name)
        {
            Id = id;
            Name = name;
        }
        public BaseArtist(JToken data) : this((long)data["id"], (string)data["name"]) { }

        public override string ToString()
            => Name;
    }
}
