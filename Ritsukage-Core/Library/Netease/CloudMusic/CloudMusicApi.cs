using Newtonsoft.Json.Linq;
using Ritsukage.Library.Netease.CloudMusic.Model;
using System.Collections.Generic;
using System.Threading.Tasks;
using BaseApi = NeteaseCloudMusicApi.CloudMusicApi;
using Providers = NeteaseCloudMusicApi.CloudMusicApiProviders;

namespace Ritsukage.Library.Netease.CloudMusic
{
    public static class CloudMusicApi
    {
        public static async Task<SongSearchResult[]> SearchSong(string key)
        {
            bool success;
            JObject json;
            var api = new BaseApi();
            (success, json) = await api.RequestAsync(Providers.Search, new()
            {
                { "keywords", key }
            });
            if (!success)
                return null;
            try
            {
                List<SongSearchResult> result = new();
                foreach (var s in (JArray)json["result"]["songs"])
                    result.Add(new(s));
                return result.ToArray();
            }
            catch
            { }
            return null;
        }

        public static async Task<SongDetail> GetSongDetail(long id)
        {
            bool success;
            JObject json;
            var api = new BaseApi();
            (success, json) = await api.RequestAsync(Providers.SongDetail, new()
            {
                { "ids", id.ToString() }
            });
            if (!success)
                return null;
            return new(json["songs"][0]);
        }

        public static async Task<SongUrl> GetSongUrl(long id, int br = 999000)
        {
            bool success;
            JObject json;
            var api = new BaseApi();
            (success, json) = await api.RequestAsync(Providers.SongUrl, new()
            {
                { "id", id.ToString() },
                { "br", br.ToString() }
            });
            if (!success)
                return new();
            return new(json["data"][0]);
        }
    }
}
