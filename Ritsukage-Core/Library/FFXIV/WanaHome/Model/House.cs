using Ritsukage.Library.FFXIV.WanaHome.Enum;
using System;

namespace Ritsukage.Library.FFXIV.WanaHome.Model
{
    public struct House
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
        /// 价格
        /// </summary>
        public int Price { get; init; }

        /// <summary>
        /// 房屋大小
        /// </summary>
        public HouseSize Size { get; init; }

        /// <summary>
        /// 房主
        /// </summary>
        public string Owner { get; init; }

        /// <summary>
        /// 开始贩售时间
        /// </summary>
        public DateTimeOffset? StartSell { get; init; }

        public string HouseName => $"{Territory} {Ward + 1:D2}-{Id + 1:D2}";

        public string SellTimeSpan => StartSell.HasValue ? $"{DateTimeOffset.Now - StartSell.Value:d\\天hh\\时mm\\分ss\\秒}" : string.Empty;
    }
}
