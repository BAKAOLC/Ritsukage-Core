using Newtonsoft.Json.Linq;
using System;

namespace Ritsukage.Library.Roll.Model
{
    public struct HistoryToday
    {
        public string Title { get; init; }
        public string Date { get; init; }

        public HistoryToday(JToken data)
        {
            Title = (string)data["title"];
            Date = $"{data["year"]}年{data["month"]}月{data["day"]}日";
        }

        public override string ToString()
            => Date + "  " + Title;

        public static HistoryToday[] Get()
        {
            var data = RollApi.Get("/history/today");
            if (data.Success)
            {
                var dataArray = (JArray)data.Data;
                var e = new HistoryToday[dataArray.Count];
                for (var i = 0; i < dataArray.Count; i++)
                    e[i] = new HistoryToday(dataArray[i]);
                return e;
            }
            return null;
        }

        static HistoryToday[] _recent;
        static DateTime _recentDate;
        public static HistoryToday[] Today()
        {
            if (_recentDate.Date == DateTime.Today.Date)
                return _recent;
            var data = Get();
            if (data != null && data.Length > 0)
            {
                _recentDate = DateTime.Today.Date;
                return _recent = data;
            }
            throw new Exception("历史上的今天获取失败");
        }
    }
}
