using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace Ritsukage.Library.Netease.CloudMusic.Model
{
    public class SongSearchResult
    {
        public long Id { get; init; }

        public string Name { get; init; }
        
        public BaseArtist[] Artists { get; init; }

        public BaseAlbum Album { get; init; }

        public DateTime PublishTime { get; init; }

        public TimeSpan Duration { get; init; }

        public string Url => "https://music.163.com/#/song?id=" + Id;

        public SongSearchResult(JToken data)
        {
            Id = (long)data["id"];
            Name = (string)data["name"];
            List<BaseArtist> artists = new();
            foreach (var a in (JArray)data["artists"])
                artists.Add(new BaseArtist(a));
            Artists = artists.ToArray();
            Album = new BaseAlbum((long)data["album"]["id"], (string)data["album"]["name"], string.Empty);
            PublishTime = Tools.Utils.GetDateTime((double)data["album"]["publishTime"] / 1000);
            Duration = new TimeSpan(0, 0, (int)data["duration"]);
        }
    }
}
