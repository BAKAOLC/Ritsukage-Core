using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ritsukage.Library.Arknights
{
    public static class AnnounceMent
    {
        const string Meta_BaseUrl = "https://ak-conf.hypergryph.com/config/prod/announce_meta/";
        const string Meta_FileName = "announcement.meta.json";
        const string Meta_Android = Meta_BaseUrl + "Android/" + Meta_FileName;
        const string Meta_IOS = Meta_BaseUrl + "IOS/" + Meta_FileName;

        public struct AnnounceMentMeta
        {
            [JsonProperty(PropertyName = "announceId")]
            public string AnnounceId;
            [JsonProperty(PropertyName = "title")]
            public string Title;
            [JsonProperty(PropertyName = "isWebUrl")]
            public string IsWebUrl;
            [JsonProperty(PropertyName = "webUrl")]
            public string WebUrl;
            [JsonProperty(PropertyName = "day")]
            public string Day;
            [JsonProperty(PropertyName = "month")]
            public string Month;
            [JsonProperty(PropertyName = "group")]
            public string Group;
        }

        public static AnnounceMentMeta[] GetAnnounceMents()
        {
            var data = JObject.Parse(Utils.HttpGET(Meta_Android));
            List<AnnounceMentMeta> list = new();
            foreach (var meta in (JArray)data["announceList"])
                list.Add(meta.ToObject<AnnounceMentMeta>());
            return list.ToArray();
        }
    }
}
