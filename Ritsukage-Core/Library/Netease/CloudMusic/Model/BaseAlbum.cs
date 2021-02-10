using Newtonsoft.Json.Linq;

namespace Ritsukage.Library.Netease.CloudMusic.Model
{
    public struct BaseAlbum
    {
        public long Id { get; init; }
        public string Name { get; init; }
        public string Pic { get; init; }

        public BaseAlbum(long id, string name, string pic)
        {
            Id = id;
            Name = name;
            Pic = pic;
        }
        public BaseAlbum(JToken data) : this((long)data["id"], (string)data["name"], (string)data["picUrl"]) { }

        public string GetPicUrl(int width, int height)
            => Pic + $"?param={width}y{height}";
    }
}
