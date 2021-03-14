using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Ritsukage.Library.Roll.Model
{
    public struct Holiday
    {
        public string Name { get; init; }
        public string Date { get; init; }
        public string LunarDate { get; init; }
        public bool ForLunar { get; init; }
        public int ResidueDays { get; init; }

        public Holiday(JToken data)
        {
            Name = (string)data["holidayName"];
            Date = (string)data["date"];
            LunarDate = (string)data["lunarDate"];
            ForLunar = (bool)data["lunarHoliday"];
            ResidueDays = (int)data["residueDays"];
        }

        public string ResidueDay()
        {
            if (ResidueDays < 0)
                return Math.Abs(ResidueDays) + "日前";
            else if (ResidueDays == 0)
                return "今天";
            else if (ResidueDays == 1)
                return "明天";
            else if (ResidueDays == 2)
                return "后天";
            else
                return Math.Abs(ResidueDays) + "天后";
        }

        public override string ToString()
            => $"{ResidueDay()} {Name}" + (ForLunar ? "  [农历]" : string.Empty);

        public static Holiday[] Get()
        {
            var data = RollApi.Get("/holiday/recent/list");
            if (data.Success)
            {
                var dataArray = (JArray)data.Data;
                var e = new Holiday[dataArray.Count];
                for (var i = 0; i < dataArray.Count; i++)
                    e[i] = new Holiday(dataArray[i]);
                return e;
            }
            return null;
        }

        static Holiday[] _recent;
        static DateTime _recentDate;
        public static Holiday[] Recent()
        {
            if (_recentDate.Date == DateTime.Today.Date)
                return _recent;
            var data = Get();
            if (data != null && data.Length > 0)
            {
                _recentDate = DateTime.Today.Date;
                var today = data.Where(x => x.ResidueDays == 0).ToList();
                int recentDay = data.Where(x => x.ResidueDays > 0).ToArray()[0].ResidueDays;
                var recent = data.Where(x => x.ResidueDays == recentDay).ToList();
                _recent = today.Concat(recent).ToArray();
                recentDay = data.Where(x => x.ResidueDays > recentDay).ToArray()[0].ResidueDays;
                recent = data.Where(x => x.ResidueDays == recentDay).ToList();
                _recent = _recent.Concat(recent).ToArray();
                return _recent;
            }
            throw new Exception("最近节日获取失败");
        }
    }
}
