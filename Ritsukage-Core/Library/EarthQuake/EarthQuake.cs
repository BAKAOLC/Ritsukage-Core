using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Ritsukage.Tools;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Ritsukage.Library.EarthQuake
{
    public static class EarthQuake
    {
        public class EarthQuakeData
        {
            [JsonProperty(PropertyName = "SAVE_TIME", ItemConverterType = typeof(DateTimeConverter))]
            public DateTime 预警时间;
            [JsonProperty(PropertyName = "O_TIME", ItemConverterType = typeof(DateTimeConverter))]
            public DateTime 发生时间;
            [JsonProperty(PropertyName = "EPI_LAT")]
            public double 纬度;
            [JsonProperty(PropertyName = "EPI_LON")]
            public double 经度;
            [JsonProperty(PropertyName = "EPI_DEPTH")]
            public double 深度;
            [JsonProperty(PropertyName = "M")]
            public double 震级;
            [JsonProperty(PropertyName = "LOCATION_C")]
            public string 地区;
            [JsonProperty(PropertyName = "SYNC_TIME")]
            public string 同步时间;

            public override string ToString()
            {
                var sb = new StringBuilder();
                sb.Append("发震时刻：").Append(发生时间.ToString("yyyy-MM-dd HH:mm:ss"));
                sb.AppendLine();
                sb.Append("纬度：").Append(纬度.ToString("F2")).Append('°');
                sb.AppendLine();
                sb.Append("经度：").Append(经度.ToString("F2")).Append('°');
                sb.AppendLine();
                sb.Append("深度：").Append(深度).Append("千米");
                sb.AppendLine();
                sb.Append("震级：").Append(震级.ToString("F1"));
                sb.AppendLine();
                sb.Append("参考位置：").Append(地区);
                return sb.ToString();
            }

            class DateTimeConverter : DateTimeConverterBase
            {
                public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
                {
                    var it = (string)reader.Value;
                    return Convert.ToDateTime(it, new DateTimeFormatInfo()
                    {
                        FullDateTimePattern = "yyyy-MM-dd HH:mm:ss"
                    });
                }

                public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
                {
                    if (value is DateTime dt)
                    {
                        writer.WriteValue(dt.ToString("yyyy-MM-dd HH:mm:ss"));
                    }
                    else
                    {
                        writer.WriteValue(value);
                    }
                }
            }
        }

        public static List<EarthQuakeData> GetData()
        {
            List<EarthQuakeData> result = new();
            var rawData = Utils.HttpGET("http://www.ceic.ac.cn/ajax/speedsearch?num=1");
            if (!string.IsNullOrEmpty(rawData))
            {
                var data = JObject.Parse(rawData.Substring(1, rawData.Length - 2));
                foreach (var eq in (JArray)data["shuju"])
                {
                    result.Add(eq.ToObject<EarthQuakeData>());
                }
                return result;
            }
            return null;
        }
    }
}
