using Newtonsoft.Json.Linq;
using System;
using System.Text;

namespace Ritsukage.Library.Roll.Model
{
    public struct LogisticsType
    {
        public int Id { get; init; }
        public string Name { get; init; }

        public LogisticsType(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => Name;
    }

    public struct Logistics
    {
        public string Id { get; init; }
        public string Type { get; init; }
        public LogisticsStatus Status { get; init; }
        public LogisticsData[] Data { get; init; }

        public string GetFullString()
        {
            var sb = new StringBuilder(ToString()).AppendLine().Append("详细信息：");
            if (Data != null)
            {
                foreach (var data in Data)
                    sb.AppendLine().Append(data.ToString());
            }
            return sb.ToString();
        }

        public override string ToString()
        {
            var sb = new StringBuilder()
                .AppendLine("快递单号：" + Id)
                .AppendLine("快递公司：" + Type)
                .Append("快递状态：" + Status switch
                {
                    LogisticsStatus.OnWay => "投递中",
                    LogisticsStatus.Received => "已签收",
                    _ => "问题邮件"
                });
            return sb.ToString();
        }

        public static LogisticsType[] GetLogisticsType(string id)
        {
            var data = RollApi.Get("/logistics/discern?logistics_no=" + id);
            if (data.Success)
            {
                var dataArray = (JArray)data["searchList"];
                if (dataArray != null)
                {
                    var e = new LogisticsType[dataArray.Count];
                    for (var i = 0; i < dataArray.Count; i++)
                        e[i] = new LogisticsType((int)dataArray[i]["logisticsTypeId"], (string)dataArray[i]["logisticsTypeName"]);
                    return e;
                }
            }
            return Array.Empty<LogisticsType>();
        }

        public static Logistics Get(string id)
        {
            var t = GetLogisticsType(id);
            if (t.Length > 0)
            {
                var data = RollApi.Get($"/logistics/details/search?logistics_no={id}&logistics_id={t[0].Id}");
                if (data.Success)
                {
                    LogisticsData[] _data = null;
                    var dataArray = (JArray)data["data"];
                    if (dataArray != null)
                    {
                        var e = new LogisticsData[dataArray.Count];
                        for (var i = 0; i < dataArray.Count; i++)
                            e[i] = new LogisticsData((string)dataArray[i]["time"], (string)dataArray[i]["desc"]);
                        _data = e;
                    }
                    LogisticsStatus status = (string)data["status"] switch
                    {
                        "在途中" => LogisticsStatus.OnWay,
                        "签收" => LogisticsStatus.Received,
                        _ => LogisticsStatus.Problem
                    };
                    return new()
                    {
                        Id = (string)data["logisticsNo"],
                        Type = (string)data["logisticsType"],
                        Status = status,
                        Data = _data
                    };
                }
            }
            throw new Exception("未能成功获取快递信息");
        }
    }

    public struct LogisticsData
    {
        public string Time { get; init; }
        public string Desc { get; init; }

        public LogisticsData(string time, string desc)
        {
            Time = time;
            Desc = desc;
        }

        public override string ToString()
            => Time + "    " + Desc;
    }

    public enum LogisticsStatus
    {
        OnWay,
        Received,
        Problem
    }
}
