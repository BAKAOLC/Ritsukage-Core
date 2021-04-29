using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using Ritsukage.Tools.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Ritsukage.Library.Bilibili.Model
{
    public struct LiveArea
    {
        /// <summary>
        /// 分区ID
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// 分区名称
        /// </summary>
        public string Name { get; init; }

        /// <summary>
        /// 分区拼音
        /// </summary>
        public string Pinyin { get; init; }

        /// <summary>
        /// 主分区ID
        /// </summary>
        public int ParentId { get; init; }

        /// <summary>
        /// 主分区名称
        /// </summary>
        public string ParentName { get; init; }

        public LiveArea(JToken data)
        {
            Id = (int)data["id"];
            Name = (string)data["name"];
            Pinyin = (string)data["pinyin"];
            ParentId = (int)data["parent_id"];
            ParentName = (string)data["parent_name"];
        }

        public override string ToString()
            => $"{ParentName}·{Name}(ID:{Id})";
    }

    public static class LiveAreaList
    {
        static readonly List<LiveArea> AreaList = new();

        public static async Task Refresh()
        {
            await Task.Run(() =>
            {
                try
                {
                    var data = JObject.Parse(Utils.HttpGET("https://api.live.bilibili.com/room/v1/Area/getList?show_pinyin=1"));
                    if ((string)data["message"] == "success")
                    {
                        lock (AreaList)
                        {
                            AreaList.Clear();
                            foreach (var main in (JArray)data["data"])
                                foreach (var area in (JArray)main["list"])
                                    AreaList.Add(new(area));
                        }
                    }
                }
                catch (Exception ex)
                {
                    ConsoleLog.Error("Bilibili Live Area", ConsoleLog.ErrorLogBuilder(ex));
                }
            });
        }

        public static async Task<LiveArea> Get(int id)
        {
            if (AreaList.Count == 0)
                await Refresh();
            return AreaList.Where(x => x.Id == id).FirstOrDefault();
        }

        public static async Task<LiveArea[]> Get(string keyword)
        {
            if (AreaList.Count == 0)
                await Refresh();
            return AreaList.Where(x => x.Name.Contains(keyword) || x.Pinyin.Contains(keyword)).ToArray();
        }
    }
}
