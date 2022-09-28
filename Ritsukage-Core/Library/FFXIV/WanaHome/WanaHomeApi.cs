using Newtonsoft.Json.Linq;
using Ritsukage.Library.FFXIV.WanaHome.Enum;
using Ritsukage.Library.FFXIV.WanaHome.Model;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;

namespace Ritsukage.Library.FFXIV.WanaHome
{
    public static class WanaHomeApi
    {
        public const string API_HOST = "https://wanahome.ffxiv.bingyin.org/api/";

        public const string API_GetTerritoryState = API_HOST + "state/";

        public const string API_GetHouseState = API_HOST + "house/";

        static JToken Get(string api, Dictionary<string, object> param = null)
        {
            if (param != null && param.Count > 0)
                api += "?" + Utils.ToUrlParameter(param);
            var result = Utils.HttpGET(api);
            if (!string.IsNullOrWhiteSpace(result))
                return JToken.Parse(result);
            return null;
        }

        public static TerritoryState GetTerritoryState(Server server)
        {
            var data = Get(API_GetTerritoryState, new()
            {
                { "server", (int)server },
                { "type", 0 },
            });
            if (data == null) return null;
            if ((int)data["code"] != 200)
                throw new Exception((string)data["msg"]);
            var onSale = new List<House>();
            foreach (var _house in (JArray)data["onsale"])
            {
                var time = (int)_house["start_sell"];
                onSale.Add(new House()
                {
                    Server = (Server)(int)_house["server"],
                    Territory = (Territory)(int)_house["territory_id"],
                    Ward = (int)_house["ward_id"],
                    Id = (int)_house["house_id"],
                    Price = (int)_house["price"],
                    StartSell = time == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(time),
                    Size = (HouseSize)(int)_house["size"],
                    Owner = (string)_house["owner"],
                });
            }
            var changes = new List<Changes>();
            foreach (var _change in (JArray)data["changes"])
            {
                var _house = _change["house"];
                changes.Add(new Changes()
                {
                    Server = (Server)(int)_house["server"],
                    Territory = (Territory)(int)_house["territory_id"],
                    Ward = (int)_house["ward_id"],
                    Id = (int)_house["house_id"],
                    EventType = (string)_change["event_type"] switch
                    {
                        "change_owner" => EventType.ChangeOwner,
                        "sold" => EventType.Sold,
                        "start_selling" => EventType.StartSelling,
                        "price_reduce" => EventType.PriceReduce,
                        _ => EventType.Unknown
                    },
                    Param1 = (string)_change["param1"],
                    Param2 = (string)_change["param2"],
                    Time = DateTimeOffset.FromUnixTimeSeconds((int)_change["record_time"]),
                });
            }
            return new TerritoryState()
            {
                OnSale = onSale,
                Changes = changes,
                LastUpdate = DateTimeOffset.FromUnixTimeSeconds((int)data["last_update"])
            };
        }

        public static HouseState GetHouseState(Server server, Territory territory, int ward, int id)
        {
            var data = Get(API_GetTerritoryState, new()
            {
                { "server", (int)server },
                { "type", (int)territory },
                { "ward_id", ward },
                { "house_id", id },
            });
            if (data == null) return null;
            if ((int)data["code"] != 200)
                throw new Exception((string)data["msg"]);
            var _house = data["data"];
            var time = (int)_house["start_sell"];
            var house = new House()
            {
                Server = server,
                Territory = territory,
                Ward = ward,
                Id = id,
                Price = (int)_house["price"],
                StartSell = time == 0 ? null : DateTimeOffset.FromUnixTimeSeconds(time),
                Size = (HouseSize)(int)_house["size"],
                Owner = (string)_house["owner"],
            };
            var changes = new List<Changes>();
            foreach (var _change in (JArray)data["changes"])
            {
                _house = data["house"];
                changes.Add(new Changes()
                {
                    Server = (Server)(int)_house["server"],
                    Territory = (Territory)(int)_house["territory_id"],
                    Ward = (int)_house["ward_id"],
                    Id = (int)_house["house_id"],
                    EventType = (string)_change["event_type"] switch
                    {
                        "change_owner" => EventType.ChangeOwner,
                        "sold" => EventType.Sold,
                        "start_selling" => EventType.StartSelling,
                        "price_reduce" => EventType.PriceReduce,
                        _ => EventType.Unknown
                    },
                    Param1 = (string)_change["param1"],
                    Param2 = (string)_change["param2"],
                    Time = DateTimeOffset.FromUnixTimeSeconds((int)_change["record_time"]),
                });
            }
            return new HouseState() { Data = house, Changes = changes };
        }

        static readonly object _lock = new object();
        static Dictionary<string, Server> ServerKeyMaps;
        public static Server MatchServer(string name)
        {
            lock (_lock)
            {
                if (ServerKeyMaps == null)
                {
                    ServerKeyMaps = new Dictionary<string, Server>();
                    foreach (var server in typeof(Server).GetEnumValues() as Server[])
                    {
                        ServerKeyMaps.Add(server.ToString(), server);
                    }
                }
            }
            foreach (var server in ServerKeyMaps)
            {
                if (server.Key.Contains(name))
                {
                    return server.Value;
                }
            }
            return Server.Unknown;
        }

        static readonly Dictionary<Territory, string[]> TerritoryKeyMaps = new Dictionary<Territory, string[]>()
        {
            { Territory.白银乡, new string[] { "白银乡", "黄金港" } },
            { Territory.海雾村, new string[] { "海雾村", "海都" } },
            { Territory.高脚孤丘, new string[] { "高脚孤丘", "沙都" } },
            { Territory.薰衣草苗园, new string[] { "薰衣草苗园", "森都" } },
            { Territory.穹顶皓天, new string[] { "穹顶皓天", "天穹街", "魔都", "伊修加德" } },
        };
        public static Territory MatchTerritory(string name)
        {
            foreach (var territory in TerritoryKeyMaps)
            {
                foreach (var key in territory.Value)
                    if (name == key)
                    {
                        return territory.Key;
                    }
            }
            return Territory.Unknown;
        }
    }
}
