using Ritsukage.Library.FFXIV.WanaHome.Enum;
using System;
using System.Text;

namespace Ritsukage.Library.FFXIV.WanaHome.Model
{
    public struct Changes
    {
        /// <summary>
        /// 服务器
        /// </summary>
        public Server Server { get; init; }

        /// <summary>
        /// 地图
        /// </summary>
        public Territory Territory { get; init; }

        /// <summary>
        /// 房区
        /// </summary>
        public int Ward { get; init; }

        /// <summary>
        /// 房号
        /// </summary>
        public int Id { get; init; }

        /// <summary>
        /// 事件类型
        /// </summary>
        public EventType EventType { get; init; }

        public string Param1 { get; init; }

        public string Param2 { get; init; }

        /// <summary>
        /// 事件发生时间
        /// </summary>
        public DateTimeOffset Time { get; init; }

        public string HouseName => $"{Territory} {Ward + 1:D2}-{Id + 1:D2}";

        public string EventMessage => EventType switch
        {
            EventType.ChangeOwner => $"持有者从 {Param1} 变更为 {Param2}",
            EventType.Sold => $"被 {Param1} 购入（历时{new TimeSpan(1, 0, int.Parse(Param2)):d\\天hh\\时mm\\分ss\\秒}）",
            EventType.StartSelling => $"以 {Param2}Gil 的价格开始出售（原持有人 {Param1}）",
            EventType.PriceReduce => $"从 {Param1}Gil 降价到 {Param2}Gil",
            _ => "未知事件",
        };

        public override string ToString()
            => $"[{Time.ToLocalTime():yyyy年MM月dd日 HH:mm:ss}][{HouseName}] " + EventMessage;
    }
}
